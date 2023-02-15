namespace Blocktrust.Mediator;

using System.Security.Cryptography.X509Certificates;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

public class DataContext : DbContext
{
    public DbSet<Oob> Oobs { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Oob>().Property(b => b.OobId).HasValueGenerator(typeof(SequentialGuidValueGenerator));
    }
}