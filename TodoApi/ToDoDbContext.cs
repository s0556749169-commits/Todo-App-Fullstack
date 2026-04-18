using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public class ToDoDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            
            if (string.IsNullOrEmpty(connectionString))
                connectionString = "server=localhost;database=todo;user=root;password=your_password";

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 30)); 
            optionsBuilder.UseMySql(connectionString, serverVersion);
        }
        optionsBuilder.EnableDetailedErrors();
    }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity => {
        entity.ToTable("users"); 
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
        entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
    });

    modelBuilder.Entity<Item>(entity => {
        entity.ToTable("items");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).HasMaxLength(255);
    });
}
}