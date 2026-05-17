namespace laVaiPizza.Application.Clientes;

public record ClienteRequest(string Nome, string Telefone, string Email, string Endereco);
public record ClienteResponse(int Id, string Nome, string Telefone, string Email, string Endereco);