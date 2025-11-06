using System.Text;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Oracle.EntityFrameworkCore.Storage.Internal;
using Swashbuckle.AspNetCore.Filters;

using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Middleware;
using Csharp.Api.Profiles;
using Csharp.Api.Services;
using Csharp.Api.Enums;

var builder = WebApplication.CreateBuilder(args);

// ---------- DB (Oracle) ----------
var oracleConnectionString = builder.Configuration.GetConnectionString("OracleConnection");
if (string.IsNullOrEmpty(oracleConnectionString))
{
    throw new InvalidOperationException("String de conexão 'OracleConnection' não foi encontrada ou está vazia. Verifique a configuração.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnectionString, oraOpt =>
    {
        // Estratégia de retry para Oracle
        oraOpt.ExecutionStrategy(c => new OracleExecutionStrategy(c));
    })
);

// ---------- DI ----------
builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<IIoTEventService, IoTEventService>();
builder.Services.AddScoped<IBeaconService, BeaconService>();
builder.Services.AddScoped<IPateoService, PateoService>();
builder.Services.AddScoped<ITagPositionProcessor, TagPositionProcessor>();
builder.Services.AddScoped<IClassificationService, ClassificationService>();

builder.Services.AddHostedService<TagPositionConsumerService>();
builder.Services.AddHostedService<InterServiceSyncService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

// ---------- Controllers / JSON ----------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // enums como string
    });

builder.Services.AddEndpointsApiExplorer();

// ---------- Swagger ----------
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Mottu Fleet API - C# (Pátio)",
        Description = "API para gerenciamento de pátios e motos da Mottu."
    });

    // XML doc (se gerado no csproj)
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Exemplos (Swashbuckle.AspNetCore.Filters)
    options.ExampleFilters();

    // Auth (Bearer)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Autenticação JWT (Bearer). Insira 'Bearer' [espaço] e então o seu token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// registra providers de exemplos do assembly onde está CreateMotoDtoExample
builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateMotoDtoExample>();

// ---------- Auth (JWT) ----------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RolesOperacionais", policy =>
        policy.RequireRole(
            nameof(UserRole.OPERACIONAL),
            nameof(UserRole.ADMINISTRATIVO),
            nameof(UserRole.TEMPORARIO)
        ));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["JwtSettings:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("Chave JWT ('JwtSettings:Key') não encontrada ou está vazia.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    };
});

var app = builder.Build();

// ---------- Migrations automáticas ----------
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
{
    Console.WriteLine("INFO: Verificando e aplicando migrations pendentes do EF Core...");
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("INFO: Migrations aplicadas com sucesso (ou nenhuma pendente).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO CRÍTICO ao aplicar migrations: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Mottu Challenge V1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
