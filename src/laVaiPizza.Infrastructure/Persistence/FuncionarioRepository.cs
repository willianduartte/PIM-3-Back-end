using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace laVaiPizza.Infrastructure.Persistence;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly AppDbContext _context;

    public FuncionarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Funcionario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // O E-mail e unico (UQ_Login_Email) e a comparacao ignora maiusculas (collation do banco).
        return await _context.Funcionarios
            .AsNoTracking()
            .Include(f => f.Login)
            .FirstOrDefaultAsync(f => f.Login.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyList<Funcionario>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Funcionarios
            .AsNoTracking()
            .Include(f => f.Login)
            .OrderBy(f => f.Cargo)
            .ThenBy(f => f.Nome)
            .ToListAsync(cancellationToken);
    }
}
