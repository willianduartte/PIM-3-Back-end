# syntax=docker/dockerfile:1
#
# Imagem da API LaVaiPizza (API + site de administracao em Razor).
# Duas etapas: a primeira compila com o SDK; a segunda leva so o resultado para a imagem de
# execucao, que e menor e roda sem privilegios de administrador.

# ---------------------------------------------------------------- compilacao
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia primeiro so os .csproj: o restore (lento) fica em cache enquanto os pacotes nao mudam
COPY src/laVaiPizza.Domain/laVaiPizza.Domain.csproj               src/laVaiPizza.Domain/
COPY src/laVaiPizza.Application/laVaiPizza.Application.csproj     src/laVaiPizza.Application/
COPY src/laVaiPizza.Infrastructure/laVaiPizza.Infrastructure.csproj src/laVaiPizza.Infrastructure/
COPY src/laVaiPizza.Api/laVaiPizza.Api.csproj                     src/laVaiPizza.Api/
RUN dotnet restore src/laVaiPizza.Api/laVaiPizza.Api.csproj

COPY src/ src/
RUN dotnet publish src/laVaiPizza.Api/laVaiPizza.Api.csproj \
      -c Release -o /app/publish --no-restore -p:UseAppHost=false

# ---------------------------------------------------------------- execucao
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Pasta das fotos enviadas pelo admin (no compose ela vira um volume, para nao perder as fotos)
RUN mkdir -p /app/wwwroot/uploads && chown -R app:app /app/wwwroot/uploads

# Usuario "app" ja existe na imagem oficial; a API nao precisa de root
USER app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Segredos (Jwt__Key, ConnectionStrings__DefaultConnection) entram por variaveis de ambiente
ENTRYPOINT ["dotnet", "laVaiPizza.Api.dll"]
