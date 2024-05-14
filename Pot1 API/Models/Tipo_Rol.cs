using System.ComponentModel.DataAnnotations;

namespace Pot1_API.Models
{
    public class Tipo_Rol
    {
        [Key]
        public int id_tipo_rol { get; set; }

        [Required]
        [StringLength(20)]
        public string nombre { get; set; }
    }
}
