namespace laVaiPizza.Domain.Constants;

/// <summary>
/// Tamanhos de pizza (espelha o CHECK da coluna PizzaPreco.Tamanho).
/// </summary>
public static class TamanhosPizza
{
    public const string Pequena = "Pequena";
    public const string Media = "Media";
    public const string Grande = "Grande";

    /// <summary>Do menor para o maior (ordem de exibicao).</summary>
    public static readonly IReadOnlyList<string> Todos = new[] { Pequena, Media, Grande };

    /// <summary>
    /// Converte o texto recebido no tamanho oficial, ignorando maiusculas e acentos
    /// ("media", "Média" e "MEDIA" viram "Media"). Nulo se nao for um tamanho valido.
    /// </summary>
    public static string? Normalizar(string? tamanho)
    {
        if (string.IsNullOrWhiteSpace(tamanho)) return null;

        var semAcento = new string(tamanho.Trim()
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                        != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());

        return Todos.FirstOrDefault(t => string.Equals(t, semAcento, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Posicao do tamanho na ordem de exibicao (tamanho desconhecido vai para o fim).</summary>
    public static int Ordem(string tamanho)
    {
        for (var i = 0; i < Todos.Count; i++)
        {
            if (string.Equals(Todos[i], tamanho, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return Todos.Count;
    }
}
