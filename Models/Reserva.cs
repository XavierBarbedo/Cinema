using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UtilizadorId { get; set; }

        [ForeignKey("UtilizadorId")]
        public virtual Utilizador Utilizador { get; set; }

        [Required]
        public int SessaoId { get; set; }

        [ForeignKey("SessaoId")]
        public virtual Sessao Sessao { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "O número de bilhetes deve estar entre 1 e 10")]
        public int NumeroBilhetes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        public DateTime DataReserva { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Estado { get; set; } = "Confirmada"; // Confirmada, Cancelada

        public string? LugaresMarcados { get; set; } // Ex: "A1,A2,A3"
    }
}
