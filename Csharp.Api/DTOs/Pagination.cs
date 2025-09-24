using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Csharp.Api.DTOs
{
    public record PagedResult<T>(IEnumerable<T> Items, int TotalItems, int Page, int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public string MetaToJson() =>
            JsonSerializer.Serialize(new { TotalItems, Page, PageSize, TotalPages });
    }

    public class LinkDto
    {
        public string Rel { get; init; } = string.Empty;
        public string Href { get; init; } = string.Empty;
        public string Method { get; init; } = "GET";
    }

    public static class HateoasBuilder
    {
        public static List<LinkDto> BuildLinks(
            Microsoft.AspNetCore.Mvc.IUrlHelper url,
            string getName, string updateName, string deleteName, Guid id) =>
            new()
            {
                new LinkDto { Rel = "self",   Href = url.Link(getName,    new { id })!, Method = "GET" },
                new LinkDto { Rel = "update", Href = url.Link(updateName, new { id })!, Method = "PUT" },
                new LinkDto { Rel = "delete", Href = url.Link(deleteName, new { id })!, Method = "DELETE" }
            };
    }
}
