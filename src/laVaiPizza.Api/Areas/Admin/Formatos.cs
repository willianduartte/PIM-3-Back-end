using System.Globalization;

namespace laVaiPizza.Api.Areas.Admin;

/// <summary>Formatacao de valores nas telas do admin (sempre em portugues do Brasil).</summary>
public static class Formatos
{
    private static readonly CultureInfo Cultura = new("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", Cultura);

    public static string Percentual(decimal valor) => valor.ToString("0.0", Cultura) + "%";

    public static string DataHora(DateTime data) => data.ToString("dd/MM/yyyy HH:mm", Cultura);

    /// <summary>Valor para campos type="number" (sempre com ponto, independente do idioma).</summary>
    public static string Numero(decimal valor) => valor.ToString("0.00", CultureInfo.InvariantCulture);

    /// <summary>Valor para campos type="datetime-local".</summary>
    public static string CampoDataHora(DateTime data) => data.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    /// <summary>
    /// Fotos enviadas pelo admin ("/uploads/...") estao na propria API; as do cardapio original
    /// ("assets/images/...") estao no site.
    /// </summary>
    public static string UrlImagem(string? url, string siteBase)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        if (url.StartsWith('/') || url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
        return siteBase.TrimEnd('/') + "/" + url;
    }
}
