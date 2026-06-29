using System.ComponentModel.DataAnnotations;

namespace AnglesPizza.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [StringLength(250)]
        [Display(Name = "Dirección")]
        public string? Address { get; set; }

        [StringLength(250)]
        [Display(Name = "Referencia")]
        public string? Reference { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}