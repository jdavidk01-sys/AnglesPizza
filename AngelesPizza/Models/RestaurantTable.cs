using System.ComponentModel.DataAnnotations;

namespace AnglesPizza.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Mesa")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Capacidad")]
        public int Capacity { get; set; }

        [Display(Name = "Activa")]
        public bool IsActive { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}