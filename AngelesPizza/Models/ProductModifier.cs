using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnglesPizza.Models
{
    public class ProductModifier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Costo Adicional")]
        public int ExtraCost { get; set; } = 0;

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        public int ProductId { get; set; }

        // Navegación
        public Product Product { get; set; } = null!;

        public ICollection<OrderDetailModifier> OrderDetailModifiers { get; set; } = new List<OrderDetailModifier>();

    }
}