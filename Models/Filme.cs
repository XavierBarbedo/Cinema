using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Cinema.Models
{
    public class Filme
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200)]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "O género é obrigatório")]
        [StringLength(50)]
        public string Genero { get; set; }

        [Required(ErrorMessage = "A duração é obrigatória")]
        [Range(1, 500, ErrorMessage = "A duração deve estar entre 1 e 500 minutos")]
        public int Duracao { get; set; } // em minutos

        [StringLength(500)]
        public string Sinopse { get; set; }

        [StringLength(500)]
        public string Capa { get; set; } // URL ou caminho da imagem

        [DataType(DataType.Date)]
        public DateTime DataLancamento { get; set; }

        public virtual ICollection<Sessao> Sessoes { get; set; }
    }
}

