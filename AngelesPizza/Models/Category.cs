using System.ComponentModel.DataAnnotations;

namespace AnglesPizza.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Activa")]
        public bool IsActive { get; set; } = true;

        // Navegación
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}