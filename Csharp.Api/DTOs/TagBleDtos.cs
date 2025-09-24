using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{
    public class CreateTagBleDto
    {
        [Required, StringLength(50)]
        public string CodigoUnicoTag { get; set; } = string.Empty;

        [Range(0, 100)]
        public int NivelBateria { get; set; } = 100;
    }

    public class UpdateTagBleDto : CreateTagBleDto { }

    public class TagBleViewDto
    {
        public Guid Id { get; set; }
        public string CodigoUnicoTag { get; set; } = string.Empty;
        public int NivelBateria { get; set; }

        // Links HATEOAS
        public List<LinkDto>? Links { get; set; }
    }
}
