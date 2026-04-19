using MediKartX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// Load environment variables from .env
// ==============================
Env.Load(); // Loads .env file


string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")!;
string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")!;
string jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
string jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
int jwtExpires = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES") ?? "60");
string hostName = Environment.GetEnvironmentVariable("HOST_NAME") ?? "localhost";
 int httpPort = int.Parse(Environment.GetEnvironmentVariable("PORT_HTTP") ?? "5126");
int httpsPort = int.Parse(Environment.GetEnvironmentVariable("PORT_HTTPS") ?? "5127");

// ==============================
// Configure Kestrel for multiple ports

// ==============================
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Parse("127.0.0.1"), httpPort); // IPv4 HTTP
    options.Listen(System.Net.IPAddress.Parse("127.0.0.1"), httpsPort, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});
// ==============================
// Database
// ==============================
builder.Services.AddDbContext<MediKartXDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==============================
// Controllers & Swagger
// ==============================
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// ==============================
// CORS Configuration
// ==============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// Register application services (Onion: Application -> Infrastructure)
builder.Services.AddScoped<MediKartX.Application.Interfaces.IAuthService, MediKartX.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.ISmsSender, MediKartX.Infrastructure.Services.TwilioSmsSender>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.IEmailSender, MediKartX.Infrastructure.Services.SendGridEmailSender>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.IMedicineService, MediKartX.Infrastructure.Services.MedicineService>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.ICartService, MediKartX.Infrastructure.Services.CartService>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.ICategoryService, MediKartX.Infrastructure.Services.CategoryService>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.ICouponService, MediKartX.Infrastructure.Services.CouponService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<MediKartX.Application.Interfaces.IOrderService, MediKartX.Infrastructure.Services.OrderService>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.IWishlistService, MediKartX.Infrastructure.Services.WishlistService>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.IReviewService, MediKartX.Infrastructure.Services.ReviewService>();
builder.Services.AddScoped<MediKartX.Application.Interfaces.IUserService, MediKartX.Infrastructure.Services.UserService>();

builder.Services.AddScoped<MediKartX.Application.Interfaces.IAdminDashboardService, MediKartX.Infrastructure.Services.AdminDashboardService>();

// ==============================
// JWT Authentication
// ==============================
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// ==============================
// Middleware
// ==============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediKartX API V1");
        c.RoutePrefix = string.Empty;
    });
}

// Enable CORS
app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
);

// Serve static files (Angular dist)
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallback(context =>
{
    context.Response.ContentType = "text/html";
    return context.Response.SendFileAsync(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"));
});

// ==============================
// Run app
// ==============================
app.Run();
