using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Abstractions;

public interface IFuncionarioRepository
{
    /// <summary>Funcionario (com o Login carregado) pelo e-mail de acesso; nulo se nao existir.</summary>
    Task<Funcionario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Todos os funcionarios com o Login carregado (somente leitura).</summary>
    Task<IReadOnlyList<Funcionario>> GetAllAsync(CancellationToken cancellationToken = default);
}
