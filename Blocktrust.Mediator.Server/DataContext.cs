namespace Blocktrust.Mediator.Server;

using System.Security.Cryptography.X509Certificates;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

//These lines are only need if a migration is run

public class DataContext : DbContext
{
    public DbSet<OobInvitationEntity> OobInvitations { get; set; }
    public DbSet<SecretEntity> Secrets { get; set; }
    public DbSet<MediatorConnectionEntity> Connections { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OobInvitationEntity>()
            .HasKey(b => b.OobId);
        modelBuilder.Entity<OobInvitationEntity>().Property(b => b.OobId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<SecretEntity>()
            .HasKey(b => b.SecretId);
        modelBuilder.Entity<SecretEntity>().Property(b => b.SecretId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<MediatorConnectionEntity>()
            .HasKey(b => b.MediatorConnectionId);
        modelBuilder.Entity<MediatorConnectionEntity>().Property(b => b.MediatorConnectionId).HasValueGenerator(typeof(SequentialGuidValueGenerator));
    }
}