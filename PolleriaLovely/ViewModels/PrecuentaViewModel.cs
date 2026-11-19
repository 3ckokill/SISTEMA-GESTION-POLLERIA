namespace PolleriaLovely.ViewModels
{
    // Precuenta de un pedido: consulta de solo lectura con precios vigentes del catálogo.
    public class PrecuentaViewModel
    {
        public int IdPedido { get; set; }

        public int NumeroMesa { get; set; }

        public List<LineaPrecuenta> Lineas { get; set; } = new();

        public decimal Total { get; set; }
    }

    public class LineaPrecuenta
    {
        public string NombreProducto { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
