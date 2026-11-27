namespace PolleriaLovely.Models
{
    // limites del negocio
    public static class ReglasNegocio
    {
        public const int MaxCantidadPorLinea = 99;
        public const int MaxLineasPorPedido = 20;
        public const int MaxUnidadesPorPedido = 200;
        public const decimal MaxStockInsumo = 10000m;

        // Nombres: solo letras (con tildes, ñ y ü); admite espacios entre palabras.
        public const string PatronSoloLetras =
            @"^ *[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+(?: +[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+)* *$";

        // Descripciones: más de 10 caracteres sin contar los espacios al inicio ni al final.
        public const string PatronDescripcion = @"^\s*\S[\s\S]{9,}\S\s*$";
    }
}
