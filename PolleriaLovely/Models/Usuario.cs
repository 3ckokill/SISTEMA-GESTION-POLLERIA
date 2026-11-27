using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        [RegularExpression(ReglasNegocio.PatronSoloLetras, ErrorMessage = "El nombre solo puede contener letras.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        [RegularExpression(ReglasNegocio.PatronSoloLetras, ErrorMessage = "El apellido solo puede contener letras.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Correo { get; set; } = string.Empty;

        [StringLength(9, ErrorMessage = "El teléfono no puede superar los 9 caracteres.")]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public bool Estado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido.")]
        public int IdRol { get; set; }

        [ValidateNever]
        public Rol? Rol { get; set; }

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
