using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace laVaiPizza.Infrastructure.Persistence;

public class LoginRepository : ILoginRepository
{
    private readonly AppDbContext _context;

    public LoginRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Login?> GetByEmailAsync(string email)
    {
        return await _context.Logins
            .FirstOrDefaultAsync(l => l.Email == email);
    }

    public async Task AddAsync(Login login)
    {
        await _context.Logins.AddAsync(login);
        await _context.SaveChangesAsync();
    }
}