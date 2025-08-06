using System.Text;
using CoolAuth.Data;
using CoolAuth.Extensions;
using CoolAuth.Repositories;
using CoolAuth.Services;
using CoolAuth.Services.Abstraction;
using CoolAuth.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

WebApplication app = null!;
var builder = WebApplication.CreateBuilder(args);
Cfg.MapEnvToConfig(builder.Configuration);
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<ICacheService,RedisService>();
builder.Services.AddScoped<JwtService>();
ConfigureAuthentication(builder);
app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.MapExceptions();
app.Run();

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer              = app.Configuration["JwtSettings:Issuer"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(app.Configuration["JwtSettings:Key"]!)),
            ValidateIssuer           = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime         = true,
        };
    });
}
