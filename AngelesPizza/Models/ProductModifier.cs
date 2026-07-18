using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [ValidateNever] 
        public ICollection<OrderDetailModifier> OrderDetailModifiers { get; set; } = new List<OrderDetailModifier>();

        [ValidateNever]
        public ICollection<ProductModifierProduct> ProductModifierProducts { get; set; } = new List<ProductModifierProduct>();

        [NotMapped]
        public List<int> ProductIds { get; set; } = new();
    }
}