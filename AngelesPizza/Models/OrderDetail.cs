using AngelesPizza.Enums;
using AngelesPizza.Models;
using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        [Display(Name = "Pedido")]
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        [Display(Name = "Producto")]
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        [Display(Name = "Precio")]
        public int Price { get; set; }

        [Display(Name = "Estado")]
        public OrderStatus Status { get; set; } = OrderStatus.Queued;

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        // Navegación
        public ICollection<OrderDetailModifier> OrderDetailModifiers { get; set; } = new List<OrderDetailModifier>();
    }
}