using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pot1_API.Models
{
    public class Archivo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_archivo { get; set; }

        [Required]
        [StringLength(400)]
        public string url { get; set; }

        [Required]
        public int id_ticket { get; set; }
    }
}
