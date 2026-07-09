# Contributing Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/)
specification. This helps us generate automated changelogs (via
[git-cliff](https://git-cliff.org), config in `cliff.toml`) and maintain a
clean project history.

For coding conventions (naming, logging, DI, async, JSON, XAML), see the
**[Coding Guidelines](docs/coding-guidelines.md)**.

## Commit Message Format

Each commit message must follow this structure:

`<type>(<scope>): <short summary>`

Rules for the summary:

- lowercase, imperative mood ("add", not "Added" or "Adds")
- no trailing period
- max 72 characters
- use the body for details, wrapped at 72 characters

### Types

| Type       | When to use                                                      |
|------------|------------------------------------------------------------------|
| `feat`     | A new user-visible feature                                       |
| `fix`      | A bug fix                                                        |
| `docs`     | Documentation only changes (`CONTRIBUTING.md`, `docs/`, READMEs)|
| `refactor` | A code change that neither fixes a bug nor adds a feature        |
| `perf`     | A code change that improves performance                          |
| `test`     | Adding or correcting tests                                       |
| `build`    | Build system, `.csproj`, `.iss`, `Directory.Build.props`, deps  |
| `ci`       | CI configuration files and scripts (`.github/workflows/`)       |
| `chore`    | Routine tooling: `.editorconfig`, `.gitignore`, root config     |
| `revert`   | Reverting a previous commit                                      |

### Scopes

The solution is a modular WPF application. Each scope maps to a project
or module:

| Scope      | Project / Directory                                           |
|------------|---------------------------------------------------------------|
| `shell`    | `AdamStudio` — app entry point, `App.xaml.cs`, `MainWindow`    |
| `core`     | `AdamStudio.Core` — converters, models, constants, localization|
| `services` | `AdamStudio.Services` — all service implementations            |
| `controls` | `AdamStudio.Controls` — custom controls, region adapters       |
| `menu`     | `Modules/AdamStudio.Modules.MenuRegion`                        |
| `content`  | `Modules/AdamStudio.Modules.ContentRegion`                     |
| `status`   | `Modules/AdamStudio.Modules.StatusBar`                         |
| `flyouts`  | `Modules/AdamStudio.Modules.FlayoutsRegion`                    |
| `settings` | `Modules/AdamStudio.Modules.SettingsRegion`                    |
| `toolbar`  | `Modules/AdamStudio.Modules.ToolBarRegion`                     |
| `legacy`   | `Legacy/` — deprecated code, do not add new features           |

Use a scope-less commit only when the change spans multiple scopes (e.g.
a solution-wide TFM bump, root `.editorconfig`, `Directory.Build.props`).

### Examples

- `feat(services): add reconnection with exponential backoff`
- `fix(content): handle webview navigation failure gracefully`
- `refactor(services): extract service registration into extension methods`
- `perf(core): cache culture lookup in localization service`
- `test(services): add integration test for TCP server handshake`
- `build: target net10.0 in all projects`
- `docs: update roadmap with tier 3 plan`
- `ci: add GitHub Actions build workflow`
- `chore: add root .editorconfig`
- `revert: undo settings flyout auto-save`

## Versioning

This project follows [Semantic Versioning](https://semver.org/) with the
rules below. The version lives in the `<Version>` and `<FileVersion>`
properties of the shell `.csproj` and is injected from the Git tag by the
CI pipeline.

### Tag format

Tags follow the format `v.X.Y.Z`. AdamStudio is a standalone desktop
application; it has a single version per release.

### Bump rules

- **MAJOR (X.0.0)** — breaking changes that require user action or that
  break compatibility with the Adam-Servers backend. Examples: removing a
  WebSocket message type, changing the Blockly to C# message contract,
  changing a default port, removing a settings group, a UI redesign that
  removes user workflows.
- **MINOR (0.X.0)** — new backward-compatible functionality. Examples:
  adding a new Blockly block category, a new settings flyout, a new
  communication channel to Adam-Servers, a significant internal refactor
  (module split, new shared project) that does not break the wire protocol.
- **PATCH (0.0.X)** — backward-compatible bug fixes and security
  hardening. Examples: fixing a crash on disconnect, fixing a settings
  persistence bug, updating a dependency for a security patch.

### How to decide

When a change touches multiple categories, bump the **highest** affected
component. For example, a commit that both fixes a bug (PATCH) and adds
a new flyout (MINOR) results in a MINOR bump.

### Wire-protocol compatibility with Adam-Servers

AdamStudio communicates with Adam-Servers over WebSocket/JSON. A
wire-protocol change (a new WebSocket message type, a changed DTO field
meaning) is a breaking change for the backend and bumps AdamStudio
MAJOR **only if** Adam-Servers cannot accept the old contract after the
upgrade. If the change is additive (a new optional field, a new message
type that the old server ignores), it is MINOR. Coordinate backend
releases with the Adam-Servers repository.

### Worked example: 2.0.1 to 2.1.0

The Tier 1 foundation pass shipped as `2.1.0`. It included:

- `chore:` root `.editorconfig` — tooling, not user-visible. PATCH on
  its own.
- `refactor:` target `net10.0` in all projects — internal, no wire
  change. MINOR (significant internal refactor).
- `build:` `Directory.Build.props` + Central Package Management —
  internal build infrastructure. MINOR (significant internal refactor).
- `build:` safe package bumps (Serilog, AvalonEdit, MahApps, etc.) —
  dependency updates, no wire change. PATCH (or MINOR if the bump
  enables new user-facing functionality).

The highest bump is MINOR, so the release is `2.1.0`, not `2.0.2`.
