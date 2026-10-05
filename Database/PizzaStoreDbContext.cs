using Microsoft.EntityFrameworkCore;
using PizzaStoreApi.Entities;

namespace PizzaStoreApi.Database;

public class PizzaStoreDbContext : DbContext
{
    public PizzaStoreDbContext(DbContextOptions<PizzaStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pizza> Pizzas => Set<Pizza>();
    public DbSet<Topping> Toppings => Set<Topping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pizza>()
            .HasMany(pizza => pizza.Toppings)
            .WithMany(topping => topping.Pizzas);
    }
}
