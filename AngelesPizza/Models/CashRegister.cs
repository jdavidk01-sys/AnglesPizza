using AngelesPizza.Enums;

namespace AngelesPizza.Models
{
    public class CashRegister
    {
        public int Id { get; set; }
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        // Monto con el que abre la caja
        public int InitialAmount { get; set; }

        // Calculados del sistema al cerrar
        public int TotalCashSales { get; set; }
        public int TotalTransferSales { get; set; }
        public int TotalCardSales { get; set; }

        public int FinalCashExpected { get; set; }     // InitialAmount + TotalCashSales

        // Conteo físico ingresado por el usuario al cerrar
        public int? FinalCashReal { get; set; }
        public int? CashDifference { get; set; }        // FinalCashReal - FinalCashExpected

        public bool? IsClosed { get; set; } = false;
        public string? Notes { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}