using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Cinema.Models
{
    public class Sessao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O filme é obrigatório")]
        public int FilmeId { get; set; }

        [ForeignKey("FilmeId")]
        public virtual Filme Filme { get; set; }

        [Required(ErrorMessage = "A sala é obrigatória")]
        [StringLength(50)]
        public string Sala { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias")]
        public DateTime DataHora { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 100.00, ErrorMessage = "O preço deve estar entre 0.01 e 100.00")]
        public decimal Preco { get; set; }

        [Required]
        [Range(1, 500, ErrorMessage = "A capacidade deve estar entre 1 e 500")]
        public int CapacidadeTotal { get; set; }

        public int LugaresDisponiveis { get; set; }

        public virtual ICollection<Reserva> Reservas { get; set; }
    }
}
