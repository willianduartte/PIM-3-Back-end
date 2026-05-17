using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Funcionarios;

public class FuncionarioService
{
    private readonly IFuncionarioRepository _repository;

    public FuncionarioService(IFuncionarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FuncionarioResponse>> GetFuncionariosAsync()
    {
        var lista = await _repository.GetAllAsync();
        return lista.Select(f => new FuncionarioResponse(f.Id, f.Nome, f.Cargo, f.Telefone ?? "", f.LoginId));
    }

    public async Task<FuncionarioResponse> CreateFuncionarioAsync(FuncionarioRequest request)
    {
        var funcionario = new Funcionario
        {
            Nome = request.Nome,
            Cargo = request.Cargo,
            Telefone = request.Telefone,
            LoginId = request.LoginId
        };

        await _repository.AddAsync(funcionario);

        return new FuncionarioResponse(funcionario.Id, funcionario.Nome, funcionario.Cargo, funcionario.Telefone, funcionario.LoginId);
    }

    public async Task DeleteFuncionarioAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}