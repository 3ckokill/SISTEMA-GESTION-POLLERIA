using System.ComponentModel.DataAnnotations;

namespace PolleriaLovely.ViewModels
{
    public class PedidoAgregarViewModel
    {
        public List<LineaPedidoInput> Detalles { get; set; } = new List<LineaPedidoInput>();
    }
}
