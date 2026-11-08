namespace laVaiPizza.Application.Funcionarios;

/// <summary>Dados publicos do funcionario (nunca inclui o hash da senha).</summary>
public record FuncionarioResponse(int Id, string Nome, string Email, string Cargo);
