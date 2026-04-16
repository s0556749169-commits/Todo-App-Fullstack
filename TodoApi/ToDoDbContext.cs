using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public partial class ToDoDbContext : DbContext
{
    public ToDoDbContext()
    {
    }

    public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public virtual DbSet<Item> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 1. ניסיון לקרוא את הכתובת מהמשתנה שהגדרת ב-Render
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            // 2. אם את במחשב המקומי (והמשתנה ריק), השתמשי בכתובת המקומית שלך
            if (string.IsNullOrEmpty(connectionString))
            {
                // כאן את יכולה לשים את מחרוזת החיבור למחשב שלך בבית אם תרצי
                connectionString = "server=localhost;database=todo;user=root;password=your_password";
            }

            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("items");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}