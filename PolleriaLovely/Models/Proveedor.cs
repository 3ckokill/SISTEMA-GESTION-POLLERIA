using PolleriaLovely.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolleriaLovely;

public class Proveedor
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdProveedor { get; set; }
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del proveedor no puede superar los 100 caracteres.")]
    public string NombreProveedor { get; set; } = string.Empty;
    [Required(ErrorMessage = "El RUC es obligatorio.")]
    [StringLength(11, ErrorMessage = "El RUC no puede superar los 11 caracteres.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC debe tener exactamente 11 dígitos.")]
    public string RucProveedor { get; set; } = string.Empty;
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(9, ErrorMessage = "El teléfono no puede superar los 9 dígitos.")]
    [RegularExpression(@"^\d{1,9}$", ErrorMessage = "El teléfono debe contener solo dígitos.")]
    public string Telefono { get; set; } = string.Empty;
    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();

}
