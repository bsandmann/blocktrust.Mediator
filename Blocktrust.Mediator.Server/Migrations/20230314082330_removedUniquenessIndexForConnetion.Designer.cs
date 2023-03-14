﻿// <auto-generated />
using System;
using Blocktrust.Mediator.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Blocktrust.Mediator.Server.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230314082330_removedUniquenessIndexForConnetion")]
    partial class removedUniquenessIndexForConnetion
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.MediatorConnection", b =>
                {
                    b.Property<Guid>("MediatorConnectionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool>("MediationGranted")
                        .HasColumnType("bit");

                    b.Property<string>("MediatorDid")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MediatorEndpoint")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RemoteDid")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoutingDid")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MediatorConnectionId");

                    b.ToTable("MediatorConnections");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.OobInvitationEntity", b =>
                {
                    b.Property<Guid>("OobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Did")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Invitation")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("OobId");

                    b.ToTable("OobInvitations");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.RegisteredRecipient", b =>
                {
                    b.Property<Guid>("RegisteredRecipientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MediatorConnectionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("RecipientDid")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RegisteredRecipientId");

                    b.HasIndex("MediatorConnectionId");

                    b.ToTable("RegisteredRecipients");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.SecretEntity", b =>
                {
                    b.Property<Guid>("SecretId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Kid")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("VerificationMaterialFormat")
                        .HasColumnType("int");

                    b.Property<int>("VerificationMethodType")
                        .HasColumnType("int");

                    b.HasKey("SecretId");

                    b.ToTable("Secrets");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.ShortenedUrlEntity", b =>
                {
                    b.Property<Guid>("ShortenedUrlEntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ExpirationUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("GoalCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LongFormUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestedPartialSlug")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ShortenedUrlEntityId");

                    b.ToTable("ShortenedUrlEntities");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.StoredMessage", b =>
                {
                    b.Property<Guid>("StoredMessageEntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MessageHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MessageId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("MessageSize")
                        .HasColumnType("bigint");

                    b.Property<Guid>("RegisteredRecipientId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("StoredMessageEntityId");

                    b.HasIndex("RegisteredRecipientId");

                    b.ToTable("StoredMessages");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.RegisteredRecipient", b =>
                {
                    b.HasOne("Blocktrust.Mediator.Server.Entities.MediatorConnection", "MediatorConnection")
                        .WithMany("RegisteredRecipients")
                        .HasForeignKey("MediatorConnectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediatorConnection");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.StoredMessage", b =>
                {
                    b.HasOne("Blocktrust.Mediator.Server.Entities.RegisteredRecipient", "RegisteredRecipient")
                        .WithMany("StoredMessages")
                        .HasForeignKey("RegisteredRecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RegisteredRecipient");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.MediatorConnection", b =>
                {
                    b.Navigation("RegisteredRecipients");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.RegisteredRecipient", b =>
                {
                    b.Navigation("StoredMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
