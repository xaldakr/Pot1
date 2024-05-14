using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pot1_API.Models
{
    public class Notificacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_notificacion { get; set; }

        [Required]
        [StringLength(500)]
        public string dato { get; set; }

        [Required]
        [StringLength(400)]
        public string url_archivo { get; set; }

        public bool notificar_cliente { get; set; }

        public DateTime fecha { get; set; }

        public bool autogenerada { get; set; }

        public int? remitente { get; set; }

        public int id_ticket { get; set; }

    }
}
