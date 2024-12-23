﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UnAd.Data.Users;

#nullable disable

namespace UnAd.Data.Users.Migrations
{
    [DbContext(typeof(UserDbContext))]
    [Migration("20240407032747_Slugs")]
    partial class Slugs
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "uuid-ossp");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ClientSubscriber", b =>
                {
                    b.Property<Guid>("ClientId")
                        .HasColumnType("uuid")
                        .HasColumnName("client_id");

                    b.Property<string>("SubscriberPhoneNumber")
                        .HasMaxLength(15)
                        .HasColumnType("character varying(15)")
                        .HasColumnName("subscriber_phone_number");

                    b.HasKey("ClientId", "SubscriberPhoneNumber")
                        .HasName("client_subscriber_pkey");

                    b.HasIndex(new[] { "SubscriberPhoneNumber" }, "IX_client_subscriber_subscriber_phone_number");

                    b.ToTable("client_subscriber", (string)null);
                });

            modelBuilder.Entity("UnAd.Data.Users.Models.Announcement", b =>
                {
                    b.Property<string>("MessageSid")
                        .HasMaxLength(34)
                        .HasColumnType("character(34)")
                        .HasColumnName("message_sid")
                        .IsFixedLength();

                    b.Property<Guid?>("ClientId")
                        .HasColumnType("uuid")
                        .HasColumnName("client_id");

                    b.Property<DateTime?>("SentOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("sent_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("MessageSid")
                        .HasName("announcement_pkey");

                    b.HasIndex(new[] { "ClientId" }, "IX_announcement_client_id");

                    b.ToTable("announcement", (string)null);
                });

            modelBuilder.Entity("UnAd.Data.Users.Models.Client", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<string>("CustomerId")
                        .HasColumnType("character varying")
                        .HasColumnName("customer_id");

                    b.Property<DateTime?>("JoinedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("joined_date")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)")
                        .HasColumnName("locale")
                        .HasDefaultValueSql("'en-US'::character varying");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("character varying")
                        .HasColumnName("name");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("character varying(15)")
                        .HasColumnName("phone_number");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("character varying(12)")
                        .HasColumnName("slug");

                    b.Property<string>("SubscriptionId")
                        .HasColumnType("character varying")
                        .HasColumnName("subscription_id");

                    b.HasKey("Id")
                        .HasName("client_pkey");

                    b.HasIndex(new[] { "PhoneNumber" }, "client_phone_number_key")
                        .IsUnique();

                    b.HasIndex(new[] { "Slug" }, "client_slug_key")
                        .IsUnique();

                    b.HasIndex(new[] { "PhoneNumber" }, "idx_client_phone_number");

                    b.HasIndex(new[] { "Slug" }, "idx_client_slug");

                    b.ToTable("client", (string)null);
                });

            modelBuilder.Entity("UnAd.Data.Users.Models.Subscriber", b =>
                {
                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(15)
                        .HasColumnType("character varying(15)")
                        .HasColumnName("phone_number");

                    b.Property<DateTime?>("JoinedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("joined_date")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)")
                        .HasColumnName("locale")
                        .HasDefaultValueSql("'en-US'::character varying");

                    b.HasKey("PhoneNumber")
                        .HasName("subscriber_pkey");

                    b.ToTable("subscriber", (string)null);
                });

            modelBuilder.Entity("ClientSubscriber", b =>
                {
                    b.HasOne("UnAd.Data.Users.Models.Client", null)
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("client_subscriber_client_id_fkey");

                    b.HasOne("UnAd.Data.Users.Models.Subscriber", null)
                        .WithMany()
                        .HasForeignKey("SubscriberPhoneNumber")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("client_subscriber_subscriber_phone_number_fkey");
                });

            modelBuilder.Entity("UnAd.Data.Users.Models.Announcement", b =>
                {
                    b.HasOne("UnAd.Data.Users.Models.Client", "Client")
                        .WithMany("Announcements")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("announcement_client_id_fkey");

                    b.Navigation("Client");
                });

            modelBuilder.Entity("UnAd.Data.Users.Models.Client", b =>
                {
                    b.Navigation("Announcements");
                });
#pragma warning restore 612, 618
        }
    }
}
