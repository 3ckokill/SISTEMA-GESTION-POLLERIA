using Microsoft.AspNetCore.Mvc.Rendering;

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
        public int IdProducto { get; set; }

        public int IdInsumo { get; set; }

        public string Cantidad { get; set; } = string.Empty;
    }
}
