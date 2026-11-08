namespace laVaiPizza.Api.Security;

/// <summary>Nomes das claims do token (mesmo nome na emissao e na validacao).</summary>
public static class JwtClaims
{
    /// <summary>Cargo do funcionario (Gerente, Pizzaiolo, Entregador) = papel usado em [Authorize(Roles = ...)].</summary>
    public const string Role = "role";

    public const string Name = "name";
    public const string FuncionarioId = "funcionarioId";
}
