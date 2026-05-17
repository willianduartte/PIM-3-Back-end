using laVaiPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace laVaiPizza.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pizza> Pizzas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Login> Logins { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoPizza> PedidoPizzas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pizza>(entity =>
        {
            entity.ToTable("pizza");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id_pizza");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Tamanho).HasColumnName("tamanho");
            entity.Property(e => e.Preco).HasColumnName("preco");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("cliente");
            entity.Property(e => e.Id).HasColumnName("id_cliente");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Telefone).HasColumnName("telefone");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Endereco).HasColumnName("endereco");
        });

        modelBuilder.Entity<Login>(entity =>
        {
            entity.ToTable("login");
            entity.Property(e => e.Id).HasColumnName("id_login");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Senha).HasColumnName("senha");
            entity.Property(e => e.Nome).HasColumnName("nome");
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.ToTable("funcionario");
            entity.Property(e => e.Id).HasColumnName("id_funcionario");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Cargo).HasColumnName("cargo");
            entity.Property(e => e.Telefone).HasColumnName("telefone");
            entity.Property(e => e.LoginId).HasColumnName("id_login");

            entity.HasOne(f => f.Login).WithOne(l => l.Funcionario).HasForeignKey<Funcionario>(f => f.LoginId);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("pedido_entrega");
            entity.Property(e => e.Id).HasColumnName("id_pedido");
            entity.Property(e => e.DataHora).HasColumnName("data_hora");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.ValorTotal).HasColumnName("valor_total");
            entity.Property(e => e.Endereco).HasColumnName("endereco");
            entity.Property(e => e.TempoEstimado).HasColumnName("tempo_estimado");
            entity.Property(e => e.ClienteId).HasColumnName("id_cliente");
            entity.Property(e => e.FuncionarioPreparaId).HasColumnName("id_funcionario_prepara");
            entity.Property(e => e.FuncionarioEntregaId).HasColumnName("id_funcionario_entrega");
        });

        modelBuilder.Entity<PedidoPizza>(entity =>
        {
            entity.ToTable("contem");
            entity.HasKey(pp => new { pp.PedidoId, pp.PizzaId });

            entity.Property(pp => pp.PedidoId).HasColumnName("id_pedido");
            entity.Property(pp => pp.PizzaId).HasColumnName("id_pizza");
            entity.Property(pp => pp.Quantidade).HasColumnName("quantidade");

            entity.HasOne(pp => pp.Pedido)
                  .WithMany(p => p.PedidoPizzas)
                  .HasForeignKey(pp => pp.PedidoId);

            entity.HasOne(pp => pp.Pizza)
                  .WithMany(p => p.PedidoPizzas)
                  .HasForeignKey(pp => pp.PizzaId);
        });

        base.OnModelCreating(modelBuilder);
    }
}