using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Csharp.Api.Swagger
{
    /// <summary>Adiciona a documentação do header X-Pagination em respostas 200 de GETs.</summary>
    public class PaginationHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!string.Equals(context.ApiDescription.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            if (!operation.Responses.TryGetValue("200", out var resp))
                return;

            resp.Headers ??= new Dictionary<string, OpenApiHeader>();
            resp.Headers["X-Pagination"] = new OpenApiHeader
            {
                Description = "Metadados de paginação em JSON: { TotalItems, Page, PageSize, TotalPages }",
                Schema = new OpenApiSchema { Type = "string" }
            };
        }
    }
}
