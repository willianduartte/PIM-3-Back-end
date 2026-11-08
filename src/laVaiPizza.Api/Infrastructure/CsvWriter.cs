using System.Globalization;
using System.Text;

namespace laVaiPizza.Api.Infrastructure;

/// <summary>
/// Gera CSV para abrir direto no Excel em portugues: separador ponto e virgula, numeros com
/// virgula decimal, datas dd/MM/aaaa e UTF-8 com BOM (acentos corretos).
/// Textos que comecam com = + - @ ganham um apostrofo para o Excel nao executar como formula.
/// </summary>
public static class CsvWriter
{
    private static readonly CultureInfo Cultura = new("pt-BR");

    public static byte[] Gerar<T>(IEnumerable<T> linhas) => Gerar(linhas, typeof(T));

    /// <summary>Versao para quando o tipo da linha so e conhecido em tempo de execucao.</summary>
    public static byte[] Gerar(System.Collections.IEnumerable linhas, Type tipoLinha)
    {
        var propriedades = tipoLinha.GetProperties();
        var sb = new StringBuilder();

        sb.AppendJoin(';', propriedades.Select(p => p.Name)).Append("\r\n");

        foreach (var linha in linhas)
        {
            sb.AppendJoin(';', propriedades.Select(p => Formatar(p.GetValue(linha)))).Append("\r\n");
        }

        var preambulo = Encoding.UTF8.GetPreamble();
        var corpo = Encoding.UTF8.GetBytes(sb.ToString());
        return preambulo.Concat(corpo).ToArray();
    }

    private static string Formatar(object? valor)
    {
        var texto = valor switch
        {
            null => string.Empty,
            DateOnly d => d.ToString("dd/MM/yyyy", Cultura),
            DateTime d => d.ToString("dd/MM/yyyy HH:mm", Cultura),
            IFormattable f => f.ToString(null, Cultura),
            _ => valor.ToString() ?? string.Empty
        };

        if (valor is string && texto.Length > 0 && "=+-@".Contains(texto[0]))
            texto = "'" + texto;

        // Aspas duplicadas e campo entre aspas quando tiver separador, aspas ou quebra de linha
        if (texto.IndexOfAny(new[] { ';', '"', '\r', '\n' }) >= 0)
            texto = "\"" + texto.Replace("\"", "\"\"") + "\"";

        return texto;
    }
}
