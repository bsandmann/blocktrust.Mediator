namespace Blocktrust.Mediator.Server;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

//These lines are only need if a migration is run

public class DataContext : DbContext
{
    public DbSet<OobInvitationEntity> OobInvitations { get; set; }
    public DbSet<SecretEntity> Secrets { get; set; }
    public DbSet<MediatorConnection> MediatorConnections { get; set; }
    public DbSet<RegisteredRecipient> RegisteredRecipients { get; set; }
    public DbSet<StoredMessage> StoredMessages { get; set; }
    public DbSet<ShortenedUrlEntity> ShortenedUrlEntities { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediatorConnection>()
            .HasKey(b => b.MediatorConnectionId);
        modelBuilder.Entity<MediatorConnection>().Property(b => b.MediatorConnectionId).HasValueGenerator(typeof(SequentialGuidValueGenerator));
        
        modelBuilder.Entity<OobInvitationEntity>()
            .HasKey(b => b.OobId);
        modelBuilder.Entity<OobInvitationEntity>().Property(b => b.OobId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<SecretEntity>()
            .HasKey(b => b.SecretId);
        modelBuilder.Entity<SecretEntity>().Property(b => b.SecretId).HasValueGenerator(typeof(SequentialGuidValueGenerator));
        
        modelBuilder.Entity<ShortenedUrlEntity>()
            .HasKey(b => b.ShortenedUrlEntityId);

        modelBuilder.Entity<RegisteredRecipient>()
            .HasKey(b => b.RegisteredRecipientId);
        modelBuilder.Entity<RegisteredRecipient>().Property(b => b.RegisteredRecipientId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<StoredMessage>()
            .HasKey(b => b.StoredMessageEntityId);
        modelBuilder.Entity<StoredMessage>().Property(b => b.StoredMessageEntityId).HasValueGenerator(typeof(SequentialGuidValueGenerator));

        modelBuilder.Entity<RegisteredRecipient>()
            .HasOne(p => p.MediatorConnection)
            .WithMany(b => b.RegisteredRecipients)
            .HasForeignKey(p=>p.MediatorConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StoredMessage>()
            .HasOne(p => p.RegisteredRecipient)
            .WithMany(b => b.StoredMessages)
            .HasForeignKey(p=>p.RegisteredRecipientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}