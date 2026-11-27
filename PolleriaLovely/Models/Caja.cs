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

        [Range(typeof(decimal), "1.00", "500.00", ErrorMessage = "El monto inicial no puede ser negativo.")]
        [RegularExpression(@"^\d+([.,]\d{1,2})?$", ErrorMessage = "El monto inicial no puede tener más de 2 decimales.")]
        public decimal MontoInicial { get; set; }

        [Range(typeof(decimal), "0.00", "100000.00", ErrorMessage = "El monto contado no puede ser negativo.")]
        [RegularExpression(@"^\d+([.,]\d{1,2})?$", ErrorMessage = "El monto contado no puede tener más de 2 decimales.")]
        public decimal? MontoContado { get; set; }

        public decimal? Diferencia { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe identificar al usuario.")]
        public int IdUsuario { get; set; }

        [ValidateNever]
        public Usuario? Usuario { get; set; }

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
