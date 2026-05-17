using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Abstractions;

public interface ILoginRepository
{
    Task<Login?> GetByEmailAsync(string email);
    Task AddAsync(Login login);
}