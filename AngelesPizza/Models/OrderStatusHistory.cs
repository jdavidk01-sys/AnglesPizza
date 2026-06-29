using AnglesPizza.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnglesPizza.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        [Display(Name = "Estado")]
        public OrderStatus Status { get; set; }

        [Display(Name = "Fecha")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        // Se relacionará en la Fase 2
        public int? UserId { get; set; }
    }
}