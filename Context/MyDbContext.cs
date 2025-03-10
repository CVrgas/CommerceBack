using System;
using System.Collections.Generic;
using CommerceBack.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommerceBack.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartProduct> CartProducts { get; set; }

    public virtual DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<TokenStatus> TokenStatuses { get; set; }

    public virtual DbSet<TokenType> TokenTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC0782254889");

            entity.ToTable("Cart");

            entity.HasIndex(e => e.UserId, "UQ__Cart__1788CC4DC7082C26").IsUnique();

            entity.HasOne(d => d.User).WithOne(p => p.CartNavigation)
                .HasForeignKey<Cart>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__UserId__4589517F");
        });

        modelBuilder.Entity<CartProduct>(entity =>
        {
            entity.ToTable("CartProduct");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartProducts)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartProduct_Cart");

            entity.HasOne(d => d.Products).WithMany(p => p.CartProducts)
                .HasForeignKey(d => d.ProductsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartProduct_Product");
        });

        modelBuilder.Entity<PasswordResetCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3214EC07A868413D");

            entity.ToTable("PasswordResetCode");

            entity.HasIndex(e => e.Code, "UQ__Password__A25C5AA7487DE686").IsUnique();

            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07320AB4B6");

            entity.ToTable("Product");

            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Rating).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RatingSum).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_ProductCategory");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategory");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Token_1");

            entity.ToTable("Token");

            entity.Property(e => e.Expiration).HasColumnType("datetime");
            entity.Property(e => e.Value).HasMaxLength(255);

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Token_TokenStatus");

            entity.HasOne(d => d.TokenTypeNavigation).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.TokenType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Token_TokenType1");
        });

        modelBuilder.Entity<TokenStatus>(entity =>
        {
            entity.ToTable("TokenStatus");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TokenType>(entity =>
        {
            entity.ToTable("TokenType");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.TimeSpanDefault).HasColumnType("decimal(18, 4)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Firstname).HasMaxLength(50);
            entity.Property(e => e.LastAccessDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Lastname).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RefreshToken).HasMaxLength(255);
            entity.Property(e => e.Salt).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Role)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
