using AngelesPizza.Models;
using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Precio")]
        public int Price { get; set; }

        [Display(Name = "Disponible")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        [Display(Name = "Imagen")]
        public byte[]? ImageData { get; set; }

        // Navegación
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}