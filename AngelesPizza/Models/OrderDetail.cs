using AnglesPizza.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnglesPizza.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        [Display(Name = "Producto")]
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }

        [Display(Name = "Precio Unitario")]
        public int UnitPrice { get; set; }

        [Display(Name = "Subtotal")]
        public int Total { get; set; }

        [Display(Name = "Estado")]
        public OrderStatus Status { get; set; } = OrderStatus.Queued;

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        // Navegación
        public ICollection<OrderDetailModifier> OrderDetailModifiers { get; set; } = new List<OrderDetailModifier>();
    }
}