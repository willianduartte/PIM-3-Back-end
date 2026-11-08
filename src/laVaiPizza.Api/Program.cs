using System.Text;
using System.Threading.RateLimiting;
using laVaiPizza.Api.Infrastructure;
using laVaiPizza.Api.Security;
using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Auth;
using laVaiPizza.Application.Funcionarios;
using laVaiPizza.Application.Pedidos;
using laVaiPizza.Application.Pizzas;
using laVaiPizza.Application.Promocoes;
using laVaiPizza.Application.Relatorios;
using laVaiPizza.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- Configuracao do JWT
// A chave nao fica no repositorio. Desenvolvimento: dotnet user-secrets. Outros ambientes: variavel Jwt__Key.
var jwt = builder.Configuration.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < JwtOptions.TamanhoMinimoChave)
{
    throw new InvalidOperationException(
        $"Jwt:Key nao configurada ou menor que {JwtOptions.TamanhoMinimoChave} caracteres. " +
        "Em desenvolvimento: dotnet user-secrets set \"Jwt:Key\" \"<chave-longa>\" --project src/laVaiPizza.Api. " +
        "Em outros ambientes: defina a variavel de ambiente Jwt__Key.");
}
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Secao));

// ---------------------------------------------------------------- Servicos da aplicacao
builder.Services.AddInfrastructure(builder.Configuration);

// Cache em memoria, usado pelos Services (ex.: cardapio)
builder.Services.AddMemoryCache();

// Services: Scoped = uma instancia por requisicao
builder.Services.AddScoped<PizzaService>();
builder.Services.AddScoped<PromocaoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PizzaAdminService>();
builder.Services.AddScoped<PromocaoAdminService>();
builder.Services.AddScoped<PedidoAdminService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FuncionarioService>();

// Seguranca (implementacoes ficam na Api; a Application so conhece as interfaces)
builder.Services.AddSingleton<ISenhaHasher, IdentityPasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IImagemStorage, ImagemStorageLocal>();

// ---------------------------------------------------------------- Web
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
// Controllers de API (JSON) + paginas Razor da area administrativa
builder.Services.AddControllersWithViews();
builder.Services.Configure<laVaiPizza.Api.Areas.Admin.SiteOptions>(builder.Configuration.GetSection(laVaiPizza.Api.Areas.Admin.SiteOptions.Secao));

// Token antiforgery tambem aceito em cabecalho (usado pelos fetch() da tela de pedidos)
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
builder.Services.AddHealthChecks();

// Erros sempre no formato ProblemDetails (RFC 7807)
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS: o site (HTML/JS) roda em outra origem. Origens permitidas em "Cors:AllowedOrigins".
const string politicaCors = "SitePublico";
var origensPermitidas = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
    options.AddPolicy(politicaCors, policy => policy
        .WithOrigins(origensPermitidas)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Limite de tentativas de login por IP (protege contra tentativa e erro de senha)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    // Pedido e acompanhamento sao publicos (sem login): limite por IP contra abuso
    options.AddPolicy("pedido", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    // O carrinho recalcula a cada mudanca de quantidade, entao o limite e maior
    options.AddPolicy("simular", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("acompanhar", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// ---------------------------------------------------------------- Autenticacao e autorizacao
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mantem os nomes das claims como estao no token ("role", "name"...).
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            RoleClaimType = JwtClaims.Role,
            NameClaimType = JwtClaims.Name,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    })
    // Login por cookie das paginas /admin (o JWT continua sendo o padrao da API)
    .AddAdminCookie();

builder.Services.AddAuthorization();

// ---------------------------------------------------------------- Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LaVaiPizza API",
        Version = "v1",
        Description = "Back-end para o sistema da Pizzaria LaVaiPizza. " +
                      "Para testar as rotas protegidas: faça login em /api/v1/auth/login, copie o token e clique em Authorize."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole somente o token JWT (o Swagger acrescenta o prefixo Bearer)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------------------------------------------------------------- Pipeline
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Atalho: a raiz abre o Swagger
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    // Em desenvolvimento nao redireciona, para o site chamar http://localhost sem problema de CORS.
    app.UseHttpsRedirection();
}

// Cultura invariante em todas as requisicoes: numeros e datas dos formularios (type="number" envia
// "34.50") sao lidos do mesmo jeito em qualquer computador. Sem isto, um servidor Windows em pt-BR
// leria "34.50" como 3450. Na tela, os valores sao formatados em pt-BR explicitamente (Formatos).
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(System.Globalization.CultureInfo.InvariantCulture),
    SupportedCultures = new[] { System.Globalization.CultureInfo.InvariantCulture },
    SupportedUICultures = new[] { System.Globalization.CultureInfo.InvariantCulture },
    RequestCultureProviders = new List<Microsoft.AspNetCore.Localization.IRequestCultureProvider>()
});

// Cabecalhos de seguranca das paginas /admin. A politica de conteudo (CSP) so deixa carregar
// scripts e estilos do proprio servidor; imagens tambem podem vir do site (fotos do cardapio).
var origemSite = Uri.TryCreate(builder.Configuration["Site:UrlBase"], UriKind.Absolute, out var uriSite)
    ? uriSite.GetLeftPart(UriPartial.Authority)
    : string.Empty;
app.Use(async (contexto, proximo) =>
{
    if (contexto.Request.Path.StartsWithSegments("/admin"))
    {
        var h = contexto.Response.Headers;
        h["Content-Security-Policy"] =
            $"default-src 'self'; img-src 'self' data: {origemSite}; script-src 'self'; style-src 'self'; " +
            "frame-ancestors 'none'; form-action 'self'; base-uri 'self'";
        h["X-Content-Type-Options"] = "nosniff";
        h["X-Frame-Options"] = "DENY";
        h["Referrer-Policy"] = "same-origin";
        // Paginas do admin nao ficam no cache do navegador (mesmo valor que o antiforgery usa)
        h["Cache-Control"] = "no-cache, no-store";
        h["Pragma"] = "no-cache";
    }

    await proximo();
});

// Arquivos de wwwroot: fotos enviadas pelo admin (/uploads), Bootstrap, Chart.js e scripts do admin
app.UseStaticFiles();

app.UseCors(politicaCors);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

