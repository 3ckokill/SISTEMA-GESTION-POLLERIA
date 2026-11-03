using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class DetallePedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDetallePedido { get; set; }

        [Required(ErrorMessage = "Debe pertenecer a un pedido.")]
        public int IdPedido { get; set; }

        [ValidateNever]
        public Pedido Pedido { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
        public int IdProducto { get; set; }

        [ValidateNever]
        public Producto Producto { get; set; }

        [Range(1, 999, ErrorMessage = "La cantidad debe estar entre 1 y 999.")]
        public int Cantidad { get; set; }
    }
}
