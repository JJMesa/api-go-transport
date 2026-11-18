# Informe de Auditoría Técnica — GoTransport.Api

> **Proyecto:** GoTransport.Api (ASP.NET Core Web API, .NET 6) · **Fecha:** 2026-06-22
> **Alcance:** solución completa (`GoTransport.sln`) · **Profundidad:** exhaustiva · **Modalidad:** solo lectura
> **Método:** evaluación por 14 dimensiones con verificación adversarial de cada hallazgo crítico/alto + verificación empírica ejecutando código `net6` real. Las severidades son las **corregidas tras verificación**.

> 🌐 **Convención de idioma (desde 2026-07-02):** todo texto **nuevo** que se agregue a la aplicación durante estas mejoras debe escribirse **en inglés** — mensajes de log, de error/excepción, de respuesta de la API, cadenas visibles al usuario y comentarios/XML docs nuevos. El texto en español ya existente se mantiene salvo que se esté modificando. **Excepción:** identificadores y valores de dominio atados a datos persistidos o convenciones existentes (nombres de roles como `Administrador`, el usuario de auditoría `SISTEMA`, datos semilla, columnas de BD) conservan su forma actual para no romper la consistencia de datos.

---

## 1. Resumen ejecutivo

- **Calificación global: 6.0 / 10 — Aceptable** (sólido en estructura; con carencias claras en testing, observabilidad, hardening de seguridad y mantenimiento).
- **Veredicto:** proyecto de portfolio **competente y bien organizado**: arquitectura en capas limpia con dirección de dependencias correcta, patrón Specification (Ardalis) bien usado con paginación y proyección a DTO en servidor, asincronía idiomática sin bloqueos, y un patrón Result coherente de extremo a extremo. No está listo para producción: el ~44% del peso de la evaluación (testing, seguridad, observabilidad, datos) acumula defectos objetivos. Ninguno es irreparable; la base es buena y el plan de mejora es de esfuerzo acotado.
- **Top 3 fortalezas:** (1) Separación de capas y uso idiomático de Specification + paginación/proyección server-side; (2) Asincronía correcta de punta a punta (sin `async void`, `.Result` ni `.Wait()`); (3) Consistencia de diseño extrema entre los ~12 servicios.
- **Top 3 riesgos más urgentes:**
  1. 🔴 Escritura de reservas sin autenticación — `[AllowAnonymous]` en `POST`/`PUT` de `ReservationsController`.
  2. 🟠 Login sin protección anti-fuerza-bruta — `AddDefaultIdentity` sin lockout + `CheckPasswordAsync` que no cuenta fallos.
  3. 🟠 Observabilidad nula — ni un solo `ILogger`; el middleware de errores traga las excepciones sin registrarlas.

---

## 2. Contexto del proyecto (Fase 0)

| Aspecto | Detalle |
|---|---|
| **Stack** | ASP.NET Core Web API REST, C# 10 |
| **Target Framework** | `net6.0` en los **7** proyectos — **fuera de soporte** (EOL 12-nov-2024) |
| **Tipo** | API REST headless con autenticación JWT Bearer (no usa UI) |
| **Propósito** | Gestión de empresa de transporte: rutas, puntos, horarios, vehículos, reservas, usuarios/roles |
| **Arquitectura** | En capas (N-tier con tintes de Clean): `Api → Application → Domain`, `Persistence → Application + Domain` |
| **Persistencia** | EF Core 7.0.17 sobre SQL Server + ASP.NET Identity (claves `long`); Ardalis.Specification |
| **Dependencias clave** | AutoMapper 13, FluentValidation 11, Ardalis.Specification (**7.0.0 Application / 8.0.0 Persistence** ⚠️), Swashbuckle 6.5, Asp.Versioning 6.4, JwtBearer 6.0.28 |
| **Tests** | xUnit + Moq + EFCore.InMemory (referenciado pero **no usado**); solo `CityServiceTests` y `CitiesControllerTests` |
| **Gobierno de paquetes** | Sin `Directory.Packages.props`, sin `Directory.Build.props`, sin `global.json` |
| **Tooling/CI** | Sin `.editorconfig`, sin analizadores explícitos; `.github/workflows` **existe pero vacío** |

**Ponderación:** pesos por defecto de la metodología, sin ajustes ni dimensiones N/A (las 14 aplican a una Web API).

**Verificación empírica:** se compiló y ejecutó un proyecto `net6.0` replicando la cadena de DI real para zanjar la contradicción sobre `IMemoryCache` (ver hallazgo 🟡-1). Resultado reproducible: `AddDefaultIdentity` registra `IMemoryCache`; `AddControllers` y `AddIdentityCore` **no**.

---

## 3. Calificación por dimensión

| # | Dimensión | Nota /10 | Peso | Comentario breve |
|---|-----------|:---:|:---:|------------------|
| 1 | Arquitectura y diseño | **7.0** | 18% | Capas limpias y sin ciclos; Domain atado a Identity y `Persistence→Application` |
| 2 | Calidad de código y C# | **6.5** | 14% | Muy consistente y legible; bug copy-paste en `CacheKey`, typos, C# conservador |
| 9 | Seguridad | **5.5** | 14% | JWT funciona; escritura anónima de reservas, sin lockout, sin HSTS |
| 11 | Testing | **4.5** | 12% | Buena calidad AAA pero cobertura 1/13; component tests sobre SQL real |
| 7 | Acceso a datos (EF Core) | **6.5** | 8% | Paginación/proyección server-side correctas; sin `AsNoTracking` ni UoW |
| 10 | Errores y resiliencia | **5.0** | 7% | Patrón Result coherente; excepciones sin log, ramas muertas, `!` riesgoso |
| 8 | Diseño de API | **6.3** | 6% | REST correcto y versionado; `AllowAnonymous`, sin ProblemDetails ni `Location` |
| 3 | Patrones / anti-patrones | **7.0** | 5% | Specification/Options/Builder bien; DI por reflexión frágil |
| 6 | Asincronía y concurrencia | **7.2** | 5% | Idiomática, sin bloqueos; falta `CancellationToken` en escrituras |
| 12 | Config. / observabilidad | **3.5** | 4% | Options sólido, pero **cero logging**, sin health checks ni tracing |
| 4 | Dependencias (NuGet) | **5.0** | 3% | Conjunto razonable; net6 EOL, versiones inconsistentes, sin CPM |
| 5 | Inyección de dependencias | **6.5** | 2% | Composición por capas ok; `IOptionsSnapshot` mal usado, reflexión frágil |
| 13 | Rendimiento | **6.0** | 1% | Paginación eficiente; caché sin invalidar, sin `AsNoTracking` |
| 14 | Mantenibilidad / tooling | **5.0** | 1% | README útil y XML docs; net6 EOL, sin tooling de calidad ni CI |

**Nota global ponderada: 6.0 / 10**

---

## 4. Fortalezas

- **[Arquitectura]** Dirección de dependencias correcta y sin ciclos; controladores delgados. Ev.: `GoTransport.Application.csproj:19`, `CitiesController.cs:54`.
- **[Datos/Patrones]** Specification con **paginación `Skip/Take` y proyección a DTO en SQL** (`Specification<City, CityDto>`). Ev.: `PagedCitySpecification.cs:10-52`.
- **[Asincronía]** `async/await` idiomático; **cero** `async void`/`.Result`/`.Wait()`; `CancellationToken` en lecturas.
- **[Calidad]** ~12 servicios con patrón idéntico; auditoría centralizada en `SaveChangesAsync`. Ev.: `ApplicationDbContext.cs:31-55`.
- **[Config/Patrones]** Patrón Options correcto y **secretos fuera del repo** (User Secrets). Ev.: `RegisterLayerExtensions.cs:24-25`.
- **[Testing]** Los tests existentes son de buena calidad (AAA, `[Theory]`, mocks verificados, Object Mother). Ev.: `CityServiceTests.cs`.

---

## 5. Hallazgos por severidad

### 🔴 Críticos
- **Escritura de reservas sin autenticación (Broken Access Control, OWASP A01).** `[Authorize]` a nivel de clase anulado por `[AllowAnonymous]` en `POST`/`PUT` (el `DELETE` sí exige auth). *Verificado: confirmado.* Ev.: `ReservationsController.cs:62, :71, :36`.

### 🟠 Altos
- ~~**Login sin anti-fuerza-bruta.** `AddDefaultIdentity` sin `Lockout`; `LoginAsync` usa `CheckPasswordAsync` (no cuenta fallos). Ev.: `Persistence/RegisterLayerExtensions.cs:28`, `AccountService.cs:37`.~~ ✅ *Resuelto:* lockout configurable vía `LockoutSettings` en appsettings (`MaxFailedAccessAttempts`/`LockoutTimeInMinutes`, por defecto 5 intentos / 15 min) en `AddDefaultIdentity` y `LoginAsync` migrado a `SignInManager.CheckPasswordSignInAsync(lockoutOnFailure: true)`, devolviendo `429 Too Many Requests` (`ErrorMessages.UserLockedOut`) al bloquearse la cuenta (QW-5/F-03).
- ~~**Observabilidad nula.** Cero `ILogger`/Serilog; el middleware no registra excepciones. Ev.: `ErrorHandlerMiddleware.cs:27-31`.~~ ✅ *Resuelto:* Serilog como proveedor de logging estructurado (sinks Console + File con rotación diaria y retención de 7 días, configurable vía la sección `Serilog` de `appsettings`), bootstrap logger + `Log.CloseAndFlush()` en `Program.cs`, y `UseSerilogRequestLogging` (ubicado como middleware más externo para capturar el status final). Además, health checks expuestos en `/health` con chequeo de conectividad de BD (`AddDbContextCheck<ApplicationDbContext>`) y respuesta JSON detallada (F-04).
- **Cobertura de tests testimonial** (originalmente 1/13 servicios, 1/12 controllers). 🔸 *Parcial (F-05):* añadidos tests unitarios para un subconjunto representativo — `DepartmentService` (catálogo con caché), `ReservationService` (Includes/lógica de fecha) y `AccountService` (login/lockout) — con un Object Mother por servicio; quedan servicios sin cubrir. Ev.: `DepartmentServiceTests.cs`, `ReservationServiceTests.cs`, `AccountServiceTests.cs`.
- ~~**Component tests no reproducibles** (golpean SQL Server real, token estático, datos semilla).~~ ✅ *Resuelto (F-05):* aislados con EF Core InMemory mediante `CustomWebApplicationFactory` (BD única por instancia + `ResetDatabase`/`DatabaseSeeder` determinista) y un `TestAuthHandler` que reemplaza el JWT real; eliminados `AuthTestingSettings` y las credenciales de prueba versionadas. Ya no requieren SQL Server ni User Secrets.
- **Framework `net6.0` EOL** en los 7 proyectos. Ev.: los 7 `.csproj`, línea 4.

### 🟡 Medios
- **`IMemoryCache` sin registro explícito (fragilidad).** Funciona hoy solo porque `AddDefaultIdentity` arrastra la UI Razor de Identity; migrar a `AddIdentityCore` rompería los 8 servicios cacheados. ⚠️ *Desmiente el "crítico" reportado: la caché NO está rota.* Ev.: `CacheService.cs:13`.
- **Derivación incoherente de clave JWT** (`HMACSHA512` vs `HmacSha256`): invalida tokens con secretos >128 bytes. Ev.: `ServiceExtensions.cs:35` vs `AccountService.cs:66`.
- **`roleList.First()` impide tokens multi-rol.** Ev.: `AccountService.cs:91`.
- **Sin `AsNoTracking` en lecturas** (grep = 0).
- **Sin unidad de trabajo** (un `SaveChanges` por operación). Ev.: `CityService.cs:55,72,80`.
- **`SaveChanges` síncrono sin override** (auditoría se pierde). Ev.: `ApplicationDbContext.cs:31`.
- **`ReservationService.GetAllAsync` sin `Include`** → riesgo de `NullReferenceException`. Ev.: `ReservationService.cs:40`.
- ~~**Middleware: no loguea + lee body inútil + registrado al final** del pipeline.~~ ✅ *Resuelto:* reubicado al inicio del pipeline (`Program.cs:39`), eliminada la lectura del body e inyectado `ILogger` con registro por rama (QW-3).
- ~~**Bug copy-paste en `CacheKey`** (`Manufacturers` == `IdentificationTypes`). Ev.: `CacheKey.cs:6-7`.~~ ✅ *Resuelto:* `CacheKey.Manufacturers = "manufacturersList"`, eliminada la colisión de caché entre fabricantes y tipos de identificación (QW-4).
- **Caché nunca invalidada** (`Remove` sin call sites).
- **`Persistence` depende de `Application`.** Ev.: `Persistence.csproj:17`.
- **`IServiceBase` aplicado a ~5/12 servicios.** Ev.: `ICityService.cs:7-19`.
- **Sin HSTS ni cabeceras de seguridad.**
- **Sin health checks.**
- **`IOptionsSnapshot<T>` para config estable** (debería ser `IOptions<T>`). Ev.: `AccountService.cs:26`.
- **Sin `.editorconfig`, analizadores ni CI** (`.github/workflows` vacío).
- **`Domain` depende de ASP.NET Identity.**

### 🟢 Bajos
- CORS `AllowAnyOrigin` (mitigado: API Bearer). Ev.: `ServiceExtensions.cs:172`.
- DI por reflexión (`AppDomain.GetAssemblies()` + convención `I{Nombre}`). Ev.: `RegisterLayerExtensions.cs:30-57`.
- Repositorio genérico aporta poco + `Context` público no usado. Ev.: `BaseRepository.cs:9`.
- Borrado físico pese a columnas de soft-delete.
- Credenciales admin versionadas (`Gotransport_1`). Ev.: `INSERT_SEED.sql`, `appsettings.ComponentTesting.json`.
- Claim `iat` con formato de fecha larga. Ev.: `AccountService.cs:62`.
- Typos propagados (`RuoteParameters.cs`, `ruoteId`, `IsDuplicarePoints`, "transaporte").
- `POST` sin `Location`; header `Pagination` no estándar; sin ProblemDetails.
- Carpeta `Exceptions` vacía; ramas muertas (`KeyNotFoundException`, `ValidationException`).
- C# conservador (0 `record`/primary constructors).
- XML docs no expuestos (sin `GenerateDocumentationFile`).

---

## 6. Seguimiento de correcciones (Fix Tracking)

> **Leyenda de estado:** ⬜ Pendiente · 🟦 En progreso · ✅ Hecho · 🔁 En revisión · ⏭️ Pospuesto · ❌ No se hará
> Actualiza la columna **Estado** (y **Verificado**) conforme avances. Las casillas `[ ]` se marcan `[x]` al completar.

### 6.1 Quick wins (alto impacto / bajo esfuerzo)

| ✔ | ID | Acción | Sev. | Estado | Responsable | PR / Commit | Verificado |
|:--:|:--:|--------|:--:|:--:|---|---|:--:|
| [x] | QW-1 | Quitar `[AllowAnonymous]` de `POST`/`PUT` de `ReservationsController` | 🔴 | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | QW-2 | Añadir `services.AddMemoryCache();` explícito + `ValidateOnBuild = true` | 🟡 | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | QW-3 | Inyectar `ILogger` en `ErrorHandlerMiddleware` y eliminar lectura del body | 🟠 | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | QW-4 | Corregir `CacheKey.Manufacturers = "manufacturersList"` | 🟡 | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | QW-5 | Configurar lockout de Identity + `CheckPasswordSignInAsync(lockoutOnFailure: true)` | 🟠 | ✅ Hecho | Juan Mesa | — | ✅ |

### 6.2 Plan priorizado completo

| ✔ | ID | Acción | Sev. | Dimensión | Esfuerzo | Estado | Responsable | PR / Commit | Verificado |
|:--:|:--:|--------|:--:|---|:--:|:--:|---|---|:--:|
| [x] | F-01 | Exigir auth en `POST`/`PUT` de Reservations | 🔴 | Seguridad/API | S | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | F-02 | `AddMemoryCache()` explícito + `ValidateOnBuild` | 🟡 | DI/Rendimiento | S | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | F-03 | Lockout + `CheckPasswordSignInAsync` | 🟠 | Seguridad | S | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | F-04 | Logging estructurado (`ILogger`/Serilog) + health checks | 🟠 | Observabilidad | M | ✅ Hecho | Juan Mesa | — | ✅ |
| [x] | F-05 | Aislar BD en component tests + ampliar cobertura unit | 🟠 | Testing | M | ✅ Hecho | Juan Mesa | — | ✅ |
| [ ] | F-06 | Migrar a `net8.0` (LTS) + `Directory.Build.props` + CPM | 🟠 | NuGet/Mantenib. | M | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-07 | Unificar clave JWT en una sola `SymmetricSecurityKey` | 🟡 | Seguridad | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-08 | Iterar todos los roles a claims (multi-rol) | 🟡 | Seguridad | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-09 | `AsNoTracking` en Specs de lectura + Includes en `GetAll` | 🟡 | Datos/Rend. | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-10 | Invalidar caché (`Remove`) en escrituras + subir TTL | 🟡 | Rendimiento | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-11 | Pipeline CI (`dotnet build/test`) en `.github/workflows` | 🟠 | Mantenib. | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-12 | `.editorconfig` + analizadores + `TreatWarningsAsErrors` (CI) | 🟡 | Mantenib. | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-13 | Override `SaveChanges` síncrono (o `ISaveChangesInterceptor` de auditoría) | 🟡 | Datos | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-14 | Corregir `ReservationService.GetAllAsync` (Includes / proyección) | 🟡 | Datos | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-15 | Sustituir DI por reflexión por Scrutor/registro explícito | 🟢 | Patrones/DI | M | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-16 | `Persistence`→solo `Domain`; mover constante/helper a Domain/SharedKernel | 🟡 | Arquitectura | M | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-17 | Restringir CORS a orígenes por entorno + `UseHsts` | 🟢 | Seguridad | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-18 | Unificar servicios bajo `IServiceBase` (o eliminarlo) | 🟡 | Arquitectura | M | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-19 | `IOptions<T>` en lugar de `IOptionsSnapshot<T>` para config estable | 🟡 | DI | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-20 | Renombrar typos (`Ruote`→`Route`, "transaporte") + limpiar dead code | 🟢 | Calidad | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-21 | `CancellationToken` en `Create/Update/Delete` (servicios + endpoints) | 🟡 | Asincronía | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-22 | Excepciones de dominio + `ProblemDetails`; eliminar carpeta `Exceptions` vacía | 🟢 | Errores/API | M | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-23 | Borrado lógico (`IsActive=false`) donde el negocio lo requiera | 🟢 | Datos | M | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-24 | Sacar credenciales de test del control de versiones | 🟢 | Seguridad | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-25 | `GenerateDocumentationFile` + `IncludeXmlComments` en Swagger | 🟢 | Mantenib. | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-26 | Reorganizar el repo en `src/` `tests/` `docs/` `database/` con un `CLAUDE.md` por área, glosario y `.claude/` | 🟢 | Mantenib. | M | 🟦 En progreso | Juan Mesa | — | ⬜ |
| [ ] | F-27 | Exigir auth en `GET schedules/{id}/reservations` (expone nombre y documento de pasajeros) — ver §6.4 | 🟠 | Seguridad | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-28 | `ReservationService.DeleteAsync` sin `Include(Schedule)` → `NullReferenceException` en todo borrado — ver §6.4 | 🟠 | Datos/Errores | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-29 | Contar el cupo por Trip (Schedule + fecha de viaje), no por Schedule — ver §6.4 | 🟠 | Negocio | M | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-30 | `ReservationService.CreateAsync` con `ScheduleId` inexistente → 500 en vez de 404 — ver §6.4 | 🟡 | Errores/API | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-31 | Soportar Schedules que cruzan la medianoche (`Duration` y "viaje pasado") — ver §6.4 | 🟡 | Negocio | S | ⬜ Pendiente | — | — | ⬜ |
| [ ] | F-32 | Rechazar Routes con el mismo Point de origen y destino — ver §6.4 | 🟢 | Negocio | S | ⬜ Pendiente | — | — | ⬜ |

### 6.3 Resumen de avance

| Severidad | Total | ⬜ Pendiente | 🟦 En progreso | ✅ Hecho |
|---|:--:|:--:|:--:|:--:|
| 🔴 Crítico | 1 | 0 | 0 | 1 |
| 🟠 Alto | 8 | 5 | 0 | 3 |
| 🟡 Medio | 14 | 13 | 0 | 1 |
| 🟢 Bajo | 9 | 8 | 1 | 0 |
| **Total** | **32** | **26** | **1** | **5** |
| **% Completado** | | | | **16%** |

### 6.4 Hallazgos posteriores (2026-11-17)

Detectados al construir el glosario de dominio (`GLOSSARY.md`) y cruzarlo con el código. Rutas según la estructura de F-26.

- **F-27** — `src/GoTransport.Api/Controllers/v1/ReservationsController.cs:36`: `[AllowAnonymous]` en `GET api/v1/schedules/{scheduleId}/reservations`. Devuelve `ReservationDto` con `PassengerFirstName`, `PassengerLastName` y `PassengerIdentification` de cualquier Schedule sin autenticación (datos personales; Ley 1581 de 2012).
- **F-28** — `src/GoTransport.Application/Services/ReservationService.cs:96,110`: `DeleteAsync` carga la reserva con `GetByIdAsync` (sin `Include`) y `IsReservationDatePassed` lee `reservation.Schedule.ArrivalTime`; sin lazy loading `Schedule` es `null` → 500 al borrar cualquier reserva existente.
- **F-29** — `ReservationService.cs:68` + `src/GoTransport.Application/Specifications/Reservations/ReservationSpecification.cs:28`: el conteo para `VehicleFull` filtra solo por `ScheduleId`. Como un Schedule se repite a diario, tras `Capacity` reservas acumuladas en cualquier fecha el Schedule queda lleno para siempre. El cupo debe contarse por Trip (Schedule + `ReservationDate`).
- **F-30** — `ReservationService.cs:67-70`: si el `ScheduleId` no existe, `vehicle!.Capacity` lanza `NullReferenceException` → 500; debería responder 404.
- **F-31** — `src/GoTransport.Application/Services/ScheduleService.cs:40,56` y `ReservationService.cs:110`: con salida 22:00 y llegada 05:30, `Duration = Arrival − Departure` es negativa y `ReservationDate + ArrivalTime` marca el viaje como pasado antes de la salida.
- **F-32** — `src/GoTransport.Application/Services/RouteService.cs:58,75`: solo se valida que el par origen–destino no esté duplicado; nada impide que origen y destino sean el mismo Point.

---

## 7. Recomendaciones para el proceso de desarrollo

- **Gate de CI obligatorio:** `dotnet build` + `dotnet test` + cobertura (coverlet ya está) en cada push/PR.
- **Calidad como build:** `.editorconfig` + `EnableNETAnalyzers`/`AnalysisMode=All` + `TreatWarningsAsErrors` en CI. Considerar `NetArchTest` para fijar reglas de capas.
- **Gobierno de paquetes:** `Directory.Packages.props` (CPM) — elimina la deriva Ardalis 7/8 y versiones de test divergentes.
- **Validación de DI temprana:** `ValidateOnBuild`/`ValidateScopes` + test de arranque que resuelva cada servicio (convierte fallos como el de `IMemoryCache` en errores de build deterministas).
- **Plantilla de testing:** objetivo medible (p. ej. `Services` > 70% líneas) e infraestructura aislada (Testcontainers/InMemory) como estándar.
- **Política de .NET:** no usar TFM en EOL.

---

## 8. Conclusión

GoTransport.Api demuestra un dominio sólido de los fundamentos (arquitectura por capas, patrones, asincronía, estilo consistente), por encima del promedio de un proyecto de portfolio. La distancia al estándar de producción está en las prácticas de robustez: control de acceso, observabilidad, testing reproducible, hardening de seguridad y mantenimiento del runtime. Casi todo el riesgo se concentra en correcciones de esfuerzo bajo/medio y localizadas: ejecutar los ítems F-01 a F-06 elevaría la madurez de "Aceptable" a "Sólido" sin reescrituras.

> **Nota de fiabilidad:** los dos hallazgos reportados inicialmente como 🔴 críticos fueron verificados: el de `IMemoryCache` se **desmintió ejecutando código `net6` real** (la caché funciona, aunque de forma frágil → reclasificado 🟡); el único 🔴 que se sostiene es la escritura anónima de reservas. Las severidades de este informe son las corregidas tras esa verificación.
