namespace laVaiPizza.Domain.Constants;

/// <summary>
/// Cargos dos funcionarios (espelha o CHECK da coluna Funcionario.Cargo).
/// O cargo define as permissoes de acesso (usado como "role" no login).
/// </summary>
public static class Cargos
{
    public const string Gerente = "Gerente";
    public const string Pizzaiolo = "Pizzaiolo";
    public const string Entregador = "Entregador";

    public static readonly IReadOnlyList<string> Todos = new[] { Gerente, Pizzaiolo, Entregador };
}
