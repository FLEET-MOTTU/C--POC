using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.DTOs
{
    public class CreateFuncionarioDto
    {
        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Cargo { get; set; }
    }

    public class UpdateFuncionarioDto : CreateFuncionarioDto { }

    public class FuncionarioViewDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Cargo { get; set; }

        // Links HATEOAS
        public List<LinkDto>? Links { get; set; }
    }
}
