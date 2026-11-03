using System.ComponentModel.DataAnnotations;

namespace PolleriaLovely.ViewModels
{
    public class VentaRegistroViewModel
    {
        public List<LineaVentaInput> Detalles { get; set; } = new List<LineaVentaInput>();
    }

    public class LineaVentaInput
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto.")]
        public int IdProducto { get; set; }

        [Range(1, 999, ErrorMessage = "La cantidad debe estar entre 1 y 999.")]
        public int Cantidad { get; set; } = 1;
    }
}
