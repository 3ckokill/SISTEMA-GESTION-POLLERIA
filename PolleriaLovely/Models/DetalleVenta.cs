using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class DetalleVenta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDetalleVenta { get; set; }

        [Required(ErrorMessage = "Debe pertenecer a una venta.")]
        public int IdVenta { get; set; }

        [ValidateNever]
        public Venta Venta { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
        public int IdProducto { get; set; }

        [ValidateNever]
        public Producto Producto { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "El precio unitario debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }

        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El subtotal no puede ser negativo.")]
        public decimal Subtotal { get; set; }
    }
}
