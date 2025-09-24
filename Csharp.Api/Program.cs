using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using System.IO;

using Csharp.Api.Data;
using Csharp.Api.Middleware;
using Csharp.Api.Services;
using Csharp.Api.Entities.Enums;
using Csharp.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

// ----- Database (Oracle)
var oracleConnectionString = builder.Configuration.GetConnectionString("OracleConnection");
if (string.IsNullOrWhiteSpace(oracleConnectionString))
{
    throw new InvalidOperationException(
        "String de conexão 'OracleConnection' não foi encontrada ou está vazia. Verifique a configuração.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnectionString, oraOpt => { })
);

// ----- DI (Services)
builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<ITagBleService, TagBleService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<IIoTEventService, IoTEventService>();

// ----- Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Enums como string no JSON
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ----- Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Mottu Fleet API - C# (Pátio)",
        Description = "API para gerenciamento de pátio, motos, tags BLE e funcionários."
    });

    // Incluir XML docs gerado pelo .csproj
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Mapear enums como string + lista de valores
    options.MapType<TipoStatusMoto>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames<TipoStatusMoto>()
            .Select(n => (IOpenApiAny)new OpenApiString(n)).ToList()
    });
    options.MapType<TipoModeloMoto>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames<TipoModeloMoto>()
            .Select(n => (IOpenApiAny)new OpenApiString(n)).ToList()
    });

    // Documentar o header X-Pagination
    options.OperationFilter<PaginationHeaderOperationFilter>();
});

var app = builder.Build();

// ----- Middleware global de exceções
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// ----- Aplicar migrations no start
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Console.WriteLine("INFO: Migrations aplicadas (ou nenhuma pendente).");
}
catch (Exception ex)
{
    Console.WriteLine($"ERRO ao aplicar migrations: {ex.Message}");
    throw;
}

// ----- Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mottu Fleet API v1");
    c.RoutePrefix = "swagger";
});

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.Run();
