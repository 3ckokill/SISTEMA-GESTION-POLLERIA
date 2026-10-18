# AGENTS.md

Instrucciones permanentes para trabajar en **PolleriaLovely**, el sistema de gestión de la pollería **Lovely Chicken**.

## Stack tecnológico

- C# / ASP.NET Core **MVC** (.NET 10, `Nullable` habilitado, `ImplicitUsings` habilitado)
- **Entity Framework Core 10** (`Microsoft.EntityFrameworkCore.SqlServer` 10.0.12)
- **SQL Server**; la cadena de conexión vive en `appsettings.json` → `ConnectionStrings:DefaultConnection` y puede variar entre equipos.
- Bootstrap y jQuery incluidos en `wwwroot/lib` (no descargar librerías nuevas sin necesidad).

## Estructura del proyecto

```
PolleriaLovely/
├── Controllers/      # Administrador, Cajero, Categorias, Home, Mozo, Productos, Usuario
├── Data/             # BibliotecaDbContext e InicializadorDatos
├── Migrations/       # Migraciones EF Core
├── Models/           # Categoria, Insumo, Producto, Proveedor, Rol, Usuario, ErrorViewModel
├── ViewModels/       # ViewModels (ej. LoginViewModel)
├── Views/            # Vistas Razor
├── Properties/       # launchSettings.json
├── wwwroot/          # css, js, lib
├── Program.cs        # Servicios, autenticación, inicialización y rutas
└── appsettings.json  # Conexión a BD y logging
```

## Reglas de trabajo

1. **Nombres en español** para clases, propiedades, controladores, vistas y variables siempre que sea razonable.
2. **Respeta la arquitectura MVC** y la estructura existente; sigue el estilo de los archivos vecinos.
3. **Modifica únicamente los archivos necesarios** para cada tarea; no toques archivos fuera del alcance pedido.
4. **Avanza en etapas pequeñas**: una responsabilidad por tarea.
5. **Compila después de cada cambio de código** cuando sea posible (`dotnet build`). Debe terminar en **0 errores**. Si no se ejecutaron las pruebas, indicarlo explícitamente.
6. **No ejecutes migraciones** (`dotnet ef migrations add/...`) sin que se pida explícitamente.
7. **Explica brevemente qué modificaste y por qué** al terminar cada tarea, para facilitar el aprendizaje.
8. **No inventes requisitos** ni hagas refactorizaciones innecesarias; si falta información, pregunta antes de asumir.
9. Antes de editar, revisa los archivos actuales y la rama de trabajo. No hagas merge a `main` sin autorización explícita.

## Seguridad

- Nunca almacenar contraseñas de usuarios en texto plano: guardar únicamente el hash generado por `IPasswordHasher<Usuario>`.
- Las cuentas de prueba y sus contraseñas conocidas solo pueden usarse en entorno **Development**. No habilitar esta inicialización en producción ni reutilizar estas contraseñas.
- No commitear secretos reales; `appsettings.json` no debe contener credenciales sensibles.

## Estado actual (actualizar cada vez que se modifique el proyecto)

- **Rama de trabajo:** `RamaMac`, fusionada en `main` con merge directo (fast-forward) el 18/10/2026. La publicación en `origin` queda pendiente de decisión del usuario. El PR [#1 — Sprint 1](https://github.com/3ckokill/PaginaWeb---GestionPolleria/pull/1) (`sprint1-seguridad`) no se tocó.
- **Pull Request:** [#1 — Sprint 1: seguridad y autorización por roles](https://github.com/3ckokill/PaginaWeb---GestionPolleria/pull/1). No fusionar hasta completar las pruebas.
- **Autenticación:** `UsuarioController` verifica la contraseña con `PasswordHasher<Usuario>`, genera claims de nombre y rol y crea una cookie de autenticación. `Program.cs` configura `LoginPath = /Usuario/Login`, `AccessDeniedPath = /Usuario/AccesoDenegado` y el middleware de autenticación/autorización.
- **Protección por rol:** `ProductosController`, `CategoriasController` y `AdministradorController` requieren `Administrador`; `CajeroController` requiere `Cajero`; `MozoController` requiere `Mozo`.
- **Inicialización:** `Data/InicializadorDatos.cs` crea los roles que falten: Administrador, Cajero y Mozo.
- **Cuentas de prueba en Development:** `Program.cs` crea cada usuario solo si no existe y si encuentra su rol; las contraseñas se almacenan como hash.
  - Administrador: usuario `admin.prueba`, contraseña inicial `LovelyDemo2026!`.
  - Cajero: usuario `cajero.prueba`, contraseña inicial `LovelyCaja2026!`.
  - Mozo: usuario `mozo.prueba`, contraseña inicial `LovelyMozo2026!`.
  - Si una cuenta ya existía, el inicializador no cambia su contraseña ni su rol. Estas credenciales son solo para pruebas locales; no usar en producción.
- **Vistas:** existen vistas para login, acceso denegado y los paneles de Administrador y Cajero; Productos y Categorías tienen vistas CRUD. **Los parciales `Views/Productos/_Form.cshtml` y `Views/Categorias/_Form.cshtml` fueron eliminados por decisión del usuario**: cada `Create.cshtml` y `Edit.cshtml` contiene su formulario completo inline, manteniendo el mismo HTML/interfaz y su sección `Scripts` con `_ValidationScriptsPartial`. El menú compartido muestra Home/Privacy y, para una sesión autenticada, el usuario, el rol y el botón de cierre de sesión; aún no hay enlaces de navegación a todos los módulos.
- **Base de datos/migraciones:** `appsettings.json` apunta a **`MAC-PUCHO\SQLEXPRESS01` / base `DB_LovelyChicken`** (migraciones aplicadas ahí; la BD vieja `PolleriaLovely` en `.\SQLEXPRESS01` tiene otros datos históricos y NO es la de la app). No se añadió ninguna migración nueva. Estado de datos: solo el producto «Helado de Lucuma» (id 6, S/ 6.00, Heladito, activo) y las categorías «Heladito» (id 1) y «Pollos» (id 3).
- **Arquitectura de validación (decisión del usuario):** los métodos `Normalizar*` viven en los controladores (el refactor a *model binders* se hizo, no gustó y se revirtió; los archivos `Models/ModelBinderPrecio.cs` y `Models/ModelBinderIdCategoria.cs` fueron eliminados). En `ProductosController`: `NormalizarErrorPrecio` (con la heurística privada `ValidarFormatoPrecio`: coma con ≠3 dígitos → decimal, con ambos separadores manda el último, parse con `InvariantCulture`, rango 0.01–1'000.000) y `NormalizarErrorCategoria`; en `CategoriasController`: `NormalizarErroresCategoria` (redundante con el `[Required]` del modelo). Ambos métodos hacen `ModelState.Remove` + `SetModelValue(texto)` y **marcan `ValidationState` explícito: `Valid` en éxito e `Invalid` en error** (y el chequeo `CategoriaExiste` también marca `Invalid` tras su `AddModelError`). Se conservan `CategoriaExiste` y `ProductoExists`.
- **Estado de validación:** E2E completo en verde tras el revert (login, «10,55»→10.55, «1,234.56»→1234.56, «abc» y «0» → «Ingrese un precio válido mayor que cero.» con redisplay del valor tecleado, categoría vacía → «Debe seleccionar una categoría.», id 999 → «La categoría seleccionada no es válida.», nombre de categoría vacío → «El nombre de la categoría es obligatorio.», Create/Edit/Delete con id correcto y limpieza total de datos de prueba). `dotnet build --no-restore` finaliza con 0 errores. Prueba manual pendiente del usuario en el navegador.
- **Incidentes de pruebas previos:** (1) «Coquita 100ml» fue borrado por el usuario entre sesiones; (2) en la tanda de binders un `grep -E` greedy extrajo el último enlace Edit y borró por error «Helado de Lucuma» (id 6); fue restaurado idéntico por SQL (nombre, «helado de tres boloñas», S/ 6.00, Heladito, activo, id 6 con `IDENTITY_INSERT`).
- **Lección aprendida (importante):** tres fixes estuvieron solo en el árbol de trabajo sin commitearse —(a) `SetModelValue` deja la entrada como `Unvalidated` → `ModelState.IsValid` queda falso **en silencio** (200 con spans de error vacíos y nada se guarda; se soluciona marcando `ValidationState` a `Valid`/`Invalid`), (b) la heurística de coma/punto «10,50»→10.50 (sin ella la coma es separador de miles en es-PE y se guarda 1050), (c) el borrado de los `_Form` — al restaurar un commit es fácil perderlos y reintroducir bugs viejos; verificar siempre con E2E tras cualquier revert. Además: en POSIX ERE (`grep -E`) no hay cuantificadores perezosos (partir el HTML por `</tr>` antes de buscar enlaces de fila) y comprobar siempre a qué BD apunta la cadena de conexión.


