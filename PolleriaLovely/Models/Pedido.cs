using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Pedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [Required(ErrorMessage = "Debe seleccionar una mesa.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una mesa válida.")]
        public int IdMesa { get; set; }

        [ValidateNever]
        public Mesa Mesa { get; set; }

        [Required(ErrorMessage = "Debe identificar al mozo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe identificar al mozo.")]
        public int IdUsuario { get; set; }

        [ValidateNever]
        public Usuario Usuario { get; set; }

        public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();
    }
}
