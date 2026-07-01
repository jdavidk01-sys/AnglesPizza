using AngelesPizza.Enums;
using AngelesPizza.Models;
using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Display(Name = "Número de Pedido")]
        public int OrderNumber { get; set; }

        [Display(Name = "Fecha")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Tipo de Pedido")]
        public OrderType OrderType { get; set; }

        [Display(Name = "Estado")]
        public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

        [Display(Name = "Cliente")]
        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Display(Name = "Mesa")]
        public int? RestaurantTableId { get; set; }

        public RestaurantTable? RestaurantTable { get; set; }

        [Display(Name = "Total")]
        public int Total { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        // Navegación
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}