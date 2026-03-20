using HireWise.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using VendingMachine.DAL.Repository;
using VendingMachine.DAL.Storage.Extensions;
using VendingMachine.WebAPI.Extensions;
using VendingMachine.WebAPI.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

AdminAuthOptions authOptions = builder.Configuration.GetSection(AdminAuthOptions.SectionName).Get<AdminAuthOptions>()
    ?? throw new InvalidOperationException("Auth configuration is not defined.");

if (string.IsNullOrWhiteSpace(authOptions.SecretKey) || authOptions.SecretKey.Length < 32)
{
    throw new InvalidOperationException("Auth:SigningKey must be configured and contain at least 32 characters.");
}

builder.Services.AddSingleton(authOptions);

builder.Services.AddAuthorization();

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "VendingMachine API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

builder.Services.AddPictureStorage(builder.Configuration);

builder.Services.ConfigureDALDependencies();
builder.Services.ConfigureBLLDependencies();
builder.Services.ConfigureAuthorization();

string connection = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<VendingMachineDbContext>(options =>
    options.UseNpgsql(connection));
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection")
    ?? throw new InvalidOperationException("Connection string 'RedisConnection' is not configured.");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnection));

var allowedOrigin = builder.Configuration.GetSection("CorsSettings")["AllowedOrigin"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy
            .WithOrigins(allowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowBlazorClient");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePictureStorage();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
