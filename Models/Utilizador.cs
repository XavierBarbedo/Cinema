using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Cinema.Models
{
    public class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória")]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Cliente"; // "Cliente" ou "Administrador"

        public virtual ICollection<Reserva> Reservas { get; set; }
    }
}
