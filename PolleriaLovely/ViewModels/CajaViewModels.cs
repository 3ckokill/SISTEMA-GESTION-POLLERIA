using System.ComponentModel.DataAnnotations;
using PolleriaLovely.Models;

namespace PolleriaLovely.ViewModels
{
    public class CajaPanelViewModel
    {
        public Caja? CajaActiva { get; set; }
        public List<Caja> Historial { get; set; } = new();
        public decimal TotalVentas { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalYapePlin { get; set; }
        public decimal TotalOtro { get; set; }
        public bool EsAdministrador { get; set; }
    }

    public class CajaAperturaViewModel
    {
        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El monto inicial no puede ser negativo.")]
        public decimal MontoInicial { get; set; }
    }

    public class CajaCierreViewModel
    {
        public int IdCaja { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal EfectivoEsperado { get; set; }

        [Required(ErrorMessage = "Debe ingresar el efectivo contado.")]
        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El efectivo contado no puede ser negativo.")]
        public decimal? MontoContado { get; set; }
    }
}
