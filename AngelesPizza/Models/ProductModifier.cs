using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Models
{
    public class ProductModifier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Costo Adicional")]
        public int ExtraCost { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        // Navegación
        public ICollection<OrderDetailModifier> OrderDetailModifiers { get; set; } = new List<OrderDetailModifier>();
    }
}