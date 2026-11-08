namespace laVaiPizza.Application.Abstractions;

/// <summary>Guarda as fotos das pizzas enviadas pelo admin (a implementacao fica na Api).</summary>
public interface IImagemStorage
{
    /// <summary>Salva o arquivo e devolve o caminho publico (ex.: /uploads/abc123.jpg).</summary>
    Task<string> SalvarAsync(Stream conteudo, string extensao, CancellationToken cancellationToken = default);

    /// <summary>Apaga uma foto enviada antes. Ignora caminhos que nao sejam de /uploads/.</summary>
    void Remover(string? imagemUrl);
}
