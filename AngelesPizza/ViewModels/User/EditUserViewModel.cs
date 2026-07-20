using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.ViewModels.User
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Nombre Completo")]
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Usuario")]
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        [Required]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool IsActive { get; set; }
    }
}