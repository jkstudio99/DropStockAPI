using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DropStockAPI.Extensions;
using DropStockAPI.Filters;
using DropStockAPI.Helpers;
using DropStockAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

// Retrieve Cloudinary settings from appsettings.json
var cloudinaryAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]
);

// Register Cloudinary as a singleton service in the dependency injection container
services.AddSingleton(new Cloudinary(cloudinaryAccount));

// Register EmailService as a singleton service in the dependency injection container
services.AddScoped<EmailService>();


// Add services to the container
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

services.AddScoped<TokenHelper>();

// Add authentication for JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings["ValidIssuer"],
        ValidAudience = jwtSettings["ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecurityKey"]!))
    };
});

// Allow CORS
services.AddCors(options =>
{
    options.AddPolicy("CorsDropStock", policy =>
    {
        policy.WithOrigins(
                "https://example.azurewebsites.net",
                "https://example.netlify.app",
                "https://example.vercel.app",
                "https://example.herokuapp.com",
                "https://example.firebaseapp.com",
                "https://example.github.io",
                "https://example.gitlab.io",
                "https://example.onrender.com",
                "https://example.surge.sh",
                "http://localhost:8080",
                "http://localhost:4200",
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:5000",
                "http://localhost:5001",
                "http://127.0.0.1:5500"
            )
            .SetIsOriginAllowedToAllowWildcardSubdomains() // Allows subdomains for the specified origins
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Ensure credentials can be sent
    });
});

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.SwaggerDoc("v1", new() { Title = "DropStockAPI", Version = "v1" });

    // Add security definition for Bearer token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    // Apply security to all operations that require authorization
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // เพิ่มฟิลเตอร์ AuthorizeCheckOperationFilter
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});



var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsDropStock");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
