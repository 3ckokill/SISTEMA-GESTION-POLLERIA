using Microsoft.EntityFrameworkCore;
using PolleriaLovely.Data;
using PolleriaLovely.Models;

namespace PolleriaLovely.Services
{
    public class InventarioService
    {
        private readonly BibliotecaDbContext _context;

        public InventarioService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoConsumo> PrepararConsumoAsync(IEnumerable<(int IdProducto, int Cantidad)> lineas)
        {
            var cantidadesPorProducto = lineas
                .GroupBy(linea => linea.IdProducto)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(linea => linea.Cantidad));

            var recetas = await _context.ProductoInsumos
                .Include(pi => pi.Insumo)
                .Where(pi => cantidadesPorProducto.Keys.Contains(pi.IdProducto))
                .ToListAsync();

            var consumos = recetas
                .GroupBy(pi => pi.IdInsumo)
                .Select(grupo => new ConsumoInsumo
                {
                    Insumo = grupo.First().Insumo,
                    Cantidad = grupo.Sum(pi => pi.Cantidad * cantidadesPorProducto[pi.IdProducto])
                })
                .ToList();

            var faltante = consumos.FirstOrDefault(consumo => consumo.Insumo.Stock < consumo.Cantidad);
            if (faltante != null)
            {
                var disponible = faltante.Insumo.Stock;
                return ResultadoConsumo.Invalido(
                    $"Stock insuficiente de «{faltante.Insumo.NombreInsumo}». Necesitas {faltante.Cantidad:N2} {faltante.Insumo.UnidadMedida} y hay {disponible:N2}.");
            }

            return ResultadoConsumo.Valido(consumos);
        }

        public void AplicarConsumo(ResultadoConsumo resultado)
        {
            foreach (var consumo in resultado.Consumos)
                consumo.Insumo.Stock -= consumo.Cantidad;
        }
    }

    public class ResultadoConsumo
    {
        public bool EsValido { get; private init; }

        public string? Mensaje { get; private init; }

        public List<ConsumoInsumo> Consumos { get; private init; } = new();

        public static ResultadoConsumo Valido(List<ConsumoInsumo> consumos) =>
            new() { EsValido = true, Consumos = consumos };

        public static ResultadoConsumo Invalido(string mensaje) =>
            new() { EsValido = false, Mensaje = mensaje };
    }

    public class ConsumoInsumo
    {
        public Insumo Insumo { get; init; } = null!;

        public decimal Cantidad { get; init; }
    }
}
