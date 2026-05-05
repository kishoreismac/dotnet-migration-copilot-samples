# Migration Plan: File-Based Logging → OpenTelemetry

## Overview
Migrate ContosoUniversity (ASP.NET MVC 5, .NET Framework 4.8) from ad-hoc diagnostic logging
(`System.Diagnostics.Trace` / `System.Diagnostics.Debug.WriteLine`) to a structured
OpenTelemetry-based logging pipeline built on `Microsoft.Extensions.Logging.ILogger<T>`.

## Current State
| Pattern | Files | Count |
|---|---|---|
| `Trace.TraceError()` | StudentsController.cs | 3 |
| `Debug.WriteLine()` | NotificationService.cs | 2 |
| `Debug.WriteLine()` | CoursesController.cs | 1 |
| `Debug.WriteLine()` | BaseController.cs | 1 |
| `Debug.WriteLine()` | NotificationsController.cs | 2 |
| Empty placeholder | Services/LoggingService.cs | 1 file |

## Target State
- `ILogger<T>` injection/factory pattern throughout all affected classes
- `LoggingService` implemented as a singleton factory wrapper using OpenTelemetry SDK
- OpenTelemetry SDK initialised in `Global.asax.cs`
- Console exporter enabled (easily swapped for OTLP/Azure Monitor in production)
- OpenTelemetry config key stored in `Web.config` `<appSettings>`

## Phases

### Phase 1 – Analysis ✅
- Identify all logging call-sites across the codebase

### Phase 2 – Dependencies
- Add OpenTelemetry NuGet packages compatible with .NET Framework 4.6.2+:
  - `OpenTelemetry` 1.9.0
  - `OpenTelemetry.Exporter.Console` 1.9.0
  - `OpenTelemetry.Extensions.Logging` 1.9.0
- Update `packages.config` and `ContosoUniversity.csproj`

### Phase 3 – Configuration
- Add `Logging:ServiceName` and `Logging:MinimumLevel` keys to `Web.config`
- Optionally document OTLP endpoint configuration

### Phase 4 – Code
1. Implement `Services/LoggingService.cs` – singleton OpenTelemetry `ILoggerFactory`
2. Update `Global.asax.cs` – initialise OpenTelemetry pipeline on app start/stop
3. Update `Controllers/BaseController.cs` – inject `ILogger`, propagate to derived controllers
4. Update `Controllers/StudentsController.cs` – replace `Trace.TraceError` with `_logger.LogError`
5. Update `Controllers/CoursesController.cs` – replace `Debug.WriteLine` with `_logger.LogError`
6. Update `Controllers/NotificationsController.cs` – replace `Debug.WriteLine` with `_logger.LogError`/`LogWarning`
7. Update `Services/NotificationService.cs` – replace `Debug.WriteLine` with `ILogger`

### Phase 5 – Verification
- [ ] `dotnet msbuild` build check
- [ ] CVE vulnerability check
- [ ] Consistency check
- [ ] Completeness check
- [ ] `dotnet test` unit tests
