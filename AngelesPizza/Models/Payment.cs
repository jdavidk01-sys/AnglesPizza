using AnglesPizza.Enums;
using System.ComponentModel.DataAnnotations;

namespace AnglesPizza.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Display(Name = "Pedido")]
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        [Display(Name = "Método de Pago")]
        public PaymentType PaymentType { get; set; }

        [Display(Name = "Valor")]
        public int Amount { get; set; }

        [Display(Name = "Fecha")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }
    }
}