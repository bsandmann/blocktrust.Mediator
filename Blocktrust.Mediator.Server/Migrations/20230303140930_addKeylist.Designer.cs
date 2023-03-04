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
    [Migration("20230303140930_addKeylist")]
    partial class addKeylist
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.MediatorConnectionEntity", b =>
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

                    b.ToTable("Connections");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.MediatorConnectionKeyEntity", b =>
                {
                    b.Property<Guid>("MediatorConnectionKeyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MediatorConnectionKeyId");

                    b.ToTable("MediatorConnectionKeyEntity");
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

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.MediatorConnectionKeyEntity", b =>
                {
                    b.HasOne("Blocktrust.Mediator.Server.Entities.MediatorConnectionEntity", "MediatorConnectionEntity")
                        .WithMany("KeyList")
                        .HasForeignKey("MediatorConnectionKeyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediatorConnectionEntity");
                });

            modelBuilder.Entity("Blocktrust.Mediator.Server.Entities.MediatorConnectionEntity", b =>
                {
                    b.Navigation("KeyList");
                });
#pragma warning restore 612, 618
        }
    }
}
