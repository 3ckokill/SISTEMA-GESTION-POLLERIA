using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Gasto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdGasto { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public CategoriaGasto Categoria { get; set; }

        [Range(typeof(decimal), "0.50", "100000.00", ErrorMessage = "El monto debe ser mayor que cero.")]
        [RegularExpression(@"^\d+([.,]\d{1,2})?$", ErrorMessage = "El monto no puede tener más de 2 decimales.")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public MetodoPago MetodoPago { get; set; } = MetodoPago.NoRegistrado;

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public bool Anulado { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe identificar al usuario.")]
        public int IdUsuario { get; set; }

        [ValidateNever]
        public Usuario? Usuario { get; set; }
    }
}
