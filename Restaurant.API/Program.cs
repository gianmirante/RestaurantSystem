using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Restaurant.API.Data;
using Scalar.AspNetCore;
using Restaurant.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DB Context Configuration
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ADD CORS REGISTRATION HERE
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClientPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7021", "http://localhost:5113") // Matches your Blazor port
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });


builder.Services.AddControllers();

// Register OpenAPI/Swagger 
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseRouting();
app.UseCors("BlazorClientPolicy"); // 🟢 ACTIVATE CORS MIDDLEWARE HERE (Must be before Authentication)




app.UseAuthentication(); 
app.UseAuthorization();

// 5. Map Controller Endpoints 
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    
    
    context.Database.Migrate();

    // Check if the table is currently empty
    if (!context.Users.Any())
    {
        context.Users.AddRange(            
            new User
            {
                Id = 1,
                Username = "admin",
                // Dynamically hashes the password string right now
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "Admin"
            }
        );
        
        // Tells SQL Server to respect our explicit hardcoded IDs (1 and 2) instead of auto-generating them
        context.Database.OpenConnection();
        try
        {
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Users ON");
            context.SaveChanges();
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Users OFF");
        }
        finally
        {
            context.Database.CloseConnection();
        }
    }
}

app.Run();