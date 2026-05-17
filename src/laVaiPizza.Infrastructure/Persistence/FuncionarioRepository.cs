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

    public async Task<IEnumerable<Funcionario>> GetAllAsync()
    {
        return await _context.Funcionarios.ToListAsync();
    }

    public async Task<Funcionario?> GetByIdAsync(int id)
    {
        return await _context.Funcionarios
            .Include(f => f.Login)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task AddAsync(Funcionario funcionario)
    {
        await _context.Funcionarios.AddAsync(funcionario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Funcionario funcionario)
    {
        _context.Funcionarios.Update(funcionario);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var funcionario = await GetByIdAsync(id);
        if (funcionario != null)
        {
            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();
        }
    }
}