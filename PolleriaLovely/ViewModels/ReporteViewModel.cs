using System.ComponentModel.DataAnnotations;
using PolleriaLovely.Models;

namespace PolleriaLovely.ViewModels
{
    public class ReporteViewModel
    {
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaFinal { get; set; }

        public bool ReporteGenerado { get; set; }

        public decimal TotalVendido { get; set; }

        public int CantidadVentas { get; set; }

        public decimal PromedioVenta { get; set; }

        public List<VentaPorCajeroViewModel> VentasPorCajero { get; set; } = new List<VentaPorCajeroViewModel>();

        public List<ProductoVendidoViewModel> ProductosMasVendidos { get; set; } = new List<ProductoVendidoViewModel>();

        public List<VentaPorDiaViewModel> VentasPorDia { get; set; } = new List<VentaPorDiaViewModel>();
    }

    public class VentaPorCajeroViewModel
    {
        public string Cajero { get; set; } = string.Empty;

        public int CantidadVentas { get; set; }

        public decimal Total { get; set; }
    }

    public class ProductoVendidoViewModel
    {
        public string NombreProducto { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal Importe { get; set; }
    }

    public class VentaPorDiaViewModel
    {
        public DateTime Fecha { get; set; }

        public int CantidadVentas { get; set; }

        public decimal Total { get; set; }
    }
}
