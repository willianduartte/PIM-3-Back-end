using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Funcionarios;

namespace laVaiPizza.Application.Auth;

/// <summary>Autentica o funcionario (e-mail + senha) e entrega o token de acesso.</summary>
public class AuthService
{
    // Hash valido (senha do seed do Gerente). Quando o e-mail nao existe, a senha e verificada
    // contra ele so para o tempo de resposta ser igual ao de um e-mail existente, e a resposta
    // nao revelar quais e-mails estao cadastrados. O resultado e ignorado nesse caso.
    private const string HashParaEqualizarTempo =
        "AQAAAAIAAYagAAAAEBfaHE4+zBRjvVJDlzOGF3ngrZ8UJXgEauDqUetEmZQsOgmJKLkdoYhGm8Y+qEww3A==";

    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthService(
        IFuncionarioRepository funcionarioRepository,
        ISenhaHasher senhaHasher,
        ITokenGenerator tokenGenerator)
    {
        _funcionarioRepository = funcionarioRepository;
        _senhaHasher = senhaHasher;
        _tokenGenerator = tokenGenerator;
    }

    /// <returns>A resposta com o token, ou nulo se e-mail ou senha estiverem incorretos.</returns>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var funcionario = await _funcionarioRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);

        var hash = funcionario?.Login.SenhaHash ?? HashParaEqualizarTempo;
        var senhaConfere = _senhaHasher.Verificar(hash, request.Senha);

        if (funcionario is null || !senhaConfere)
            return null;

        var token = _tokenGenerator.Gerar(funcionario);

        return new LoginResponse(
            token.Token,
            token.ExpiraEm,
            new FuncionarioResponse(funcionario.Id, funcionario.Nome, funcionario.Login.Email, funcionario.Cargo));
    }
}
