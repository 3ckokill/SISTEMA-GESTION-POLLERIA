using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Mesa
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdMesa { get; set; }

        [Required(ErrorMessage = "El número de mesa es obligatorio.")]
        [Range(1, 1000, ErrorMessage = "El número de mesa no es válido.")]
        public int Numero { get; set; }

        [Required]
        public bool Estado { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
