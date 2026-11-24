using System.ComponentModel.DataAnnotations;

namespace PolleriaLovely.ViewModels
{
    public class RegistrarEntradaInsumoViewModel
    {
        public int IdInsumo { get; set; }

        public string NombreInsumo { get; set; } = string.Empty;

        public string UnidadMedida { get; set; } = string.Empty;

        public decimal StockActual { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad recibida.")]
        public string Cantidad { get; set; } = string.Empty;
    }
}
