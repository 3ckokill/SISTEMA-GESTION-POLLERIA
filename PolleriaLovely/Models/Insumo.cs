using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Insumo
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdInsumo { get; set; }
        [Required(ErrorMessage = "El nombre del insumo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del insumo no puede superar los 100 caracteres.")]
        [RegularExpression(ReglasNegocio.PatronSoloLetras, ErrorMessage = "El nombre del insumo solo puede contener letras.")]
        public string NombreInsumo { get; set; } = string.Empty;
        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        public string UnidadMedida { get; set; } = string.Empty;
        [Range(typeof(decimal), "0", "10000", ErrorMessage = "El stock debe estar entre 0 y 10,000.")]
        public decimal Stock { get; set; }
        [Range(typeof(decimal), "0", "10000", ErrorMessage = "El stock mínimo debe estar entre 0 y 10,000.")]
        public decimal StockMinimo { get; set; }
        [Required]
        public bool Estado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido.")]
        public int IdProveedor { get; set; }
        [ValidateNever]
        public Proveedor? Proveedor { get; set; }

        public ICollection<ProductoInsumo> Recetas { get; set; } = new List<ProductoInsumo>();
    }
}

