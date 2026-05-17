namespace laVaiPizza.Application.Funcionarios;

public record FuncionarioRequest(string Nome, string Cargo, string Telefone, int? LoginId);
public record FuncionarioResponse(int Id, string Nome, string Cargo, string Telefone, int? LoginId);