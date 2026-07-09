# Coding Guidelines

This document captures the conventions established in the
AdamStudio codebase. It is descriptive, not prescriptive — when in
doubt, match the existing code. New conventions are introduced only
when they reduce ambiguity or prevent recurring bugs.

## 1. Naming conventions

### 1.1 Namespaces

- Match the folder path: file at
  `Services/TcpClientDependency/TcpClientService.cs` declares
  `namespace AdamController.Services.TcpClientDependency`.
- One namespace per folder; do not split a folder across multiple
  namespaces.
- All namespaces use traditional brace-enclosed syntax. No
  file-scoped namespaces.
- Root namespace for the solution is `AdamController`.

### 1.2 Identifiers

- **Types, public members, methods, properties:** `PascalCase`.
  - `ContentRegionModule`, `OnInitialized`,
  `IsCheckedScratchMenuItem`.
- **Private fields:** `m` prefix + `PascalCase`.
  - `mRegionManager`, `mCommunicationProviderService`,
  `mIsWarningStackOverflowAlreadyShow`.
- **Private static fields:** `s` prefix + `PascalCase`.
  - `sInstance`, `sDefaultCulture`.
- **Constants:** `c` prefix + `PascalCase`.
  - `cFilter`, `cStartMessage`, `cBufferSize`.
- **Locals and parameters:** `camelCase`.
  - `regionName`, `receivedString`, `isNewValue`.
- **Event names:** `Raise` prefix + description + `Event` suffix.
  - `RaiseTcpClientConnectedEvent`,
  `RaiseWebViewMessageReceivedEvent`.
- **Delegate command properties:** description + `DelegateCommand`
  suffix.
  - `ShowRegionCommand`, `SwitchToVideoDelegateCommand`,
  `RunPythonCodeDelegateCommand`.

### 1.3 Files

- One public type per file. The file name matches the type name.
- Small related types (DTOs, records) may share a file if they form a
  cohesive group (e.g. `PairingDtos.cs` contains `PairRequestDto` +
  `PairResponseDto`).
- View–ViewModel pairs live in sibling folders:
  `Views/ScratchControlView.xaml` ↔
  `ViewModels/ScratchControlViewModel.cs`.
- Service interfaces live in `Services/Interfaces/`,
  implementations in `Services/<Area>Dependency/`.

## 2. MVVM & Prism

### 2.1 Base classes

All ViewModels inherit from one of the base classes in
`AdamController.Core/Mvvm/`:

| Base class | Extends | Used by |
|---|---|---|
| `ViewModelBase` | `BindableBase, IDestructible` | Standalone VMs |
| `RegionViewModelBase` | `ViewModelBase, INavigationAware, IConfirmNavigationRequest` | Region VMs that navigate |
| `DialogViewModelBase` | `BindableBase, IDialogAware` | Prism dialog VMs (currently unused) |
| `FlyoutBase` | `BindableBase, IFlyout` | Flyout panel VMs (in `AdamController.Controls`) |

### 2.2 View–ViewModel wiring

All views use Prism's convention-based wiring:

```xml
<UserControl ...
    prism:ViewModelLocator.AutoWireViewModel="True">
```

Prism resolves `ScratchControlView` → `ScratchControlViewModel`
by naming convention. Never set `DataContext` manually.

### 2.3 Properties

Use `SetProperty` from `BindableBase`:

```csharp
private bool isCheckedScratchMenuItem;
public bool IsCheckedScratchMenuItem
{
    get => isCheckedScratchMenuItem;
    set => SetProperty(ref isCheckedScratchMenuItem, value);
}
```

Side effects in the setter are acceptable when they raise
`CanExecuteChanged` on dependent commands:

```csharp
private bool isTcpClientConnected;
public bool IsTcpClientConnected
{
    get => isTcpClientConnected;
    set
    {
        bool isNewValue = SetProperty(ref isTcpClientConnected, value);
        if (isNewValue)
            RaiseDelegateCommandsCanExecuteChanged();
    }
}
```

### 2.4 Commands

All commands use `DelegateCommand` or `DelegateCommand<T>` from
Prism. Declare as public properties, instantiate in the constructor:

```csharp
public DelegateCommand<string> ShowRegionCommand { get; }
public DelegateCommand SwitchToVideoDelegateCommand { get; }

public ContentRegionViewModel(...)
{
    ShowRegionCommand = new DelegateCommand<string>(ShowRegion);
    SwitchToVideoDelegateCommand = new DelegateCommand(
        SwitchToVideo, SwitchToVideoCanExecute);
}
```

`CanExecute` is a separate method. Call
`RaiseCanExecuteChanged()` explicitly when the guard condition
changes.

### 2.5 Regions

Four Prism regions are defined in `MainWindow.xaml`:

| Region | Host element | Defined in |
|---|---|---|
| `RegionNames.ContentRegion` | `ContentControl` (main Grid) | MainWindow |
| `RegionNames.MenuRegion` | `ContentControl` (LeftWindowCommands) | MainWindow |
| `RegionNames.FlyoutsRegion` | `FlyoutContainer` (Flyouts) | MainWindow |
| `RegionNames.StatusBarRegion` | `ContentControl` (Grid bottom) | MainWindow |

Sub-regions use `SubRegionNames`:
`SubRegionNames.InsideContentRegion` inside `ContentRegionView`.

Region names are referenced via
`{x:Static core:RegionNames.MenuRegion}` in XAML and
`RegionNames.ContentRegion` in code.

### 2.6 Flyouts

The app uses a custom `FlyoutManager` instead of Prism dialogs.
Flyouts are registered and opened by name:

```csharp
// Registration (in module or App):
mFlyoutManager.RegisterFlyoutWithDefaultRegion<PortSettingsView>(
    FlyoutNames.FlyoutPortSettings);

// Opening:
mFlyoutManager.OpenFlyout(FlyoutNames.FlyoutPortSettings);
```

Flyout ViewModels extend `FlyoutBase` (from
`AdamController.Controls`). Flyout names are defined in
`FlyoutNames` (constants class).

### 2.7 Modules

Each region module implements `IModule` with two methods:

```csharp
public class ContentRegionModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ContentRegionView>();
        containerRegistry.RegisterForNavigation<ScratchControlView>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        mRegionManager.RequestNavigate(
            RegionNames.ContentRegion, nameof(ContentRegionView));
    }
}
```

Modules are registered in `App.ConfigureModuleCatalog`.

## 3. Dependency Injection

### 3.1 Container

Prism.DryIoc. The container is configured in `App.xaml.cs` — all
registrations happen in `RegisterTypes(IContainerRegistry)`.

### 3.2 Registration style

All registrations are **inline** in `App.RegisterTypes`. No
extension methods on `IContainerRegistry` are used. Services use
`RegisterSingleton` or factory delegates:

```csharp
containerRegistry.RegisterSingleton<IFlyoutManager, FlyoutManager>();
containerRegistry.RegisterSingleton<IWebViewProvider, WebViewProvider>();

containerRegistry.RegisterSingleton<IAvalonEditService>(
    containerRegistry => new AvalonEditService(...));
```

### 3.3 Lifetimes

All services are registered as **singletons**. The project does
not use scoped or transient registrations. Stateless services,
thread-safe services, and long-lived hosts (TCP/UDP/WebSocket
clients, `FlyoutManager`, `WebViewProvider`) are all singletons.

### 3.4 Options

Configuration is bound to record or class types where practical.
Services receive configuration via constructor injection — never
read `App.config` or `Properties.Settings` directly outside the
composition root (`App.xaml.cs`).

The legacy settings flow (`Properties.Settings.Default`) is being
migrated to `IOptions<T>` as part of Tier 4. Until then, both
patterns coexist: new services use constructor-injected options,
legacy services may read `Settings.Default` directly.

### 3.5 Assembly boundaries and visibility

The solution is a monorepo: `AdamStudio.Core` is shared by the
shell and all modules. We do not split Core into separate contract
libraries — the cost of duplicate contracts and extra projects
outweighs the benefit of compile-time isolation.

Consequences:

- **`public` vs `internal`** controls visibility across assembly
  boundaries. A `public` class in Core is reachable from any project
  that references Core.
- **`internal` + an extension method** (e.g. a hypothetical
  `AddTcpClient()`) is used to discourage direct instantiation of
  implementation classes from unrelated modules. This is a
  convention, not a hard guarantee — a determined caller can still
  invoke the public extension.
- **DI-activated constructors MUST be public.**
  `Microsoft.Extensions.DependencyInjection` and Prism's
  `IContainerProvider` resolve constructors via reflection with
  public binding flags. A class marked `internal` can still have a
  `public` constructor — the type stays invisible outside its
  assembly, but the container can instantiate it. Marking the
  constructor itself `internal` (or any non-public modifier) makes
  the service unresolvable and crashes the host at startup with
  `"A suitable constructor could not be located"`. This is the rule,
  not a recommendation: if a class is registered in DI through
  `RegisterSingleton`, `RegisterScoped`, `RegisterTransient`, or any
  factory delegate, its constructor MUST be public.

## 4. Async and cancellation

- All async methods return `Task` or `Task<T>`. Avoid `async void`
  except for event handlers.
- Public async methods accept a `CancellationToken` parameter named
  `ct` (default value `default`) when they are RPC-style, or
  `stoppingToken` when they are background-service-style.
- Pass the token to every `await` that accepts one.
- For linked cancellation (e.g. apply a timeout on top of a caller's
  token), use `CancellationTokenSource.CreateLinkedTokenSource` and
  dispose it via `using`.

```csharp
public async Task SendAsync(byte[] payload, CancellationToken ct = default)
{
    await mNetworkStream.WriteAsync(payload, ct).ConfigureAwait(false);
}

public async Task<T> RequestWithTimeoutAsync<T>(
    T request, TimeSpan timeout, CancellationToken ct)
{
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    linkedCts.CancelAfter(timeout);
    return await mClient.RequestAsync(request, linkedCts.Token);
}
```

## 5. JSON serialization

- DTOs that travel over WebSocket or are persisted to disk SHOULD be
  `record` types when possible; use `record` for immutable payloads,
  `class` only when inheritance or mutability is required.
- All DTOs that cross a process boundary (WebSocket to Adam-Servers,
  WebView2 to Blockly) MUST be registered in a
  `JsonSerializerContext` so source generation is used instead of
  reflection. This is a Tier 4 migration target; until then, the
  existing `JsonSerializer` calls remain.
- Property names use `JsonPropertyName` with camelCase for wire
  compatibility with the existing Blockly and Adam-Servers contracts:

```csharp
public record WebMessageJsonReceived
{
    [JsonPropertyName("action")]
    public string Action { get; init; }

    [JsonPropertyName("data")]
    public string Data { get; init; }
}
```

## 6. Exceptions

- Throw domain exceptions for expected business failures. Catch them
  at the boundary (ViewModel, service) and convert to user-facing
  status messages via `IStatusBarNotificationDeliveryService`.
- Do not catch `Exception` to swallow it silently. Log via the
  structured logger (once migrated) or surface via
  `IStatusBarNotificationDeliveryService`, then rethrow or convert
  to a domain exception with the original as `InnerException`.
- Never leak stack traces to the UI or to Adam-Servers. Convert to
  a `MessageDto`-style payload with only the error message and a
  status code.

## 7. Events

Custom delegates are used instead of `EventHandler<T>`:

```csharp
// Delegate definition
public delegate void TcpClientConnectedEventHandler(object sender);
public delegate void TcpClientReceivedEventHandler(
    object sender, byte[] buffer, long offset, long size);

// Event declaration
public event TcpClientConnectedEventHandler RaiseTcpClientConnectedEvent;

// Raise method
protected virtual void OnRaiseTcpClientConnectedEvent()
{
    TcpClientConnectedEventHandler raiseEvent = RaiseTcpClientConnectedEvent;
    raiseEvent?.Invoke(this);
}
```

Naming: `Raise<Description>Event` for both the event field and the
`OnRaise` method. Copy the delegate to a local variable before
invoking to avoid race conditions.

## 8. WebView2

WebView2 hosts the Google Blockly visual editor as an embedded
browser. Communication between WPF and JavaScript uses a provider
pattern.

### 8.1 Provider

`IWebViewProvider` (in Services) abstracts the WebView2 control
away from the ViewModel:

- VM calls `ExecuteJavaScript(script)` → raises event → View
  executes `WebView.ExecuteScriptAsync(script)`.
- View receives `CoreWebView2.WebMessageReceived` → deserializes
  JSON → calls `IWebViewProvider.WebViewMessageReceived(data)`.

### 8.2 Message protocol

JSON with `action` and `data` fields:

```csharp
public class WebMessageJsonReceived : EventArgs
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; }
}
```

### 8.3 Local file serving

Blockly files are served via virtual host mapping:

```csharp
WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "localhost", mPathToSource,
    CoreWebView2HostResourceAccessKind.Allow);
WebView.CoreWebView2.Navigate("https://localhost/index.html");
```

## 9. #region organization

`#region` blocks are used consistently to organize class members.
Standard region names (in order of appearance):

```
#region DelegateCommands
#region Services
#region ~                    // constructor
#region Public fields        // properties
#region Private methods
#region Subscriptions / Subscribes
#region Event methods
#region OnRaise events
#region Navigation
#region Const
#region Var                  // private fields
```

Even `using` directives in `App.xaml.cs` are grouped in `#region`
blocks by category (`system`, `prism`, `innerhit`).

The one exception is the **structured logging region** (Tier 4
target, see section 11): classes that adopt `LoggerMessage.Define`
MUST use `#region Structured logging definitions (allocation-free)`
at the end of the class.

## 10. Source control

### 10.1 Commit messages

Follow Conventional Commits as described in
[CONTRIBUTING.md](../CONTRIBUTING.md). Recap:

```
<type>(<scope>): <short summary>

- bullet point 1
- bullet point 2
```

- Types: `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`,
  `ci`, `chore`, `revert`.
- Scopes: `shell`, `core`, `services`, `controls`, `menu`, `content`,
  `status`, `flyouts`, `settings`, `toolbar`, `legacy` — see
  [CONTRIBUTING.md](../CONTRIBUTING.md#scopes) for the full list and
  which project each scope maps to.
- Summary in imperative mood, lowercase, no trailing period.
- Body: optional, bullets with `-`. Explain *why*, not just *what* —
  the diff already shows *what*.

### 10.2 Atomic commits

- One commit = one logical change. Moving a type from Services to
  Core and changing its event-naming style are two commits.
- The solution must compile and all tests must pass at every commit.
- Avoid mixing formatting churn (renames, whitespace) with semantic
  changes.

## 11. Structured logging (Tier 4 migration target)

The project currently has no logging framework; status and error
messages are delivered to the UI via
`IStatusBarNotificationDeliveryService` (see section 12). Serilog is
already referenced (`Serilog`, `Serilog.Extensions.Logging`,
`Serilog.Sinks.File`) and wired into the DI container, but
production code paths do not yet use structured logging.

Tier 4 migrates logging to `LoggerMessage.Define` (allocation-free,
source-generated structured logging). When that migration lands, the
following rules apply. Until then, this section is a forward-looking
specification, not an enforced rule.

### 11.1 Pattern

Each class that logs declares:

1. A `private static readonly Action<ILogger, ...>` field per log
   event.
2. A `private void LogXxx(...)` instance method that invokes the
   delegate.
3. All declarations grouped inside
   `#region Structured logging definitions (allocation-free)` at the
   end of the class.

```csharp
public class TcpClientService : ITcpClientService
{
    // ... fields, ctor, business methods ...

    #region Structured logging definitions (allocation-free)

    private static readonly Action<ILogger, int, Exception?> LogConnectedAction =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(40, "Connected"),
            "TCP client connected to port {Port}");

    private static readonly Action<ILogger, Exception?> LogShuttingDownAction =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(41, "ShuttingDown"),
            "TCP client is shutting down...");

    private void LogConnected(int port) =>
        LogConnectedAction(mLogger, port, null);

    private void LogShuttingDown() =>
        LogShuttingDownAction(mLogger, null);

    #endregion
}
```

### 11.2 EventId allocation

Each class owns a contiguous EventId range. Before adding a new
event, check existing ranges and pick the next free one. The
registry is maintained in `docs/event-id-registry.md` (to be created
in Tier 4).

When adding a new class, claim the next free decade (50, 60, 70,
...) and register it in the table.

### 11.3 Message templates

- Use **named placeholders** in curly braces: `{Port}`, `{EndPoint}`,
  `{Remaining}`. Never use string interpolation in the template — the
  structured logger will not be able to capture the values.
- End informational messages with no punctuation; end error messages
  with a period. Match the style of the nearest existing log message.
- The last parameter of `LoggerMessage.Define` is always `Exception?`.
  Pass `null` for non-error events, pass the caught exception for
  error events.

## 12. Status messages

The project uses `IStatusBarNotificationDeliveryService` for
user-facing status and error messages. This is the primary feedback
channel to the user (alongside flyouts and dialogs).

```csharp
mStatusBarNotification.AppLogMessage = "Error reading blokly code";
mStatusBarNotification.CompileLogMessage =
    $"{syslogMessage.TimeStamp:T} {syslogMessage.Message}";
```

The service extends `BindableBase` and fires
`PropertyChanged` — the status bar binds to it and updates
automatically.

Status messages are for the user. Internal diagnostics that the user
should not see go through structured logging (section 11) once
migrated.

## 13. Localization

Per-view XAML resource dictionaries loaded at runtime by
`CultureProvider.ChangeAppCulture()`.

- Master dictionaries: `LocalizationDictionary/en.xaml`,
  `LocalizationDictionary/ru.xaml` — merge sub-dictionaries.
- Sub-dictionaries per view: `MainMenu.en.xaml`,
  `ScratchControlView.en.xaml`, etc.
- Referenced in XAML via
  `{DynamicResource MainMenu.File.MainHeader}`.

## 14. Configuration

Configuration uses WinForms `ApplicationSettingsBase` via the
auto-generated `Properties.Settings` class:

```csharp
string ip = Settings.Default.ServerIP;
int port = Settings.Default.TcpConnectStatePort;
Settings.Default.AppThemeName = "Dark";
```

Settings are auto-saved on every property change (wired in the
`App` constructor via `Settings.Default.PropertyChanged`).

The settings file is `AdamController.Core/App.config` under
`<userSettings>`. This is being migrated to `IOptions<T>` in Tier 4;
new configuration should prefer record-based options injected via
DI.

## 15. C# language features

- **Target framework:** `net10.0-windows7.0` for all projects
  (centralized in `Directory.Build.props`).
- **Nullable reference types:** disabled by default (centralized in
  `Directory.Build.props`). `AdamStudio.Controls` and the Legacy
  `AdamController.WebApi.Client` override to enable. Tier 4 rolls
  out nullable across all projects.
- **`ImplicitUsings`:** disabled. Add `using` directives explicitly.
  Same two projects override to enable.
- **All `using` directives are explicit.** Do not rely on global
  usings.
- **Traditional namespaces** (brace-enclosed). No file-scoped
  namespaces.
- **`var`:** use when the type is obvious from the right-hand side.
  Prefer explicit types for readability-critical code
  (`TcpClientService client = ...` not `var client = ...`).
- **Collection expressions:** use `[...]` for newly-allocated
  collections (`byte[] response = [pong, portBytes[0], portBytes[1]];`).
- **Primary constructors:** preferred for classes with simple DI
  needs. Use the `m` prefix when promoting a primary-constructor
  parameter to a field by capturing it in a method.

## 16. Central Package Management

Package versions are centralized in `Directory.Packages.props`. Rules:

- All `<PackageReference>` entries in `.csproj` files MUST omit the
  `Version` attribute; the version is resolved from the
  `<PackageVersion>` items in `Directory.Packages.props`.
- To bump a package, change its `Version` in
  `Directory.Packages.props` only — never edit `Version` in a
  `.csproj`.
- Legacy projects (`Legacy/`) opt out via
  `<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>`
  and declare `Version` explicitly. This is temporary — Legacy is
  deleted in Tier 4.
- Before bumping a package, check transitive dependencies: if
  package A depends on package B >= X.Y, bumping A may require
  bumping B in the same commit. CPM blocks downgrade and produces
  NU1109 otherwise.
