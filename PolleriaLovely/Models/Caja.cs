using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Caja
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCaja { get; set; }

        [Required]
        public DateTime FechaApertura { get; set; }

        public DateTime? FechaCierre { get; set; }

        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El monto inicial no puede ser negativo.")]
        public decimal MontoInicial { get; set; }

        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El monto contado no puede ser negativo.")]
        public decimal? MontoContado { get; set; }

        public decimal? Diferencia { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [ValidateNever]
        public Usuario? Usuario { get; set; }

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
