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
        public string NombreInsumo { get; set; }
        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        public string UnidadMedida { get; set; }
        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El stock no puede ser negativo.")]
        public decimal Stock { get; set; }
        [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public decimal StockMinimo { get; set; }
        [Required]
        public bool Estado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido.")]
        public int IdProveedor { get; set; }
        [ValidateNever]
        public Proveedor Proveedor { get; set; }
    }
}

