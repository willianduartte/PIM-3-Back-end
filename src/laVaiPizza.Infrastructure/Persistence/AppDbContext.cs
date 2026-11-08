using laVaiPizza.Application.Relatorios;
using laVaiPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace laVaiPizza.Infrastructure.Persistence;

/// <summary>
/// Mapeamento das entidades para o banco criado pelo script_banco.sql (database-first,
/// sem migrations). Nomes de tabela e coluna seguem as entidades (PascalCase); aqui
/// ficam so as chaves compostas, relacionamentos, precisao dos decimais e os triggers.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Login> Logins => Set<Login>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pizza> Pizzas => Set<Pizza>();
    public DbSet<PizzaPreco> PizzaPrecos => Set<PizzaPreco>();
    public DbSet<Promocao> Promocoes => Set<Promocao>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<HistoricoPreco> HistoricoPrecos => Set<HistoricoPreco>();
    public DbSet<PedidoStatusHistorico> PedidoStatusHistoricos => Set<PedidoStatusHistorico>();

    /// <summary>
    /// Precisao padrao dos decimais que nao sao colunas de tabela (ex.: resultados das procedures
    /// de relatorio, lidos com SqlQuery). As colunas mapeadas abaixo continuam com HasPrecision(10, 2).
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Resultados das procedures de relatorio: tipos sem chave e sem tabela, so para leitura
        // com Database.SqlQuery. Registrados aqui para receberem a convencao de precisao acima.
        modelBuilder.Entity<ResumoDashboard>().HasNoKey().ToView(null);
        modelBuilder.Entity<FaturamentoMensal>().HasNoKey().ToView(null);
        modelBuilder.Entity<FaturamentoDiario>().HasNoKey().ToView(null);
        modelBuilder.Entity<RankingPizza>().HasNoKey().ToView(null);
        modelBuilder.Entity<VendaPorTamanho>().HasNoKey().ToView(null);
        modelBuilder.Entity<PedidosPorStatus>().HasNoKey().ToView(null);
        modelBuilder.Entity<HorarioPico>().HasNoKey().ToView(null);
        modelBuilder.Entity<VendaPorPizza>().HasNoKey().ToView(null);

        modelBuilder.Entity<Login>(entity =>
        {
            entity.ToTable("Login");
            entity.HasOne(l => l.Funcionario)
                  .WithOne(f => f.Login)
                  .HasForeignKey<Funcionario>(f => f.LoginId);
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.ToTable("Funcionario");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
        });

        // O EF Core precisa saber dos triggers: sem isso o INSERT/UPDATE usa a clausula
        // OUTPUT, que o SQL Server recusa em tabelas com trigger.
        modelBuilder.Entity<Pizza>(entity =>
        {
            entity.ToTable("Pizza", t => t.HasTrigger("trg_ProtegerPizzas"));
        });

        modelBuilder.Entity<PizzaPreco>(entity =>
        {
            entity.ToTable("PizzaPreco", t => t.HasTrigger("trg_AuditoriaPreco"));
            entity.HasKey(p => new { p.PizzaId, p.Tamanho });
            entity.Property(p => p.Preco).HasPrecision(10, 2);
            entity.HasOne(p => p.Pizza)
                  .WithMany(p => p.Precos)
                  .HasForeignKey(p => p.PizzaId);
        });

        modelBuilder.Entity<Promocao>(entity =>
        {
            entity.ToTable("Promocao");
            entity.Property(p => p.Valor).HasPrecision(10, 2);
            entity.Property(p => p.ValorMinimo).HasPrecision(10, 2);
            entity.HasOne(p => p.Pizza)
                  .WithMany()
                  .HasForeignKey(p => p.PizzaId);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedido", t => t.HasTrigger("trg_HistoricoStatusPedido"));
            entity.Property(p => p.Subtotal).HasPrecision(10, 2);
            entity.Property(p => p.Desconto).HasPrecision(10, 2);
            entity.Property(p => p.TaxaEntrega).HasPrecision(10, 2);
            entity.Property(p => p.Total).HasPrecision(10, 2);

            entity.HasOne(p => p.Cliente)
                  .WithMany(c => c.Pedidos)
                  .HasForeignKey(p => p.ClienteId);
            entity.HasOne(p => p.FuncionarioPrepara)
                  .WithMany()
                  .HasForeignKey(p => p.FuncionarioPreparaId);
            entity.HasOne(p => p.FuncionarioEntrega)
                  .WithMany()
                  .HasForeignKey(p => p.FuncionarioEntregaId);
        });

        modelBuilder.Entity<ItemPedido>(entity =>
        {
            entity.ToTable("ItemPedido");
            entity.HasKey(i => new { i.PedidoId, i.PizzaId, i.Tamanho });
            entity.Property(i => i.PrecoUnitario).HasPrecision(10, 2);
            entity.HasOne(i => i.Pedido)
                  .WithMany(p => p.Itens)
                  .HasForeignKey(i => i.PedidoId);
            entity.HasOne(i => i.Pizza)
                  .WithMany()
                  .HasForeignKey(i => i.PizzaId);
        });

        modelBuilder.Entity<HistoricoPreco>(entity =>
        {
            entity.ToTable("HistoricoPreco");
            entity.Property(h => h.PrecoAntigo).HasPrecision(10, 2);
            entity.Property(h => h.PrecoNovo).HasPrecision(10, 2);
            entity.HasOne(h => h.Pizza)
                  .WithMany()
                  .HasForeignKey(h => h.PizzaId);
            entity.HasOne(h => h.AlteradoPorFuncionario)
                  .WithMany()
                  .HasForeignKey(h => h.AlteradoPorFuncionarioId);
        });

        modelBuilder.Entity<PedidoStatusHistorico>(entity =>
        {
            entity.ToTable("PedidoStatusHistorico");
            entity.HasOne(h => h.Pedido)
                  .WithMany(p => p.HistoricoStatus)
                  .HasForeignKey(h => h.PedidoId);
            entity.HasOne(h => h.Funcionario)
                  .WithMany()
                  .HasForeignKey(h => h.FuncionarioId);
        });
    }
}
