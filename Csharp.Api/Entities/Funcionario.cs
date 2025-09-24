using System.ComponentModel.DataAnnotations;

namespace Csharp.Api.Entities
{
    public class Funcionario
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Cargo { get; set; }
    }
}
