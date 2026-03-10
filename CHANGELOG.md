# Changelog

## [1.0.0] - 2026-03-09

### Breaking Changes

- **Namespace**: All plugin types are now in the `UnityWebGLSignalR` namespace. Add `using UnityWebGLSignalR;` to your scripts.
- **Invoke API**: The 10 `Invoke` overloads have been replaced with a single `Invoke(string methodName, params object[] args)` method. Existing call sites are source-compatible.
- **Minimum Unity version**: Legacy Emscripten compatibility code removed. Requires Unity 2021+ (tested on Unity 6).

### Added

- `SignalROptions` class for configuring authentication, transport, timeouts, and other connection settings.
- `TransportType` and `SignalRLogLevel` enums for strongly-typed option values.
- `Init(string url, SignalROptions options)` overload for passing configuration options.
- `IsConnected` property to check connection state (Editor and WebGL).
- `IDisposable` implementation for proper cleanup of `HubConnection` resources.
- `WithAutomaticReconnect()` enabled by default on both Editor and WebGL paths.
- Custom retry delays via `SignalROptions.RetryDelays`.
- Assembly definition (`UnityWebGLSignalR.asmdef`) for proper project references.
- CORS support on the server for development mode.
- Unity EditMode tests (28) for options serialization and plugin logic.
- Unity PlayMode integration tests (5) for end-to-end client-server validation.
- GitHub Actions CI (`ci.yml`) for Unity EditMode tests, WebGL builds, and smoke tests on PRs.
- Automated release pipeline (`release.yml`): merging to `main` auto-determines version (PR label or patch increment), exports `.unitypackage`, tags, and creates a GitHub Release.
- Reusable test workflow (`test.yml`) used by CI pipeline.
- Composite action (`setup-unity`) for disk space cleanup, PowerShell install, SignalR DLLs, and Unity Library caching.
- `ExportPackage` editor script for CI-driven `.unitypackage` export with version from environment variable.
- Custom WebGL template (`Assets/WebGLTemplates/SignalR/`) with SignalR JS client `<script>` tag included automatically in all WebGL builds.
- Release pipeline auto-updates `Server/wwwroot/` with fresh WebGL build on merge to main.
- WebGL smoke test (Playwright) — headless browser test verifying the jslib SignalR bridge end-to-end in CI.
- Release dry-run mode via `workflow_dispatch`.

### Fixed

- **Memory leak**: JavaScript `invokeCallback` allocated buffers via `_malloc` but never called `_free`. Every callback invocation leaked memory.
- **Falsy argument bug**: JavaScript `InvokeJs` used truthiness checks (`if (arg1 && arg2)`) which failed for empty strings, `"0"`, and `"false"`. Now uses `!== 0` null-pointer check with a `NULL_SENTINEL` for marshaling null arguments through the C#/JS boundary.
- **Null handler crash**: WebGL `HandlerCallback` methods did not check `TryGetValue` results, causing `NullReferenceException` for unregistered methods.
- **Async void crash**: Editor `Invoke` methods were `async void` with no try/catch. Exceptions from `InvokeAsync` would crash silently.
- **Duplicate On() crash**: Calling `On()` twice for the same method name threw `ArgumentException` from `Dictionary.Add`. Now uses indexer assignment.
- **Reconnecting NRE**: `OnConnectionReconnectingEvent` accessed `exception.Message` without null check (fixed in both Editor and jslib paths).
- **Event handler race**: Connection event handlers were subscribed after `StartAsync` completed, creating a window where close events could be missed. Moved to `Init()`.
- **WebGL Dispose() leak**: Static `types` and `handlers` dictionaries were never cleared on dispose, leaking handler registrations across sessions.

### Changed

- `On<T>` Editor overloads simplified to pass handler directly to `connection.On()`.
- JavaScript bridge (`SignalR.jslib`) reduced from 240 to ~170 lines by replacing duplicated if/else chains with loops.
- `OnJs` handler slots (`handlerCallback1-8`) replaced with per-registration closure capture.
- `signalr.ps1` switched from `nuget` CLI to `dotnet restore` — removes the NuGet CLI dependency.
- TestScript updated to use `[SerializeField]` instead of `GameObject.Find`.
- Instance replacement now logs a warning.
- Removed `version.txt` — version now derived from git tags.
