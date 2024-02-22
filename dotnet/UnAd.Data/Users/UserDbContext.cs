using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users.Models;

namespace UnAd.Data.Users;

public partial class UserDbContext : DbContext {
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options) { }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Subscriber> Subscribers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Client>(entity => {
            entity.HasKey(e => e.Id).HasName("client_pkey");

            entity.ToTable("client");

            entity.HasIndex(e => e.PhoneNumber, "client_phone_number_key").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "idx_client_phone_number");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CustomerId)
                .HasColumnType("character varying")
                .HasColumnName("customer_id");
            entity.Property(e => e.Locale)
                .HasMaxLength(5)
                .HasDefaultValueSql("'en-US'::character varying")
                .HasColumnName("locale");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.SubscriptionId)
                .HasColumnType("character varying")
                .HasColumnName("subscription_id");

            entity.HasMany(d => d.Subscribers).WithMany(p => p.Clients)
                .UsingEntity<Dictionary<string, object>>(
                    "ClientSubscriber",
                    r => r.HasOne<Subscriber>().WithMany()
                        .HasForeignKey("SubscriberPhoneNumber")
                        .HasConstraintName("client_subscriber_subscriber_phone_number_fkey"),
                    l => l.HasOne<Client>().WithMany()
                        .HasForeignKey("ClientId")
                        .HasConstraintName("client_subscriber_client_id_fkey"),
                    j => {
                        j.HasKey("ClientId", "SubscriberPhoneNumber").HasName("client_subscriber_pkey");
                        j.ToTable("client_subscriber");
                        j.IndexerProperty<Guid>("ClientId").HasColumnName("client_id");
                        j.IndexerProperty<string>("SubscriberPhoneNumber")
                            .HasMaxLength(15)
                            .HasColumnName("subscriber_phone_number");
                    });
        });

        modelBuilder.Entity<Product>(entity => {
            entity.HasKey(e => e.ProductId).HasName("product_pkey");

            entity.ToTable("product");

            entity.Property(e => e.ProductId)
                .HasColumnType("character varying")
                .HasColumnName("product_id");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Subscriber>(entity => {
            entity.HasKey(e => e.PhoneNumber).HasName("subscriber_pkey");

            entity.ToTable("subscriber");

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.JoinedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("joined_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
