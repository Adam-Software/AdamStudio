# Coding Guidelines — AdamStudio

> C# 13 · .NET 10 · WPF · Prism.DryIoc · MahApps.Metro

---

## Language & Framework

- **C# 13**, target `net10.0` in all projects.
- **Nullable:** enabled (`<Nullable>enable</Nullable>`). Honor nullability
  in all signatures.
- **ImplicitUsings:** **disabled.** All `using` directives are explicit.
- **File-scoped namespaces:** preferred.

## Naming

| Target | Style | Example |
|--------|-------|---------|
| Types, public members | `PascalCase` | `TcpClientService`, `Connect()` |
| Private fields | `m` prefix + `PascalCase` | `mLogger`, `mDeviceStore` |
| Local variables, parameters | `camelCase` | `ipAddress`, `maxRetries` |
| Constants | `PascalCase` | `MaxRetryCount` |
| Interfaces | `I` prefix + `PascalCase` | `ITcpClientService` |
| Async methods | `Async` suffix | `ConnectAsync()` |

## Code Style

- **Primary constructors:** preferred for simple DI-injected services.
- **Collection expressions:** use `[...]` instead of `new List<T>()` /
  `new T[]`.
- **Expression-bodied members:** use for single-expression methods and
  properties.
- **No `#region`** except:
  - `#region Structured logging definitions (allocation-free)` — at the
    end of the class, contiguous `EventId` range.
- **No `var`** when the type is not obvious from the right-hand side.
  Exception: `var` is OK for `new` expressions and LINQ.

## DI & Services

- Services registered via **extension methods** on
  `IContainerRegistry`:
  ```csharp
  public static class TcpClientServiceExtensions
  {
      public static void AddTcpClientService(
          this IContainerRegistry registry)
      {
          registry.RegisterSingleton<ITcpClientService, TcpClientService>();
      }
  }
  ```
- `App.xaml.cs` → `RegisterTypes` calls only extension methods.
- Server-only types (internal to a project) use `internal` extension
  methods in the same project.
- DI-activated constructors **must be `public`.** `internal` constructors
  crash the container at startup.
- One logical service registration **per extension method class / file.**

## Async

- All async methods return `Task` / `Task<T>`. No `async void` except
  event handlers.
- Pass `CancellationToken` where applicable.
- Use `await` in a single code path. Avoid `async void` lambdas.

## Logging

- **`LoggerMessage.Define` only.** No string interpolation in log calls.
- Each class that logs owns a contiguous `EventId` range.
- Wrap all definitions in:
  ```csharp
  #region Structured logging definitions (allocation-free)
  private static readonly Action<ILogger, string, Exception?> LogConnected =
      LoggerMessage.Define<string>(LogLevel.Information,
          new EventId(1001, nameof(TcpClientService)),
          "Connected to {Endpoint}");
  #endregion
  ```
- Call: `LogConnected(mLogger, endpoint, null);`

## JSON

- All wire types registered in a **source-generated**
  `JsonSerializerContext`:
  ```csharp
  [JsonSerializable(typeof(DeviceStateDto))]
  [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
  public partial class AdamJsonContext : JsonSerializerContext { }
  ```
- DTOs are `record` types when possible.
- No `System.Text.Json` with `JsonSerializer.Serialize<T>` — use
  `AdamJsonContext.Default.DeviceStateDto.Serialize(dto)`.

## Commands (WPF)

- Use `CanExecute` only for **ViewModel-level properties** (e.g. `IsBusy`),
  paired with `[NotifyCanExecuteChangedFor]`.
- Do **not** use `CanExecute` for per-item state passed as
  `CommandParameter` — it does not re-evaluate on nested property changes.
- Use `IsEnabled="{Binding Property}"` in XAML instead, paired with
  `VisualStateManager` for clear disabled visuals.

## XAML

- Dark theme by default. Background `#121212`, cards `#212121` / `#322F35`,
  text white / `#BDBDBD`, accent `#6750A4` / `#D0BCFF`.
- Buttons: rounded corners (`CornerRadius="8"` or `"12"`),
  `HeightRequest="44"` (MAUI) / min height `40px` (WPF).
- Icon buttons use emoji as `Text` (🔊 🖥️ 🗑️ 🔍).

## Commits

- Format: `<type>(<scope>): <short summary>` + optional body.
- Types: `feat`, `fix`, `docs`, `refactor`, `ci`, `test`.
- Summary: imperative mood, **lowercase**, no trailing period.
- Examples:
  ```
  feat(tcp): add reconnection with exponential backoff
  fix(webview): handle navigation failure gracefully
  refactor(di): extract service registration into extension methods
  ```
- Versioning: Semantic Versioning. Independent tags:
  `client.v.X.Y.Z`, `server.v.X.Y.Z`.

## What NOT to Do

- No `internal` constructors on DI-registered classes.
- No `string interpolation` in log calls.
- No `async void` (except event handlers).
- No `new T()` for services — use DI.
- No `#region` blocks (except structured logging definitions).
- No mixing unrelated changes in one commit.