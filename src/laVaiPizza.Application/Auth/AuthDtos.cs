namespace laVaiPizza.Application.Auth;

public record LoginRequest(string Email, string Senha);
public record RegisterRequest(string Email, string Senha, string Nome);
public record AuthResponse(int Id, string Nome, string Email);