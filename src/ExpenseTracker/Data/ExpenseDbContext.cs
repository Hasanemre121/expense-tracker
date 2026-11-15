using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data;

public class ExpenseDbContext : DbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }

    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Description).IsRequired().HasMaxLength(200);
            e.Property(x => x.Amount).HasConversion<double>();
            e.Property(x => x.Category).HasConversion<string>();
        });
    }
}
