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

        public decimal TotalGastado { get; set; }

        public decimal UtilidadEstimada { get; set; }

        public List<VentaPorCajeroViewModel> VentasPorCajero { get; set; } = new List<VentaPorCajeroViewModel>();

        public List<ProductoVendidoViewModel> ProductosMasVendidos { get; set; } = new List<ProductoVendidoViewModel>();

        public List<VentaPorDiaViewModel> VentasPorDia { get; set; } = new List<VentaPorDiaViewModel>();

        public List<VentaPorMetodoPagoViewModel> VentasPorMetodoPago { get; set; } = new List<VentaPorMetodoPagoViewModel>();

        public List<ResumenCajaViewModel> ResumenCajas { get; set; } = new List<ResumenCajaViewModel>();
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

    public class VentaPorMetodoPagoViewModel
    {
        public MetodoPago MetodoPago { get; set; }

        public string NombreMetodo { get; set; } = string.Empty;

        public int CantidadVentas { get; set; }

        public decimal Total { get; set; }
    }

    public class ResumenCajaViewModel
    {
        public int IdCaja { get; set; }

        public string Cajero { get; set; } = string.Empty;

        public DateTime FechaApertura { get; set; }

        public DateTime? FechaCierre { get; set; }

        public decimal MontoInicial { get; set; }

        public decimal TotalVentas { get; set; }

        public decimal VentasEfectivo { get; set; }

        public decimal EfectivoEsperado { get; set; }

        public decimal? MontoContado { get; set; }

        public decimal? Diferencia { get; set; }
    }
}
