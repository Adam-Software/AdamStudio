# Contributing Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/)
specification. This helps us generate automated changelogs and maintain a
clean project history.

For coding conventions (naming, logging, DI, async, JSON, XAML), see the
**[Coding Guidelines](docs/coding-guidelines.md)**.

## Commit Message Format

Each commit message must follow this structure:

`<type>(<scope>): <short summary>`

### Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `ci`: Changes to CI configuration files and scripts
- `test`: Adding or correcting tests

### Scopes

The solution is a modular WPF application. Each scope maps to a project
or module:

| Scope      | Project / Directory                                      |
|------------|----------------------------------------------------------|
| `shell`    | `AdamController` — app entry point, `App.xaml.cs`, `MainWindow` |
| `core`     | `AdamController.Core` — converters, models, constants, localization |
| `services` | `AdamController.Services` — all service implementations   |
| `controls` | `AdamController.Controls` — custom controls, region adapters |
| `menu`     | `Modules/AdamController.Modules.MenuRegion`              |
| `content`  | `Modules/AdamController.Modules.ContentRegion`           |
| `status`   | `Modules/AdamController.Modules.StatusBarRegion`         |
| `flyouts`  | `Modules/AdamController.Modules.FlyoutsRegion`           |

Use a scope-less commit only when the change spans multiple scopes (e.g.
a solution-wide `.csproj` version bump).

### Examples

- `feat(services): add reconnection with exponential backoff`
- `fix(content): handle webview navigation failure gracefully`
- `refactor(services): extract service registration into extension methods`
- `docs: update roadmap`
- `ci: add GitHub Actions build workflow`

## Versioning

This project follows [Semantic Versioning](https://semver.org/) with the
rules below. The version lives in the `<Version>` and `<FileVersion>`
properties of the app's `.csproj` and is injected from the Git tag by the
CI pipeline.

### Tag format

AdamStudio and Adam-Servers ship independently and live in separate
repositories. Tags are prefixed to avoid collisions:

- **Client:** `client.v.X.Y.Z` (this repo)
- **Server:** `server.v.X.Y.Z` (Adam-Servers repo)

### Bump rules

- **MAJOR (X.0.0)** — breaking changes that require user action or that
  break compatibility between client and server. Examples: removing a
  WebSocket message type, changing the Blockly ↔ C# message contract,
  changing a default port, removing a settings group.
- **MINOR (0.X.0)** — new backward-compatible functionality. Examples:
  adding a new Blockly block category, a new settings flyout, a new
  server communication channel, a significant internal refactor (module
  split, new shared project) that does not break the wire protocol.
- **PATCH (0.0.X)** — backward-compatible bug fixes and security
  hardening. Examples: fixing a crash on disconnect, fixing a settings
  persistence bug, updating a dependency for a security patch.

### How to decide

When a change touches multiple categories, bump the **highest** affected
component. For example, a commit that both fixes a bug (PATCH) and adds
a new flyout (MINOR) results in a MINOR bump.

### Client vs server

A client-only change (e.g. a UI refactor, a new converter) bumps only
the client version. A wire-protocol change (e.g. a new WebSocket message
type, a changed DTO) bumps **both** client and server, because both
sides must understand the new contract.