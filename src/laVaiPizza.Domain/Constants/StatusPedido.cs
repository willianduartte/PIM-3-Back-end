namespace laVaiPizza.Domain.Constants;

/// <summary>
/// Status do pedido, na ordem do fluxo (espelha o CHECK da coluna Pedido.Status).
/// Em Preparacao -> Aguardando Coleta (pizzaiolo) -> Saiu para Entrega -> Entregue (entregador).
/// </summary>
public static class StatusPedido
{
    public const string EmPreparacao = "Em Preparação";
    public const string AguardandoColeta = "Aguardando Coleta";
    public const string SaiuParaEntrega = "Saiu para Entrega";
    public const string Entregue = "Entregue";

    public static readonly IReadOnlyList<string> Fluxo = new[]
    {
        EmPreparacao, AguardandoColeta, SaiuParaEntrega, Entregue
    };
}
