namespace Blocktrust.Mediator;

using System.Security.Cryptography.X509Certificates;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;


//These lines are only need if a migration is run

public class DataContext : DbContext
{
    public DbSet<OobEntity> Oobs { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OobEntity>()
            .HasKey(b => b.OobId);
        modelBuilder.Entity<OobEntity>().Property(b => b.OobId).HasValueGenerator(typeof(SequentialGuidValueGenerator));
    }
}