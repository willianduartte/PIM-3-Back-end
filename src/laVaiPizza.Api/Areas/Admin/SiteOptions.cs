namespace laVaiPizza.Api.Areas.Admin;

/// <summary>Configuracao "Site": endereco publico do site, usado para mostrar as fotos do cardapio no admin.</summary>
public class SiteOptions
{
    public const string Secao = "Site";

    /// <summary>Ex.: https://willianduartte.github.io/PIM-4-Front-End/ (as fotos do seed ficam no site, nao na API).</summary>
    public string UrlBase { get; set; } = string.Empty;
}
