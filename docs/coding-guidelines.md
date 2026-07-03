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
not use scoped or transient registrations.

## 4. Configuration

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
`<userSettings>`.

## 5. Localization

Per-view XAML resource dictionaries loaded at runtime by
`CultureProvider.ChangeAppCulture()`.

- Master dictionaries: `LocalizationDictionary/en.xaml`,
  `LocalizationDictionary/ru.xaml` — merge sub-dictionaries.
- Sub-dictionaries per view: `MainMenu.en.xaml`,
  `ScratchControlView.en.xaml`, etc.
- Referenced in XAML via
  `{DynamicResource MainMenu.File.MainHeader}`.

## 6. Status messages

The project does not use a logging framework. Status and error
messages are delivered to the UI via
`IStatusBarNotificationDeliveryService`:

```csharp
mStatusBarNotification.AppLogMessage = "Error reading blokly code";
mStatusBarNotification.CompileLogMessage =
    $"{syslogMessage.TimeStamp:T} {syslogMessage.Message}";
```

The service extends `BindableBase` and fires
`PropertyChanged` — the status bar binds to it and updates
automatically.

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

## 10. Source control

### 10.1 Commit messages

Follow Conventional Commits. Recap:

```
<type>(<scope>): <short summary>

- bullet point 1
- bullet point 2
```

**Types:** `feat`, `fix`, `refactor`, `perf`, `docs`, `ci`,
`test`, `chore`.

**Scopes** — match the project area:

| Scope | Maps to |
|---|---|
| `shell` | AdamController (App.xaml.cs, MainWindow, DI root) |
| `core` | AdamController.Core (MVVM, converters, behaviors, models) |
| `services` | AdamController.Services (TCP, UDP, WebView, files) |
| `controls` | AdamController.Controls (custom controls, FlyoutManager) |
| `menu` | Modules.MenuRegion |
| `content` | Modules.ContentRegion (Scratch, ComputerVision, Settings) |
| `status` | Modules.StatusBar |
| `flyouts` | Modules.FlyoutsRegion (notifications, settings panels) |

**Summary:** imperative mood, lowercase, no trailing period.

**Body:** optional, bullets with `-`. Explain *why*, not just
*what* — the diff already shows *what*.

### 10.2 Atomic commits

- One commit = one logical change.
- The solution must compile at every commit.
- Do not mix formatting churn (renames, whitespace) with semantic
  changes.

## 11. C# language settings

- **Target framework:** `net8.0-windows7.0` for all projects.
- **Nullable reference types:** disabled in most projects.
  `AdamController.Controls` and
  `AdamController.WebApi.Client` are the exceptions (enabled).
- **`ImplicitUsings`:** disabled in most projects. Same two
  exceptions.
- **All `using` directives are explicit.** Do not rely on global
  usings.
- **Traditional namespaces** (brace-enclosed). No file-scoped
  namespaces.
- **`var`:** use when the type is obvious from the right-hand
  side. Prefer explicit types for readability-critical code.
