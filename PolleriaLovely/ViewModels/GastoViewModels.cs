using System.ComponentModel.DataAnnotations;
using PolleriaLovely.Models;

namespace PolleriaLovely.ViewModels
{
    public class GastoEdicionViewModel
    {
        public int IdGasto { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, 6, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public CategoriaGasto Categoria { get; set; }

        [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Range(0, 4, ErrorMessage = "Debe seleccionar un método de pago válido.")]
        public MetodoPago MetodoPago { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }
    }

    public class GastoListadoViewModel
    {
        public int IdGasto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public CategoriaGasto Categoria { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool Anulado { get; set; }
    }
}
