using backend.Interfaces;
using backend.Models.Options;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.Configure<BunnyOptions>(builder.Configuration.GetSection("Bunny"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<BunnyOptions>>().Value);
builder.Services.AddSingleton<IBunnyEmbedSigner, BunnyEmbedSigner>();

builder.Services.AddHttpClient<IVideoService, BunnyVideoService>();

builder.Services.AddSingleton<IVideoProgressStore, InMemoryVideoProgressStore>();

builder.Services.AddScoped<IAuthService, AuthService>();

// Cors
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("front", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// Authentication
var jwtKey = builder.Configuration["Jwt:SigningKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(o =>
  {
      o.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
          ClockSkew = TimeSpan.FromSeconds(30)
      };
  });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("front");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
