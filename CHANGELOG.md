# Changelog

## [Unreleased]

### Breaking Changes

- **Namespace**: All plugin types are now in the `UnityWebGLSignalR` namespace. Add `using UnityWebGLSignalR;` to your scripts.
- **Invoke API**: The 10 `Invoke` overloads have been replaced with a single `Invoke(string methodName, params object[] args)` method. Existing call sites are source-compatible.
- **Minimum Unity version**: Legacy Emscripten compatibility code removed. Requires Unity 2021+ (tested on Unity 6).

### Added

- `SignalROptions` class for configuring authentication, transport, timeouts, and other connection settings.
- `Init(string url, SignalROptions options)` overload for passing configuration options.
- `IsConnected` property to check connection state.
- `IDisposable` implementation for proper cleanup of `HubConnection` resources.
- `WithAutomaticReconnect()` enabled by default on both Editor and WebGL paths.
- Custom retry delays via `SignalROptions.RetryDelays`.
- CORS support on the server for development mode.
- Unity EditMode tests for options serialization and plugin logic.
- Unity PlayMode integration tests for end-to-end client-server validation.
- GitHub Actions CI for Unity EditMode tests and WebGL builds on PRs and merges.
- Automated release pipeline: merging to `main` auto-determines version (PR label or patch increment), runs tests, exports `.unitypackage`, tags, and creates a GitHub Release.
- Custom WebGL template (`Assets/WebGLTemplates/SignalR/`) with SignalR JS client `<script>` tag included automatically in all WebGL builds.
- CI auto-updates `Server/wwwroot/` with fresh WebGL build on merge to main.
- WebGL smoke test (Playwright) — headless browser test verifying the jslib SignalR bridge end-to-end in CI.
- CI dry-run mode via `workflow_dispatch` for both Unity CI and Release pipelines.

### Fixed

- **Memory leak**: JavaScript `invokeCallback` allocated buffers via `_malloc` but never called `_free`. Every callback invocation leaked memory.
- **Falsy argument bug**: JavaScript `InvokeJs` used truthiness checks (`if (arg1 && arg2)`) which failed for empty strings, `"0"`, and `"false"`. Now uses `!== 0` null-pointer check.
- **Null handler crash**: WebGL `HandlerCallback` methods did not check `TryGetValue` results, causing `NullReferenceException` for unregistered methods.
- **Async void crash**: Editor `Invoke` methods were `async void` with no try/catch. Exceptions from `InvokeAsync` would crash silently.
- **Duplicate On() crash**: Calling `On()` twice for the same method name threw `ArgumentException` from `Dictionary.Add`. Now uses indexer assignment.
- **Reconnecting NRE**: `OnConnectionReconnectingEvent` accessed `exception.Message` without null check.
- **Event handler race**: Connection event handlers were subscribed after `StartAsync` completed, creating a window where close events could be missed. Moved to `Init()`.
- **WebGL Dispose() leak**: Static `types` and `handlers` dictionaries were never cleared on dispose, leaking handler registrations across sessions.

### Changed

- `On<T>` Editor overloads simplified to pass handler directly to `connection.On()`.
- JavaScript bridge (`SignalR.jslib`) reduced from 240 to ~170 lines by replacing duplicated if/else chains with loops.
- `OnJs` handler slots (`handlerCallback1-8`) replaced with per-registration closure capture.
- TestScript updated to use `[SerializeField]` instead of `GameObject.Find`.
- Instance replacement now logs a warning.

## [0.0.7] - Previous

- Upgrade to Unity 6, .NET 10, SignalR 10.
