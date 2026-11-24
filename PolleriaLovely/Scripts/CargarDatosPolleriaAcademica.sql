SET NOCOUNT ON;

DECLARE @ProveedorAvicola INT = (SELECT TOP 1 IdProveedor FROM Proveedores WHERE NombreProveedor = N'Avicola El plumas');
DECLARE @ProveedorVerduras INT = (SELECT TOP 1 IdProveedor FROM Proveedores WHERE NombreProveedor = N'Verduras Campo');
DECLARE @ProveedorMayorista INT = (SELECT TOP 1 IdProveedor FROM Proveedores WHERE NombreProveedor = N'Mayorista Juan');
DECLARE @CategoriaPollos INT = (SELECT TOP 1 IdCategoria FROM Categorias WHERE NombreCategoria LIKE N'Pollos%');
DECLARE @CategoriaAcompanamientos INT = (SELECT TOP 1 IdCategoria FROM Categorias WHERE NombreCategoria LIKE N'Acompa%');

IF @ProveedorAvicola IS NULL OR @ProveedorVerduras IS NULL OR @ProveedorMayorista IS NULL
    THROW 51000, 'Faltan proveedores base para cargar los datos academicos.', 1;
IF @CategoriaPollos IS NULL OR @CategoriaAcompanamientos IS NULL
    THROW 51001, 'Faltan categorias base para cargar los datos academicos.', 1;

DECLARE @Insumos TABLE (Nombre NVARCHAR(100), Unidad NVARCHAR(50), Stock DECIMAL(10,2), StockMinimo DECIMAL(10,2), IdProveedor INT);
INSERT INTO @Insumos VALUES
    (N'Pollo entero', N'Kilogramo', 20, 5, @ProveedorAvicola),
    (N'Papa', N'Kilogramo', 30, 5, @ProveedorMayorista),
    (N'Lechuga', N'Kilogramo', 10, 2, @ProveedorVerduras),
    (N'Tomate', N'Kilogramo', 10, 2, @ProveedorVerduras),
    (N'Zanahoria', N'Kilogramo', 5, 1, @ProveedorVerduras),
    (N'Beterraga', N'Kilogramo', 5, 1, @ProveedorVerduras),
    (N'Ajo molido', N'Gramo', 1000, 200, @ProveedorMayorista),
    (N'Ají panca', N'Gramo', 2000, 300, @ProveedorMayorista),
    (N'Comino', N'Gramo', 500, 100, @ProveedorMayorista),
    (N'Orégano', N'Gramo', 250, 50, @ProveedorMayorista),
    (N'Sal', N'Gramo', 5000, 1000, @ProveedorMayorista),
    (N'Sillao', N'Mililitro', 5000, 1000, @ProveedorMayorista),
    (N'Vinagre', N'Mililitro', 3000, 500, @ProveedorMayorista),
    (N'Cerveza negra', N'Mililitro', 10000, 2000, @ProveedorMayorista),
    (N'Aceite vegetal', N'Mililitro', 10000, 2000, @ProveedorMayorista);

INSERT INTO Insumos (NombreInsumo, UnidadMedida, Stock, StockMinimo, Estado, IdProveedor)
SELECT i.Nombre, i.Unidad, i.Stock, i.StockMinimo, 1, i.IdProveedor
FROM @Insumos i
WHERE NOT EXISTS (SELECT 1 FROM Insumos x WHERE x.NombreInsumo = i.Nombre);

DECLARE @Productos TABLE (Nombre NVARCHAR(100), Descripcion NVARCHAR(200), Precio DECIMAL(10,2), IdCategoria INT);
INSERT INTO @Productos VALUES
    (N'1/4 Pollo a la Brasa', N'Cuarto de pollo con papas y ensalada.', 15.00, @CategoriaPollos),
    (N'1/2 Pollo a la Brasa', N'Medio pollo con papas y ensalada.', 28.00, @CategoriaPollos),
    (N'Pollo Entero a la Brasa', N'Pollo entero con papas y ensalada.', 52.00, @CategoriaPollos),
    (N'Papas Fritas Personal', N'Porción personal de papas fritas.', 8.00, @CategoriaAcompanamientos),
    (N'Ensalada de Pollería', N'Ensalada fresca de acompañamiento.', 5.00, @CategoriaAcompanamientos);

INSERT INTO Productos (NombreProducto, Descripcion, Precio, Estado, IdCategoria)
SELECT p.Nombre, p.Descripcion, p.Precio, 1, p.IdCategoria
FROM @Productos p
WHERE NOT EXISTS (SELECT 1 FROM Productos x WHERE x.NombreProducto = p.Nombre);

DECLARE @Recetas TABLE (Producto NVARCHAR(100), Insumo NVARCHAR(100), Cantidad DECIMAL(10,4));
INSERT INTO @Recetas VALUES
    (N'1/4 Pollo a la Brasa', N'Pollo entero', 0.3750), (N'1/4 Pollo a la Brasa', N'Papa', 0.2000),
    (N'1/4 Pollo a la Brasa', N'Lechuga', 0.0500), (N'1/4 Pollo a la Brasa', N'Tomate', 0.0250),
    (N'1/4 Pollo a la Brasa', N'Zanahoria', 0.0200), (N'1/4 Pollo a la Brasa', N'Beterraga', 0.0250),
    (N'1/4 Pollo a la Brasa', N'Ajo molido', 1.0000), (N'1/4 Pollo a la Brasa', N'Ají panca', 7.5000),
    (N'1/4 Pollo a la Brasa', N'Comino', 1.0000), (N'1/4 Pollo a la Brasa', N'Orégano', 0.2500),
    (N'1/4 Pollo a la Brasa', N'Sal', 1.2500), (N'1/4 Pollo a la Brasa', N'Sillao', 7.5000),
    (N'1/4 Pollo a la Brasa', N'Vinagre', 3.7500), (N'1/4 Pollo a la Brasa', N'Cerveza negra', 25.0000),
    (N'1/4 Pollo a la Brasa', N'Aceite vegetal', 7.5000),
    (N'1/2 Pollo a la Brasa', N'Pollo entero', 0.7500), (N'1/2 Pollo a la Brasa', N'Papa', 0.4000),
    (N'1/2 Pollo a la Brasa', N'Lechuga', 0.1000), (N'1/2 Pollo a la Brasa', N'Tomate', 0.0500),
    (N'1/2 Pollo a la Brasa', N'Zanahoria', 0.0400), (N'1/2 Pollo a la Brasa', N'Beterraga', 0.0500),
    (N'1/2 Pollo a la Brasa', N'Ajo molido', 2.0000), (N'1/2 Pollo a la Brasa', N'Ají panca', 15.0000),
    (N'1/2 Pollo a la Brasa', N'Comino', 2.0000), (N'1/2 Pollo a la Brasa', N'Orégano', 0.5000),
    (N'1/2 Pollo a la Brasa', N'Sal', 2.5000), (N'1/2 Pollo a la Brasa', N'Sillao', 15.0000),
    (N'1/2 Pollo a la Brasa', N'Vinagre', 7.5000), (N'1/2 Pollo a la Brasa', N'Cerveza negra', 50.0000),
    (N'1/2 Pollo a la Brasa', N'Aceite vegetal', 15.0000),
    (N'Pollo Entero a la Brasa', N'Pollo entero', 1.5000), (N'Pollo Entero a la Brasa', N'Papa', 0.8000),
    (N'Pollo Entero a la Brasa', N'Lechuga', 0.2000), (N'Pollo Entero a la Brasa', N'Tomate', 0.1000),
    (N'Pollo Entero a la Brasa', N'Zanahoria', 0.0800), (N'Pollo Entero a la Brasa', N'Beterraga', 0.1000),
    (N'Pollo Entero a la Brasa', N'Ajo molido', 4.0000), (N'Pollo Entero a la Brasa', N'Ají panca', 30.0000),
    (N'Pollo Entero a la Brasa', N'Comino', 4.0000), (N'Pollo Entero a la Brasa', N'Orégano', 1.0000),
    (N'Pollo Entero a la Brasa', N'Sal', 5.0000), (N'Pollo Entero a la Brasa', N'Sillao', 30.0000),
    (N'Pollo Entero a la Brasa', N'Vinagre', 15.0000), (N'Pollo Entero a la Brasa', N'Cerveza negra', 100.0000),
    (N'Pollo Entero a la Brasa', N'Aceite vegetal', 30.0000),
    (N'Papas Fritas Personal', N'Papa', 0.2000), (N'Papas Fritas Personal', N'Aceite vegetal', 20.0000),
    (N'Papas Fritas Personal', N'Sal', 3.0000),
    (N'Ensalada de Pollería', N'Lechuga', 0.0500), (N'Ensalada de Pollería', N'Tomate', 0.0250),
    (N'Ensalada de Pollería', N'Zanahoria', 0.0200), (N'Ensalada de Pollería', N'Beterraga', 0.0250),
    (N'Ensalada de Pollería', N'Aceite vegetal', 5.0000), (N'Ensalada de Pollería', N'Vinagre', 5.0000),
    (N'Ensalada de Pollería', N'Sal', 1.0000);

INSERT INTO ProductoInsumos (IdProducto, IdInsumo, Cantidad)
SELECT p.IdProducto, i.IdInsumo, r.Cantidad
FROM @Recetas r
INNER JOIN Productos p ON p.NombreProducto = r.Producto
INNER JOIN Insumos i ON i.NombreInsumo = r.Insumo
WHERE NOT EXISTS (
    SELECT 1 FROM ProductoInsumos pi
    WHERE pi.IdProducto = p.IdProducto AND pi.IdInsumo = i.IdInsumo
);

SELECT 'Insumos academicos' AS Concepto, COUNT(*) AS Total FROM Insumos WHERE NombreInsumo IN (SELECT Nombre FROM @Insumos)
UNION ALL
SELECT 'Productos academicos', COUNT(*) FROM Productos WHERE NombreProducto IN (SELECT Nombre FROM @Productos)
UNION ALL
SELECT 'Lineas de receta', COUNT(*) FROM ProductoInsumos pi INNER JOIN Productos p ON p.IdProducto = pi.IdProducto WHERE p.NombreProducto IN (SELECT Producto FROM @Recetas);
