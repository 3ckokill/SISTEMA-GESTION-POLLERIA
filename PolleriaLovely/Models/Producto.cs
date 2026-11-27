using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PolleriaLovely.Models
{
    public class Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del producto no puede superar los 100 caracteres.")]
        [RegularExpression(ReglasNegocio.PatronSoloLetras, ErrorMessage = "El nombre del producto solo puede contener letras.")]
        public string NombreProducto { get; set; } = string.Empty;
        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        [RegularExpression(ReglasNegocio.PatronDescripcion, ErrorMessage = "La descripción debe tener más de 10 caracteres (sin contar espacios al inicio o al final).")]

        public string? Descripcion { get; set; }
        [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        public bool Estado { get; set; }
        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int IdCategoria { get; set; }
        [ValidateNever]
        public Categoria? Categoria { get; set; }

        public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();

        public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

        public ICollection<ProductoInsumo> Receta { get; set; } = new List<ProductoInsumo>();
    }
}

