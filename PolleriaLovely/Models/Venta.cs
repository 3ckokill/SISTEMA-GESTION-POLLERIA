using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Venta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdVenta { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El total no puede ser negativo.")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un usuario válido.")]
        public int IdUsuario { get; set; }

        [ValidateNever]
        public Usuario Usuario { get; set; }

        public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
    }
}
