# ASProducts

Aplicación full-stack para la gestión de productos (CRUD): listado, creación, edición y eliminación, con carga de imágenes y validación de formularios en tiempo real.

El proyecto está dividido en dos partes:
- **`ASProducts.Web`** — Frontend en React 19 + TypeScript
- **`ASProducts.Api`** — Backend en ASP.NET Core (.NET 10)

---

## Requisitos previos

| Herramienta | Versión |
|---|---|
| Node.js | 20.19.0+ |
| pnpm o bun | 9.0.0+ o 1.3.0+ |
| .NET SDK | 10 |
| LocalDB (SQL Server Express) | Incluido con Visual Studio, o instalable por separado |

> **Nota sobre el gestor de paquetes:** el frontend fue desarrollado originalmente con **Bun** por velocidad de instalación y ejecución. Se documentan también los comandos con **pnpm**, dada su mayor adopción, estabilidad y compatibilidad en entornos de integración continua (CI/CD) y equipos de desarrollo. Ambos gestores son totalmente compatibles con este proyecto.

---

## Puesta en marcha

El backend debe estar corriendo antes de usar el frontend contra la API real (si usas `VITE_USE_MOCK=false`).

### 1. Backend (`ASProducts.Api`)

**Configurar la cadena de conexión**

En `appsettings.json` (o `appsettings.Development.json`), agrega tu cadena de conexión a LocalDB:

```json
{
  "ConnectionStrings": {
    "ASProductsConnection": "Server=(localdb)\\mssqllocaldb;Database=ASProductsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Restaurar dependencias y ejecutar**

Las migraciones y el seed de datos se aplican automáticamente al arrancar la aplicación (ver `Program.cs`), así que no es necesario correr `dotnet ef database update` manualmente:

```bash
cd ASProducts.Api
dotnet restore
dotnet run
```

Por defecto la API queda disponible en `https://localhost:5001` (o el puerto configurado en `launchSettings.json`). En modo desarrollo puedes explorar los endpoints en la documentación interactiva de Scalar, disponible en `/scalar/v1`.

> El seed de datos (`SeedData.Initialize`) solo puebla la tabla `Products` si está vacía — no destruye datos existentes en cada arranque. Esto significa que los productos creados manualmente durante las pruebas se conservan entre reinicios de la API. Si en algún momento necesitas volver al estado inicial de demostración, puedes descomentar el bloque `Reset DBContext` dentro de `SeedData.cs`.

**Correr los tests del backend**

```bash
dotnet test
```

### 2. Frontend (`ASProducts.Web`)

**Instalación**

```bash
# Con pnpm
cd ASProducts.Web
pnpm install

# Con bun
cd ASProducts.Web
bun install
```

**Variables de entorno**

Este proyecto valida sus variables de entorno con Zod al arrancar (`src/config/env.ts`). Crea un archivo `.env` en la raíz del frontend con:

```env
VITE_USE_MOCK=false
VITE_API_BASE_URL=https://localhost:5001
```

| Variable | Descripción |
|---|---|
| `VITE_USE_MOCK` | `true` para trabajar contra datos mockeados en memoria (`product.mock.ts`), sin necesidad de backend. `false` para consumir la API real del backend .NET. |
| `VITE_API_BASE_URL` | URL base de la API. Debe coincidir con el puerto donde corre `ASProducts.Api`. |

> Si el archivo `.env` falta o las variables no son válidas, la aplicación lanza un error explícito al arrancar en vez de fallar silenciosamente.

**Levantar el servidor de desarrollo**

```bash
# Con pnpm
pnpm dev

# Con bun
bun dev
```

Por defecto Vite sirve en `http://localhost:5173` — este puerto está explícitamente habilitado en la política de CORS del backend (`Program.cs`), así que si cambias el puerto del frontend, debes actualizar también la política CORS en el backend.

### Scripts disponibles (frontend)

| Script | pnpm | bun |
|---|---|---|
| Desarrollo | `pnpm dev` | `bun dev` |
| Build | `pnpm build` | `bun run build` |
| Preview | `pnpm preview` | `bun run preview` |
| Lint | `pnpm lint` | `bun run lint` |
| Tests | `pnpm test` | `bun run test` |
| Tests (watch) | `pnpm test:watch` | `bun run test:watch` |

---

## Pruebas unitarias

### Frontend
**Vitest** + **React Testing Library** con entorno `jsdom`. La configuración vive en `vitest.config.ts` y el setup global en `src/test/setup.ts`.

Los tests están colocados junto al archivo que prueban, cubriendo:
- **Lógica pura**: validación de esquemas Zod (`product.schema.test.ts`)
- **Componentes**: renderizado, interacción de usuario y validación de formularios (`ProductForm.test.tsx`, `ImageUploader.test.tsx`)
- **Hooks personalizados**: estados de carga/éxito/error contra una API mockeada (`hooks.test.tsx`)
- **Accesibilidad y edge cases**: navegación por teclado, archivos inválidos, campos vacíos, eliminación explícita de imagen

```bash
# Con pnpm
pnpm test

# Con bun
bun run test
```

### Backend
**xUnit** + **Moq**, con distintas estrategias de aislamiento según la capa:
- **Repositorios**: EF Core InMemory provider (`ProductRepositoryTests`)
- **Orchestrator**: SQLite en modo `:memory:`, necesario porque el InMemory provider de EF Core no soporta transacciones explícitas (`BeginTransactionAsync`) y el Orchestrator depende de ellas
- **Servicios y Controllers**: mocks puros vía Moq, sin dependencias de infraestructura
- **`ImageStorageService`**: usa un directorio temporal real en disco (no mockeado), ya que el servicio hace I/O directo sin una abstracción de filesystem inyectable

```bash
dotnet test
```

---

## Estructura del proyecto

```
ASProducts-Web/
└── src/
    ├── api/                    # Cliente HTTP y llamadas a la API
    ├── assets/                  # Recursos estáticos
    ├── components/
    │   ├── common/               # Componentes compartidos (modal, toast, navbar...)
    │   └── ui/                    # Componentes de UI reutilizables (ImageUploader...)
    ├── config/                  # Configuración y variables de entorno
    ├── features/
    │   └── products/              # Lógica de dominio de productos (hooks, schema, formulario, mocks)
    ├── layouts/                  # Layouts de página
    ├── pages/                    # Páginas / vistas enrutadas
    ├── router/                    # Definición de rutas
    ├── scss/                      # Estilos globales
    └── test/
        ├── setup.ts                # Setup global de Vitest
        └── features/
            ├── components/ui/       # Tests de componentes de UI (ImageUploader)
            └── products/             # Tests de dominio (hooks, schema, ProductForm)

ASProducts.Api/
├── Controllers/               # Endpoints REST (thin controllers)
├── Data/
│   ├── Migrations/              # Migraciones de EF Core
│   ├── Seeds/Images/             # Imágenes de demostración copiadas al arrancar
│   └── ProductsContext.cs        # DbContext
├── DTOs/                       # Contratos de entrada/salida de la API
├── Mappings/                   # Perfiles de AutoMapper
├── Models/                     # Entidades de dominio (EF Core)
├── Orchestrators/               # Coordinación transaccional entre Product + Image
├── Repositories/                # Acceso a datos vía EF Core
├── Services/                    # Lógica de negocio (ProductService, ImageStorageService)
├── wwwroot/images/products/     # Imágenes de producto servidas como archivos estáticos
├── appsettings.json
└── Program.cs

ASProducts.Tests/
├── Controllers/                 # ProductsControllerTests
├── Orchestrators/                # ProductOrchestratorTests
├── Repositories/                  # ProductRepositoryTests
└── Services/                      # ImageServiceTests, ProductServiceTests
```

---

## Flujo de datos

**Frontend:** los hooks en `src/features/products/hooks.ts` alternan automáticamente entre la API real (`productsApi`) y los datos mockeados (`productMock`) según `VITE_USE_MOCK`, sin que el resto de la aplicación necesite saberlo.

**Backend:** los controllers son deliberadamente "delgados" (thin controllers) — no contienen lógica de negocio ni acceden directamente a servicios de imagen o repositorios. Cada operación de escritura que involucra producto + imagen (crear, actualizar) se delega al `ProductOrchestrator`, que envuelve ambas operaciones en una única transacción de base de datos vía `BeginTransactionAsync`.

---

## Decisiones de arquitectura

### Frontend

**Alternancia mock/API real vía variable de entorno**
Los hooks en `src/features/products/hooks.ts` deciden en un único punto si consumen `productMock` o `productsApi`, según `VITE_USE_MOCK`. Esto evita condicionales dispersos por la aplicación y permite desarrollar o demostrar el frontend sin depender de que el backend esté disponible.

**Validación de entorno al arrancar (fail-fast)**
`src/config/env.ts` valida las variables de entorno con Zod antes de que la app renderice. Si falta una variable o tiene un formato inválido, la aplicación falla de forma explícita en vez de comportarse de manera inesperada en runtime.

**Invalidación de cache de imagen tras actualizar**
Al editar un producto, `useUpdateProduct` agrega un campo `_imageVersion: Date.now()` al registro cacheado en TanStack Query. Esto evita que el navegador reutilice una imagen vieja servida bajo la misma URL cuando el usuario sube una nueva, sin necesidad de invalidar toda la cache ni forzar un refetch completo.

**React Hook Form + Zod**
Los esquemas de validación se definen una sola vez con Zod y se reutilizan tanto en el formulario (vía `zodResolver`) como en la construcción del `FormData` enviado al backend, evitando duplicar reglas de negocio en dos lugares distintos.

**Estructura por features, no por tipo de archivo**
En lugar de carpetas globales `hooks/`, `schemas/`, `forms/`, la lógica de producto vive junta en `features/products/`. Esto escala mejor a medida que crecen los dominios.

**Bootstrap 5 + SCSS en vez de una librería de componentes**
Se priorizó control granular sobre estilos y bajo tamaño de bundle sobre la velocidad de un sistema de diseño completo, dado el alcance acotado de la prueba.

### Backend

**Orchestrator para coordinar transacciones entre capas**
`ProductOrchestrator` existe específicamente para envolver en una sola transacción de base de datos las operaciones que abarcan tanto el producto (EF Core / SQL) como su imagen (filesystem). Si falla el guardado de la imagen a mitad del flujo, la transacción hace rollback y el producto tampoco queda persistido — evitando estados inconsistentes donde exista un producto en base de datos sin imagen esperada, o viceversa.

**Thin controllers**
Los controllers no acceden directamente a `IProductService` ni `IImageStorageService` para las operaciones de creación/actualización — delegan todo el flujo al Orchestrator. Esto centraliza la lógica de transaccionalidad en un solo lugar y facilita testear el flujo completo de forma aislada del framework HTTP.

**Contrato explícito para eliminación de imagen (`RemoveImage`)**
Enviar "sin imagen" en una actualización es ambiguo entre dos intenciones distintas: *conservar la imagen actual* o *eliminarla*. El DTO de actualización distingue tres estados posibles:
- `Image` presente → se guarda como nueva imagen (reemplaza la anterior).
- `Image` ausente y `RemoveImage = true` → se elimina la imagen existente explícitamente.
- `Image` ausente y `RemoveImage = false` (default) → la imagen existente se conserva sin tocar el filesystem.

Si llegan ambos (`Image` presente y `RemoveImage = true`) simultáneamente, `Image` tiene prioridad — se guarda la nueva imagen y se ignora la bandera de eliminación, evitando un estado contradictorio.

**`GetPhysicalPath` como fuente de verdad para la existencia de imagen**
En vez de guardar una columna `ImageUrl` o `HasImage` en la entidad `Product`, `ImageStorageService.GetPhysicalPath` resuelve la existencia de la imagen consultando directamente el filesystem por convención de nombre (`{productId}.{extensión}`). Esto evita que la base de datos y el disco puedan desincronizarse (ej. una URL en base de datos apuntando a un archivo que ya no existe), a costa de una consulta a disco por producto al enriquecer las respuestas del API.

**Reseed en cada arranque de la aplicación**
`SeedData.Initialize` limpia y repuebla la tabla `Products` cada vez que la API arranca. Es una decisión pragmática para tener un estado predecible en cada demo, aunque no sería apropiada tal cual para un entorno de producción (debería condicionarse a `Environment.IsDevelopment()`).

---

## Notas técnicas relevantes

**Validación duplicada, intencionalmente**
Las reglas de negocio (longitud de nombre, rango de precio, rango de edad) están definidas tanto en el frontend (Zod) como en el backend (`DataAnnotations` en los DTOs y la entidad `Product`). Esto es deliberado: la validación de frontend mejora la experiencia de usuario (feedback inmediato), pero nunca debe ser la única línea de defensa — el backend valida de forma independiente sin confiar en que el cliente ya lo hizo.

**Formato de imagen validado solo por extensión, no por contenido**
`ImageStorageService.SaveAsync` valida el formato de archivo a partir de la extensión del nombre recibido (whitelist: `.jpg`, `.jpeg`, `.png`, `.webp`), no inspeccionando los bytes reales del archivo (magic numbers). Es suficiente para el alcance de esta prueba, pero en un entorno productivo real valdría la pena agregar validación de contenido para evitar que un archivo con extensión falsificada pase la validación.