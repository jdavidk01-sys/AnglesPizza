using AngelesPizza.Models;
using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Models
{
    public class OrderDetailModifier
    {
        public int Id { get; set; }

        [Display(Name = "Detalle del Pedido")]
        public int OrderDetailId { get; set; }

        public OrderDetail OrderDetail { get; set; } = null!;

        [Display(Name = "Modificador")]
        public int ProductModifierId { get; set; }

        public ProductModifier ProductModifier { get; set; } = null!;

        [Display(Name = "Costo Adicional")]
        public int ExtraCost { get; set; }
    }
}