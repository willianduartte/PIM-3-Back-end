using System.ComponentModel.DataAnnotations;
using laVaiPizza.Application.Funcionarios;

namespace laVaiPizza.Application.Auth;

/// <summary>
/// Credenciais do funcionario. As anotacoes ficam nos parametros do construtor (exigencia do
/// .NET 8 para records) e sao validadas automaticamente pela API, que responde 400.
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    string Email,
    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100, ErrorMessage = "A senha deve ter no máximo 100 caracteres.")]
    string Senha);

/// <summary>Resposta do login: token JWT, quando expira (UTC) e quem entrou.</summary>
public record LoginResponse(string Token, DateTime ExpiraEm, FuncionarioResponse Funcionario);
