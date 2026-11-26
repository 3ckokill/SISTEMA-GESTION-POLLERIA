using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PolleriaLovely.ViewModels
{
    public class RecetaProductoViewModel
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public List<RecetaLineaViewModel> Lineas { get; set; } = new();

        public List<SelectListItem> InsumosDisponibles { get; set; } = new();
    }

    public class RecetaLineaViewModel
    {
        public int IdProductoInsumo { get; set; }

        public string NombreInsumo { get; set; } = string.Empty;

        public string UnidadMedida { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }
    }

    public class AgregarRecetaLineaViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
        public int IdProducto { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un insumo válido.")]
        public int IdInsumo { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad.")]
        public string Cantidad { get; set; } = string.Empty;
    }
}
