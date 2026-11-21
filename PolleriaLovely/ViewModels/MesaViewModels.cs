using System.ComponentModel.DataAnnotations;

namespace PolleriaLovely.ViewModels
{
    public class MesaListadoViewModel
    {
        public int IdMesa { get; set; }
        public int Numero { get; set; }
        public bool Estado { get; set; }
        public bool Ocupada { get; set; }
    }

    public class MesaEdicionViewModel
    {
        public int IdMesa { get; set; }

        [Range(1, 1000, ErrorMessage = "El número de mesa debe estar entre 1 y 1000.")]
        public int Numero { get; set; }

        public bool Estado { get; set; } = true;
    }
}
