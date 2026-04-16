protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        
        // אם אין משתנה סביבה (כמו במחשב שלך), הוא ישתמש ב-Localhost
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "server=localhost;database=todo;user=root;password=your_password";
        }

        // שימוש בגרסה קבועה (8.0.30) מונע מהשרת לקרוס בניסיון לזהות את הדאטהבייס
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 30)); 
        optionsBuilder.UseMySql(connectionString, serverVersion);
    }
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // חשוב מאוד: הגדרת שמות טבלאות באותיות קטנות (linux compatible)
    modelBuilder.Entity<User>(entity => {
        entity.ToTable("users"); 
        entity.HasKey(e => e.Id);
    });

    modelBuilder.Entity<Item>(entity => {
        entity.ToTable("items");
        entity.HasKey(e => e.Id);
    });
}