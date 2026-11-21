using System.ComponentModel.DataAnnotations;
using PolleriaLovely.Models;

namespace PolleriaLovely.ViewModels
{
    // Precuenta de un pedido: consulta de solo lectura con precios vigentes del catálogo.
    public class PrecuentaViewModel
    {
        public int IdPedido { get; set; }

        public int NumeroMesa { get; set; }

        public List<LineaPrecuenta> Lineas { get; set; } = new();

        public decimal Total { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
        [Range(1, 4, ErrorMessage = "Debe seleccionar un método de pago válido.")]
        public MetodoPago MetodoPago { get; set; }
    }

    public class LineaPrecuenta
    {
        public string NombreProducto { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
