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
    public DbSet<ConnectionEntity> Connections { get; set; }
    public DbSet<ConnectionKeyEntity> RecipientKeys { get; set; }
    public DbSet<StoredMessageEntity> StoredMessages { get; set; }

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

        modelBuilder.Entity<ConnectionEntity>()
            .HasKey(b => b.ConnectionEntityId);
        modelBuilder.Entity<ConnectionEntity>().Property(b => b.ConnectionEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<ConnectionKeyEntity>()
            .HasKey(b => b.ConnectionKeyEntityId);
        modelBuilder.Entity<ConnectionKeyEntity>().Property(b => b.ConnectionKeyEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<StoredMessageEntity>()
            .HasKey(b => b.StoredMessageEntityId);
        modelBuilder.Entity<StoredMessageEntity>().Property(b => b.StoredMessageEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<ConnectionKeyEntity>()
            .HasOne(p => p.ConnectionEntity)
            .WithMany(b => b.KeyList)
            .HasForeignKey(p => p.ConnectionKeyEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StoredMessageEntity>()
            .HasOne(p => p.ConnectionKeyEntity)
            .WithMany(b => b.StoredMessage)
            .HasForeignKey(p => p.StoredMessageEntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}