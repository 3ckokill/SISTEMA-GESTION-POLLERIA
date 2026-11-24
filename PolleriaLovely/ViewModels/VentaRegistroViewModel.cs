using System.ComponentModel.DataAnnotations;
using PolleriaLovely.Models;

namespace PolleriaLovely.ViewModels
{
    public class VentaRegistroViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
        [Range(1, 4, ErrorMessage = "Debe seleccionar un método de pago válido.")]
        public MetodoPago MetodoPago { get; set; }

        public List<LineaVentaInput> Detalles { get; set; } = new List<LineaVentaInput>();
    }

    public class LineaVentaInput
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto.")]
        public int IdProducto { get; set; }

        [Range(1, ReglasNegocio.MaxCantidadPorLinea, ErrorMessage = "La cantidad debe estar entre 1 y 99.")]
        public int Cantidad { get; set; } = 1;
    }
}
