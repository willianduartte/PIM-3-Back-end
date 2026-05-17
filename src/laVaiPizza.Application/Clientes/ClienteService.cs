using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Clientes;

public class ClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

    public async Task<IEnumerable<ClienteResponse>> GetClientesAsync()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        return clientes.Select(c => new ClienteResponse(c.Id, c.Nome, c.Telefone ?? "", c.Email ?? "", c.Endereco));
    }

    public async Task<ClienteResponse> CreateClienteAsync(ClienteRequest request)
    {
        var cliente = new Cliente { Nome = request.Nome, Telefone = request.Telefone, Email = request.Email, Endereco = request.Endereco };
        await _clienteRepository.AddAsync(cliente);
        return new ClienteResponse(cliente.Id, cliente.Nome, cliente.Telefone, cliente.Email, cliente.Endereco);
    }
}