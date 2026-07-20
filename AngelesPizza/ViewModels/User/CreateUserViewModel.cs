using System.ComponentModel.DataAnnotations;

namespace AngelesPizza.ViewModels.User
{
    public class CreateUserViewModel
    {
        [Display(Name = "Nombre Completo")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmar Contraseña")]
        [Required]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Seleccione un rol.")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;
    }
}