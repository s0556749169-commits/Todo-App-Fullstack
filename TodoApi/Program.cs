using Microsoft.EntityFrameworkCore;
using TodoApi; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// --- 1. הגדרות שירותים (Services) ---

// הגדרת CORS - מאפשר ל-React לתקשר עם השרת
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// הזרקת ה-DbContext
builder.Services.AddDbContext<ToDoDbContext>();

// הגדרת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// מפתח סודי ל-JWT
var key = Encoding.ASCII.GetBytes("A_Very_Long_Secret_Key_For_My_Todo_App_123!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// --- 2. הגדרות Pipeline (Middleware) ---

// מוודא שבסיס הנתונים והטבלאות קיימים בענן
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    db.Database.EnsureCreated();
}

// הפעלת Swagger
app.UseSwagger();
app.UseSwaggerUI();

// חשוב: UseCors חייב להיות לפני Authentication ו-Authorization
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// --- 3. נתיבים (Routes) ---

app.MapGet("/", () => "API is Running!");

// הרשמה (Register)
app.MapPost("/register", async (ToDoDbContext db, User newUser) =>
{
    // בדיקה בסיסית אם המשתמש כבר קיים
    var exists = await db.Users.AnyAsync(u => u.Username == newUser.Username);
    if (exists) return Results.BadRequest(new { message = "Username already exists" });

    db.Users.Add(newUser);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "User registered successfully" });
});

// התחברות (Login)
app.MapPost("/login", async (ToDoDbContext db, User loginUser) =>
{
    if (string.IsNullOrWhiteSpace(loginUser.Username) || string.IsNullOrWhiteSpace(loginUser.Password))
    {
        return Results.BadRequest(new { message = "Username and password are required" });
    }

    var user = await db.Users.FirstOrDefaultAsync(u => 
        u.Username == loginUser.Username && u.Password == loginUser.Password);
    
    if (user is null) return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { 
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("id", user.Id.ToString()) 
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token) });
});

// שליפת משימות של המשתמש המחובר
app.MapGet("/items", async (ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("id")?.Value;
    if (userId == null) return Results.Unauthorized();

    var userTasks = await db.Items
        .Where(t => t.UserId == int.Parse(userId))
        .ToListAsync();

    return Results.Ok(userTasks);
}).RequireAuthorization();

// הוספת משימה
app.MapPost("/items", async (ToDoDbContext db, Item newItem, ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("id")?.Value;
    if (userId == null) return Results.Unauthorized();

    newItem.UserId = int.Parse(userId);
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
}).RequireAuthorization();

// מחיקת משימה
app.MapDelete("/items/{id}", async (ToDoDbContext db, int id, ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("id")?.Value;
    if (userId == null) return Results.Unauthorized();

    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    
    if (item.UserId != int.Parse(userId)) return Results.Forbid();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Deleted", id });
}).RequireAuthorization();

app.Run();