using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.Enums
{
    public enum OrderStatus
    {
        Creado = 1,

        Fila = 2,

        Preparando = 3,

        Listo = 4,

        Entregado = 5,

        Cancelado = 6
    }
}