using Microsoft.EntityFrameworkCore;
using TodoApi; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

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
// מפתח סודי - חייב להיות לפחות 32 תווים
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

// הפעלת Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// --- נתיבים (Routes) ---

app.MapGet("/", () => "API is Running!");

// 1. שליפת כל המשימות
// 1. שליפת כל המשימות (עכשיו רק למשתמשים מחוברים!)
// 1. שליפת משימות - רק של המשתמש המחובר!
app.MapGet("/items", async (ToDoDbContext db, ClaimsPrincipal user) =>
{
    // חילוץ ה-ID מהטוקן
    var userId = user.FindFirst("id")?.Value;
    if (userId == null) return Results.Unauthorized();

    // סינון המשימות לפי ה-UserId ששמור בטבלה
    var userTasks = await db.Items
        .Where(t => t.UserId == int.Parse(userId))
        .ToListAsync();

    return Results.Ok(userTasks);
}).RequireAuthorization();

// 2. הוספת משימה - שיוך אוטומטי למשתמש
app.MapPost("/login", async (ToDoDbContext db, User loginUser) =>
{
    // 1. בדיקה שהשדות לא ריקים או מכילים רק רווחים
    if (string.IsNullOrWhiteSpace(loginUser.Username) || string.IsNullOrWhiteSpace(loginUser.Password))
    {
        return Results.BadRequest(new { message = "Username and password are required" });
    }

    // 2. חיפוש המשתמש במסד הנתונים
    var user = await db.Users.FirstOrDefaultAsync(u => 
        u.Username == loginUser.Username && u.Password == loginUser.Password);
    
    // 3. אם לא נמצא משתמש - מחזירים שגיאה
    if (user is null) 
    {
        return Results.Unauthorized();
    }

    // 4. יצירת הטוקן (הקוד הקיים שלך...)
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
// 4. מחיקת משימה
app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    if (await db.Items.FindAsync(id) is Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.Ok(item);
    }
    return Results.NotFound();
});
// נתיב להתחברות - מחזיר טוקן
app.MapPost("/login", async (ToDoDbContext db, User loginUser) =>
{
    // מחפשים את המשתמש במסד הנתונים (במציאות מצפינים סיסמה!)
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginUser.Username && u.Password == loginUser.Password);
    
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

// נתיב להרשמה (משתמש חדש)
app.MapPost("/register", async (ToDoDbContext db, User newUser) =>
{
    db.Users.Add(newUser);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "User registered successfully" });
});
app.Run();