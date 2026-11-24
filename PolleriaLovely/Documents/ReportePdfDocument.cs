using PolleriaLovely.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PolleriaLovely.Documents
{
    public sealed class ReportePdfDocument : IDocument
    {
        private readonly ReporteViewModel _reporte;

        public ReportePdfDocument(ReporteViewModel reporte)
        {
            _reporte = reporte;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().Column(column =>
                {
                    column.Item().Text("Reporte de ventas y gastos").FontSize(18).Bold();
                    column.Item().Text($"Periodo: {_reporte.FechaInicio:dd/MM/yyyy} al {_reporte.FechaFinal:dd/MM/yyyy}").FontSize(10);
                    column.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
                page.Content().Column(column =>
                {
                    column.Spacing(12);
                    AgregarResumen(column);
                    AgregarVentasPorCajero(column);
                    AgregarProductos(column);
                    AgregarVentasPorDia(column);
                    AgregarMetodosPago(column);
                    AgregarCajas(column);
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }

        private void AgregarResumen(ColumnDescriptor column)
        {
            AgregarTitulo(column, "Resumen");
            column.Item().Grid(grid =>
            {
                grid.Columns(5);
                AgregarDato(grid, "Total vendido", Dinero(_reporte.TotalVendido));
                AgregarDato(grid, "Ventas", _reporte.CantidadVentas.ToString());
                AgregarDato(grid, "Promedio", Dinero(_reporte.PromedioVenta));
                AgregarDato(grid, "Total gastado", Dinero(_reporte.TotalGastado));
                AgregarDato(grid, "Utilidad estimada", Dinero(_reporte.UtilidadEstimada));
            });
        }

        private void AgregarVentasPorCajero(ColumnDescriptor column)
        {
            AgregarTitulo(column, "Ventas por cajero");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });
                Encabezados(table, "Cajero", "Ventas", "Total");
                foreach (var item in _reporte.VentasPorCajero)
                    Fila(table, item.Cajero, item.CantidadVentas.ToString(), Dinero(item.Total));
            });
        }

        private void AgregarProductos(ColumnDescriptor column)
        {
            AgregarTitulo(column, "Productos más vendidos");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });
                Encabezados(table, "Producto", "Cantidad", "Importe");
                foreach (var item in _reporte.ProductosMasVendidos)
                    Fila(table, item.NombreProducto, item.Cantidad.ToString(), Dinero(item.Importe));
            });
        }

        private void AgregarVentasPorDia(ColumnDescriptor column)
        {
            AgregarTitulo(column, "Ventas por día");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });
                Encabezados(table, "Fecha", "Ventas", "Total");
                foreach (var item in _reporte.VentasPorDia)
                    Fila(table, item.Fecha.ToString("dd/MM/yyyy"), item.CantidadVentas.ToString(), Dinero(item.Total));
            });
        }

        private void AgregarMetodosPago(ColumnDescriptor column)
        {
            AgregarTitulo(column, "Ventas por método de pago");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });
                Encabezados(table, "Método", "Ventas", "Total");
                foreach (var item in _reporte.VentasPorMetodoPago)
                    Fila(table, item.NombreMetodo, item.CantidadVentas.ToString(), Dinero(item.Total));
            });
        }

        private void AgregarCajas(ColumnDescriptor column)
        {
            AgregarTitulo(column, "Resumen por caja");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });
                Encabezados(table, "Caja", "Cajero", "Ventas", "Efectivo esperado", "Contado", "Diferencia");
                foreach (var item in _reporte.ResumenCajas)
                {
                    Fila(table, $"#{item.IdCaja}", item.Cajero, Dinero(item.TotalVentas), Dinero(item.EfectivoEsperado),
                        item.MontoContado.HasValue ? Dinero(item.MontoContado.Value) : "Pendiente",
                        item.Diferencia.HasValue ? Dinero(item.Diferencia.Value) : "Pendiente");
                }
            });
        }

        private static void AgregarTitulo(ColumnDescriptor column, string titulo)
        {
            column.Item().PaddingTop(4).Text(titulo).FontSize(12).Bold();
        }

        private static void AgregarDato(GridDescriptor grid, string etiqueta, string valor)
        {
            grid.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(column =>
            {
                column.Item().Text(etiqueta).FontSize(8).FontColor(Colors.Grey.Darken1);
                column.Item().Text(valor).Bold();
            });
        }

        private static void Encabezados(TableDescriptor table, params string[] encabezados)
        {
            foreach (var encabezado in encabezados)
                table.Cell().Element(CeldaEncabezado).Text(encabezado);
        }

        private static void Fila(TableDescriptor table, params string[] valores)
        {
            foreach (var valor in valores)
                table.Cell().Element(Celda).Text(valor);
        }

        private static IContainer Celda(IContainer container) => container.Padding(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

        private static IContainer CeldaEncabezado(IContainer container) => container.Background(Colors.Grey.Lighten3).Padding(4).BorderBottom(1).BorderColor(Colors.Grey.Darken1).DefaultTextStyle(x => x.Bold());

        private static string Dinero(decimal valor) => $"S/ {valor:N2}";
    }
}
