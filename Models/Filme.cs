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

        [StringLength(2000)]
        public string Sinopse { get; set; }

        [StringLength(1000)]
        public string Capa { get; set; } // poster
        [StringLength(1000)]
        public string Background { get; set; }
        [StringLength(200)]
        public string? TrailerYoutubeId { get; set; }
        [StringLength(700)]
        public string Elenco { get; set; }
        [StringLength(200)]
        public string Realizador { get; set; }

        [DataType(DataType.Date)]
        public DateTime DataLancamento { get; set; }

        // NOVO CAMPO BOOLEAN
        public bool SempreNoCinema { get; set; }

        public virtual ICollection<Sessao> Sessoes { get; set; }
    }

}
