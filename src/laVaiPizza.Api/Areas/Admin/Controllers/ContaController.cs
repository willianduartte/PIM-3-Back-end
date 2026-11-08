using System.Security.Claims;
using laVaiPizza.Api.Areas.Admin.Models;
using laVaiPizza.Api.Security;
using laVaiPizza.Application.Auth;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace laVaiPizza.Api.Areas.Admin.Controllers;

/// <summary>Entrar, sair e acesso negado. Usa o mesmo AuthService do login da API (JWT).</summary>
[Route("admin/conta")]
public class ContaController : AdminControllerBase
{
    private readonly AuthService _authService;

    public ContaController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet("entrar")]
    public async Task<IActionResult> Entrar(string? returnUrl)
    {
        var atual = await HttpContext.AuthenticateAsync(AdminAuth.Scheme);
        if (atual.Succeeded) return Redirect("/admin");

        return View(new EntrarViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("entrar")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Entrar(EntrarViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resposta = await _authService.LoginAsync(new LoginRequest(modelo.Email, modelo.Senha), ct);
        if (resposta is null)
        {
            // Mesma mensagem para e-mail inexistente e senha errada
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            modelo.Senha = string.Empty;
            return View(modelo);
        }

        var f = resposta.Funcionario;
        var identidade = new ClaimsIdentity(
            new[]
            {
                new Claim(JwtClaims.FuncionarioId, f.Id.ToString()),
                new Claim(JwtClaims.Name, f.Nome),
                new Claim("email", f.Email),
                new Claim(JwtClaims.Role, f.Cargo)
            },
            AdminAuth.Scheme, JwtClaims.Name, JwtClaims.Role);

        await HttpContext.SignInAsync(AdminAuth.Scheme, new ClaimsPrincipal(identidade));

        // So volta para enderecos do proprio site (evita redirecionamento para sites de terceiros)
        return Url.IsLocalUrl(modelo.ReturnUrl) ? LocalRedirect(modelo.ReturnUrl!) : Redirect("/admin");
    }

    [HttpPost("sair")]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(AdminAuth.Scheme);
        return RedirectToAction(nameof(Entrar));
    }

    [HttpGet("negado")]
    public IActionResult Negado() => View();
}

/// <summary>/admin leva cada cargo para a sua tela inicial.</summary>
[Route("admin")]
public class HomeController : AdminControllerBase
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return User.IsInRole(Cargos.Gerente)
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Pedidos");
    }
}
