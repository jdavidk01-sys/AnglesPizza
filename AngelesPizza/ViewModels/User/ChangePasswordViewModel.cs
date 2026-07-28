using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.ViewModels.User
{
    public class ChangePasswordViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Nueva Contraseña")]
        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmar Contraseña")]
        [Required]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}