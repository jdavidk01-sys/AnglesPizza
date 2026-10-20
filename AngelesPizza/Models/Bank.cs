using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Models
{
    public class Bank
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre del Banco")]
        public string Name { get; set; } = null!;

        [StringLength(20)]
        [Display(Name = "Código SWIFT")]
        public string? SwiftCode { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación inversa
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
