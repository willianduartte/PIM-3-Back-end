using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Abstractions;

/// <summary>Token de acesso gerado e o instante (UTC) em que expira.</summary>
public record TokenGerado(string Token, DateTime ExpiraEm);

/// <summary>Gera o token de acesso (JWT) de um funcionario autenticado.</summary>
public interface ITokenGenerator
{
    TokenGerado Gerar(Funcionario funcionario);
}
