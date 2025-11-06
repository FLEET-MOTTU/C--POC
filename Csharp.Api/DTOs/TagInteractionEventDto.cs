using System;
using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{
    /// <summary>Evento recebido via API (simulada) de interação da tag com um beacon.</summary>
    public class TagInteractionEventDto
    {
        [Required, StringLength(50)]
        public string CodigoUnicoTag { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string BeaconIdDetectado { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [Range(0, 100)]
        public int? NivelBateria { get; set; }

        [StringLength(50)]
        public string? TipoEvento { get; set; }
    }
}
