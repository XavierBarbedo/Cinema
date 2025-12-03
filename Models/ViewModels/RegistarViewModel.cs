using System.ComponentModel.DataAnnotations;

namespace Cinema.Models.ViewModels
{
    public class RegistarViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; }

        [Required, EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória")]
        public string Password { get; set; }
    }
}
