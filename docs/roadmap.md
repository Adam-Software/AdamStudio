# Roadmap — AdamStudio

> Single source of truth for "what's done" and "what's next".
> If it is not in the roadmap, it does not exist.
> See `docs/coding-guidelines.md` for code conventions.

---

## Tier 1 — .NET 10 & Foundation

- [ ] **Add `.editorconfig` to root.**
- [ ] **Target `net10.0` in all `.csproj` files** (10 projects).
  - Verify all NuGet packages support .NET 10.
  - Fix breaking API changes if any.

---

## Tier 2 — Testing Assessment

Before changing behaviour, understand what to protect.

- [ ] **Audit existing tests** — count, coverage, what they test.
- [ ] **Identify critical paths that need tests first:**
  - TCP/UDP communication with robot
  - Blockly code generation → server execution pipeline
  - Settings persistence
  - WebView ↔ C# message passing
- [ ] **Choose test framework** (xUnit recommended) and add test
  projects to solution.
- [ ] **Write integration test for server communication** — the
  highest-risk area.

---

## Tier 3 — Package Updates

One package per patch. Build and verify after each.

- [ ] **Create `Directory.Build.props`** to centralize package versions
  across 10 projects.
- [ ] **Update `Prism.Wpf` 8.1.97 → 9.x** (5 .csproj, ~50 usages).
  - `IModule`, `IContainerRegistry`, `IContainerProvider` API stable.
  - Custom `SystemDialogService` (Win32) — not affected.
- [ ] **Update `Prism.DryIoc` 8.1.97 → 9.x.**
- [ ] **Update `MahApps.Metro` 2.4.10 → latest.**
- [ ] **Update `MahApps.Metro.IconPacks` 4.11.0 → latest.**
- [ ] **Update all remaining NuGet packages** to latest stable.

---

## Tier 4 — Code Standards

Bring code in line with `docs/coding-guidelines.md`.

- [ ] **Add `<Nullable>enable</Nullable>` to all `.csproj` files** and fix
  resulting warnings. ImplicitUsings stays **disabled**.
- [ ] **Private fields: rename to `mPascalCase`** across all `.cs` files
  (currently mix of `_camelCase`, `m_CamelCase`, `mCamelCase`).
- [ ] **Remove `#region` / `#endregion`** (except structured logging
  definitions).
- [ ] **Introduce `LoggerMessage.Define`** in classes that use `ILogger`.
- [ ] **Create source-generated `JsonSerializerContext`** for all wire
  types. DTOs as `record` types.
- [ ] **Replace `Settings.Default` with `IOptions<AppSettings>`** via DI.
- [ ] **Refactor DI registration** into extension methods
  (`AddXxxService()`).
- [ ] **Replace custom delegate patterns with `EventHandler<T>`.**
- [ ] **Rename `FlayoutsRegion` → `FlyoutsRegion`** (19 files).
- [ ] **Remove `Legacy/` folder.** Verify no active references.
- [ ] **Fix `FFmpegDirectory = BaseDirectory`** — use safe subdirectory.
- [ ] **Fix HResult magic number exception filter** — use typed catch.

---

## Tier 5 — WebView2

WebView2 is used to render Blockly. Current problem: 180 MB runtime
bundled in installer. The `IWebViewProvider` abstraction already exists.

- [ ] **Switch to Evergreen WebView2 mode.** Most Win10/11 have Edge
  pre-installed — no need to bundle the runtime. Add a startup check
  and prompt to install if missing (~1.5 MB bootstrapper vs 180 MB).
  This alone removes the largest component from `AdamInstallBundle`.
- [ ] **Investigate alternatives** (document findings, decide):
  - Keep WebView2 Evergreen (lowest effort, removes 180 MB)
  - CefSharp (more control, still bundles Chromium)
  - Move Blockly to browser tab, communicate via WebSocket
  - Avalonia with built-in web view (full UI migration)
- [ ] **Extract `IWebViewProvider` into a proper service** with DI,
  clean event model (replace custom delegates with `EventHandler<T>`).

---

## Tier 6 — Build & CI/CD

Replace manual VS build + Inno Setup. Runtime components (~275 MB)
must NOT be stored in git.

- [ ] **Create `GitHub Actions` workflow** `.github/workflows/build.yml`.
  Trigger on `main` / `devel`, PRs to `main`.
  Steps: restore → build (x64/Release) → upload artifact.
- [ ] **Download runtime components during CI** from official URLs:
  - `windowsdesktop-runtime-10.x-win-x64.exe`
  - `aspnetcore-runtime-10.x-win-x64.exe`
  - `VC_redist.x64.exe`
  - (WebView2 — only if not using Evergreen)
  Cache between runs.
- [ ] **Merge `AdamSetup.devel.iss` + `AdamSetup.release.iss`** into one
  `AdamSetup.iss` with `#define` parameters.
- [ ] **Parameterize runtime versions** in `.iss` via CI variables.
- [ ] **Remove `7za.exe`** — use Inno Setup built-in extraction.
- [ ] **Fix uninstaller** — stop deleting user data.
- [ ] **Version bump automation** — `<FileVersion>` → Inno Setup → tag.
- [ ] **Code signing** (when certificate is available).

---

## Tier 7 — Cross-Repo

- [ ] **Adam-Servers: GitHub Actions** (replace Azure Pipelines stubs).
- [ ] **Adam-Blockly: GitHub Actions** (replace Azure Pipelines).
- [ ] **Chain CI/CD:** Blockly release → Studio installer artifact.
- [ ] **Add `.editorconfig` to Adam-Servers and Adam-Blockly.**
- [ ] **Rename `Comunication` → `Communication` in Adam-Servers.**
- [ ] **Synchronize API versioning** (independent tags:
  `v.X.Y.Z`).

---

## Carry-over (deferred)

- [ ] **Prism → CommunityToolkit.Mvvm migration** for ViewModels.
- [ ] **Blockly: add build system** (`package.json`, npm).
- [ ] **Blockly: extract custom blocks** into separate npm package.
- [ ] **Blockly: add i18n support** (currently Russian only).
- [ ] **Server: migrate `Startup.cs` → minimal hosting.**
- [ ] **Server: fix DI violations** (controllers use `new`).
- [ ] **Server: replace static `ComunicationHelper` with DI services.**
- [ ] **Server: add Health Checks, structured logging.**
- [ ] **Server: replace UDP result delivery** with TCP + ACK.
