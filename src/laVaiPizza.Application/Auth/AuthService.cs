using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Auth;

public class AuthService
{
    private readonly ILoginRepository _repository;

    public AuthService(ILoginRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var login = await _repository.GetByEmailAsync(request.Email);

        // Verificação simples de senha (para estudo escolar)
        if (login == null || login.Senha != request.Senha)
            return null;

        return new AuthResponse(login.Id, login.Nome, login.Email);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var login = new Login
        {
            Email = request.Email,
            Senha = request.Senha,
            Nome = request.Nome
        };

        await _repository.AddAsync(login);

        return new AuthResponse(login.Id, login.Nome, login.Email);
    }
}