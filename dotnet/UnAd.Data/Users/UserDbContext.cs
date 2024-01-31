using Microsoft.EntityFrameworkCore;
using UnAd.Data.Users.Models;

namespace UnAd.Data.Users;

public partial class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options) {
    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Role>(entity => {
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.ToTable("role");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
        });

        modelBuilder.Entity<User>(entity => {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.RoleId)
                .HasMaxLength(50)
                .HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}


