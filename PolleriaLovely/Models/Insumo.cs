using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Insumo
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdInsumo { get; set; }
        [Required, StringLength(100)]
        public string NombreInsumo { get; set; }
        [Required]
        public string UnidadMedida { get; set; }
        [Required]
        public decimal Stock { get; set; }
        [Required]
        public decimal StockMinimo { get; set; }
        [Required]
        public bool Estado { get; set; }

        public int IdProveedor { get; set; }
        [ValidateNever]
        public Proveedor Proveedor { get; set; }
    }
}


