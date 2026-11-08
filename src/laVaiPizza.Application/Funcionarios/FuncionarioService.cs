using laVaiPizza.Application.Abstractions;

namespace laVaiPizza.Application.Funcionarios;

public class FuncionarioService
{
    private readonly IFuncionarioRepository _funcionarioRepository;

    public FuncionarioService(IFuncionarioRepository funcionarioRepository)
    {
        _funcionarioRepository = funcionarioRepository;
    }

    public async Task<IReadOnlyList<FuncionarioResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var funcionarios = await _funcionarioRepository.GetAllAsync(cancellationToken);

        return funcionarios
            .Select(f => new FuncionarioResponse(f.Id, f.Nome, f.Login.Email, f.Cargo))
            .ToList();
    }
}
