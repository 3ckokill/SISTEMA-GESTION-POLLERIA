using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        [Required, StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required, StringLength(100)]
        public string Contrasena { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(100)]
        public string Apellido { get; set; }

        [Required, StringLength(150)]
        public string Correo { get; set; }

        [StringLength(9)]
        public string Telefono { get; set; }

        [Required]
        public bool Estado { get; set; }

        public int IdRol { get; set; }
        public Rol Rol { get; set; }

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
