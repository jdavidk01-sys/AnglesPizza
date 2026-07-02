using AngelesPizza.Enums;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Models
{
    public class OrderDetailModifier
    {
        public int Id { get; set; }

        [Display(Name = "Detalle del Pedido")]
        public int OrderDetailId { get; set; }

        [ValidateNever] 
        public OrderDetail OrderDetail { get; set; } = null!;

        [Display(Name = "Modificador")]
        public int ProductModifierId { get; set; }

        [ValidateNever] public ProductModifier ProductModifier { get; set; } = null!;

        [Display(Name = "Costo Adicional")]
        public int ExtraCost { get; set; }
    }
}