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

        // Letras (con tildes, ñ, ü), números y espacios. Sin caracteres especiales.
        public const string PatronAlfanumerico =
            @"^ *[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9/]+(?: +[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9/]+)* *$";

        // Descripciones: más de 10 caracteres sin contar ningún espacio
        public const string PatronDescripcionAlfanumerica =
            @"^(?=(?: *[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9]){11})[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9 ]+$";

    }
}
