using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Enums
{
    public enum OrderStatus
    {
        [Display(Name="Creado")]
        Created = 1,
        [Display(Name = "Fila")]
        Queued = 2,
        [Display(Name = "Preparando")]
        Preparing = 3,
        [Display(Name = "Listo")]
        Ready = 4,
        [Display(Name = "Entregado")]
        Delivered = 5,
        [Display(Name = "Cancelado")]
        Cancelled = 6
    }
}