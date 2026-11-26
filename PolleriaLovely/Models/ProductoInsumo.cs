using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class ProductoInsumo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProductoInsumo { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
        public int IdProducto { get; set; }

        public Producto Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un insumo válido.")]
        public int IdInsumo { get; set; }

        public Insumo Insumo { get; set; }

        [Range(typeof(decimal), "0.0001", "1000000", ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public decimal Cantidad { get; set; }
    }
}
