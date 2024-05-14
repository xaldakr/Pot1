using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pot1_API.Models
{
    public class Tarea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_tarea { get; set; }

        [Required]
        [StringLength(70)]
        public string nombre { get; set; }

        [Required]
        [StringLength(250)]
        public string info { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("^(BAJA|NORMAL|IMPORTANTE|CRÍTICA)$")]
        public string prioridad { get; set; }

        [Required]
        public string estado { get; set; }

        public bool completada { get; set; }

        [Required]
        public int id_ticket { get; set; }

        public int? id_encargado { get; set; }
    }
}
