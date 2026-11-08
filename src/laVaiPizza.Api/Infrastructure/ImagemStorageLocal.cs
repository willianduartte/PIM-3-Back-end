using laVaiPizza.Application.Abstractions;

namespace laVaiPizza.Api.Infrastructure;

/// <summary>
/// Guarda as fotos em wwwroot/uploads (servidas como arquivos estaticos em /uploads/...).
/// O nome do arquivo e sempre gerado pelo servidor (GUID): o nome enviado pelo cliente nunca e usado.
/// </summary>
public class ImagemStorageLocal : IImagemStorage
{
    private const string PrefixoUrl = "/uploads/";
    private readonly string _pasta;

    public ImagemStorageLocal(IWebHostEnvironment environment)
    {
        var raiz = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _pasta = Path.Combine(raiz, "uploads");
    }

    public async Task<string> SalvarAsync(Stream conteudo, string extensao, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_pasta);

        var nome = $"{Guid.NewGuid():N}{extensao}";
        await using var arquivo = File.Create(Path.Combine(_pasta, nome));
        await conteudo.CopyToAsync(arquivo, cancellationToken);

        return PrefixoUrl + nome;
    }

    public void Remover(string? imagemUrl)
    {
        if (string.IsNullOrWhiteSpace(imagemUrl) || !imagemUrl.StartsWith(PrefixoUrl, StringComparison.Ordinal))
            return;

        // GetFileName impede "../" no caminho
        var caminho = Path.Combine(_pasta, Path.GetFileName(imagemUrl));
        if (File.Exists(caminho))
            File.Delete(caminho);
    }
}
