using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely.Models
{
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCategoria { get; set; }
       
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de la categoría no puede superar los 50 caracteres.")]

        public string NombreCategoria { get; set; }
        [Required(ErrorMessage = "La descripción de la categoría es obligatoria.")]
        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]

        public string Descripcion { get; set; }
        [Required]
        public bool Estado { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
