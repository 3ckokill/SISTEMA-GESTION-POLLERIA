using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Rol
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRol { get; set; }

        [Required, StringLength(50)]
        public string NombreRol { get; set; } = string.Empty;

        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public bool Estado { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
