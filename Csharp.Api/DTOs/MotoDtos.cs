using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.DTOs
{
    public class CreateMotoDto
    {
        [StringLength(10)]
        public string? Placa { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoModeloMoto Modelo { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoStatusMoto StatusMoto { get; set; }
    }

    public class UpdateMotoDto : CreateMotoDto { }

    public class MotoViewDto
    {
        public Guid Id { get; set; }
        public string? Placa { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoModeloMoto Modelo { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoStatusMoto StatusMoto { get; set; }

        public DateTime DataCriacaoRegistro { get; set; }
        public TagBleViewDto? Tag { get; set; }
        public List<LinkDto>? Links { get; set; }
    }
}
