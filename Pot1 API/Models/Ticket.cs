using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pot1_API.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_ticket { get; set; }

        [Required]
        [StringLength(35)]
        [RegularExpression("^(CREADO|ASIGNADO|EN ESPERA DE INFORMACIÓN|EN RESOLUCIÓN|RESUELTO)$")]
        public string estado { get; set; }

        [Required]
        public string descripcion { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("^(BAJA|NORMAL|IMPORTANTE|CRÍTICA)$")]
        public string prioridad { get; set; }

        [Required]
        [StringLength(250)]
        public string servicio { get; set; }

        public bool resuelta { get; set; }

        public DateTime fecha { get; set; }

        public int? id_encargado { get; set; }

        public int id_cliente { get; set; }
    }
}
