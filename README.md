# Unity WebGL SignalR

Currently tested against and targeting [Unity 6000.3.2f1](https://unity.com/releases/editor/whats-new/6000.3.2) (Unity 6).

## Repo Components

- **Unity**: Unity3D project with Core SignalR Plugin and example scene. For use in the editor and WebGL.
- **Server**: ASP.NET Core 10 project with SignalR hub/methods for connection that serves built Unity WebGL files.

## Quick Start

```bash
make help              # Show all available commands
make install-signalr   # Install SignalR DLLs for Unity
make server            # Run the server
make health            # Check if server is running
```

## Plugin

The [Asset Package](https://docs.unity3d.com/Manual/AssetPackages.html) needed for adding the Plugin to a project can be found in the [Releases](https://github.com/evanlindsey/Unity-WebGL-SignalR/releases).

## Client C# Packages

- [_Microsoft.AspNetCore.SignalR.Client - 10.0.1_](https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client/10.0.1)

To work with SignalR in the Unity Editor, package dependencies (targeting .NET Standard 2.0) are required.

See [SETUP.md](./Unity/Assets/Plugins/SignalR/SETUP.md) for detailed cross-platform installation instructions, or run the quick setup below:

```powershell
# Requires PowerShell and .NET SDK
cd Unity/Assets/Plugins/SignalR/lib
./signalr.ps1
```

## Client JS File

The WebGL build requires the SignalR JavaScript client. This is handled by a custom WebGL template (`Assets/WebGLTemplates/SignalR/`) that includes the `<script>` tag automatically.

To configure: **Player Settings > Resolution and Presentation > Web Template** — select **SignalR**

All WebGL builds (including player-mode tests) will include the SignalR client automatically.

> **Note:** The JS client version (npm: `10.0.0`) and .NET DLL version (NuGet: `10.0.1` in `signalr.ps1`) are versioned independently. Update the template's `index.html` when upgrading the JS client.

## Usage

All plugin types are in the `UnityWebGLSignalR` namespace:

```csharp
using UnityWebGLSignalR;
```

### Public Methods

- **Init(string url)**: Initialize a new instance of HubConnectionBuilder with the URL
- **Init(string url, SignalROptions options)**: Initialize with URL and configuration options (auth, transport, timeouts)
- **Connect**: Start the connection to the hub and bind events
- **On**: Bind to the callback of a named client handler
- **Invoke(string methodName, params object[] args)**: Send arguments to a named hub method (WebGL: 0-10 args)
- **Stop**: Stop the connection to the hub
- **Dispose**: Clean up the connection and resources

### Public Properties

- **IsConnected**: Returns `true` when connected to the hub

### Public Events

- **ConnectionStarted**: Called on successful connection to the hub
- **ConnectionClosed**: Called when the hub connection is closed

### Arguments

As per the official SignalR API, up to 8 args can be received ([On](https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/clients/csharp/Client.Core/src/HubConnectionExtensions.cs)) and up to 10 args can be sent ([Invoke](https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/clients/csharp/Client.Core/src/HubConnectionExtensions.InvokeAsync.cs)).

- The example handler and hub method are set to serialize/deserialize a single argument as JSON.

### Configuration Options

Use `SignalROptions` to configure authentication, transport, and connection behavior:

| Option               | Type                        | Description                                    | Platforms   |
| -------------------- | --------------------------- | ---------------------------------------------- | ----------- |
| `AccessToken`        | `string`                    | Static Bearer auth token                       | Both        |
| `AccessTokenFactory` | `Func<Task<string>>`        | Async token provider                           | Editor only |
| `Headers`            | `Dictionary<string,string>` | Custom HTTP headers                            | Both        |
| `WithCredentials`    | `bool?`                     | Send cookies cross-origin (default: true)      | Both        |
| `Transport`          | `TransportType?`            | Force WebSockets/SSE/LongPolling               | Both        |
| `SkipNegotiation`    | `bool?`                     | Skip negotiation (WebSockets only)             | Both        |
| `HttpTimeout`        | `int?`                      | HTTP request timeout in ms (default: 100000)   | WebGL       |
| `ServerTimeout`      | `int?`                      | Server activity timeout in ms (default: 30000) | Both        |
| `KeepAliveInterval`  | `int?`                      | Ping interval in ms (default: 15000)           | Both        |
| `LogMessageContent`  | `bool?`                     | Log message bodies (default: false)            | WebGL       |
| `LogLevel`           | `SignalRLogLevel?`          | Client log level                               | WebGL       |
| `RetryDelays`        | `int[]`                     | Custom reconnect retry delays in ms            | Both        |

## Example

### Basic Usage

```csharp
using UnityWebGLSignalR;

void Start()
{
    var signalR = new SignalR();
    signalR.Init("<SignalRHubURL>");

    // Handler callback
    signalR.On("<HandlerName>", (string payload) =>
    {
        var json = JsonUtility.FromJson<JsonPayload>(payload);
        Debug.Log($"<HandlerName>: {json.message}");
    });

    // Connection callback
    signalR.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
    {
        Debug.Log($"Connected: {e.ConnectionId}");

        var json = new JsonPayload { message = "<MessageToSend>" };
        signalR.Invoke("<HubMethod>", JsonUtility.ToJson(json));
    };
    signalR.ConnectionClosed += (object sender, ConnectionEventArgs e) =>
    {
        Debug.Log($"Disconnected: {e.ConnectionId}");
    };

    signalR.Connect();
}

[Serializable]
public class JsonPayload
{
    public string message;
}
```

### With Authentication

```csharp
using UnityWebGLSignalR;

void Start()
{
    var signalR = new SignalR();
    signalR.Init("<SignalRHubURL>", new SignalROptions
    {
        AccessToken = "<YourJWTToken>",
        Transport = TransportType.WebSockets,
        SkipNegotiation = true
    });

    signalR.Connect();
}
```

## Testing

Tests are run from the Unity Test Runner window (`Window > General > Test Runner`):

- **EditMode tests**: Options serialization, plugin construction, enum validation (no server required)
- **PlayMode tests**: End-to-end integration tests (requires `make server` running on localhost:5000)
- **WebGL smoke test**: Headless browser test (Playwright) that loads the built WebGL app and verifies the SignalR jslib bridge works end-to-end — connection, hub method invocation, and handler callbacks

To run the smoke test locally (requires a WebGL build in `Server/wwwroot/`):

```bash
make server &               # Start the server
cd e2e && npm ci && npx playwright install chromium --with-deps
npx playwright test          # Run smoke test
```

## CI / CD

Two GitHub Actions workflows are included:

- **Unity CI** (`ci.yml`) — Runs on PRs. EditMode tests → WebGL build → Playwright smoke test.
- **Release** (`release.yml`) — Runs on push to `main`. Auto-determines version, builds WebGL + exports `.unitypackage` in parallel, commits fresh wwwroot, tags, and creates GitHub Release.

The Release workflow supports **dry-run mode** via `workflow_dispatch` — runs the full pipeline without committing or creating a release, useful for validating changes.

## Releasing

Releases are fully automated on merge to `main`:

- **Major/minor version**: Add a `version:X.Y.Z` label to the PR before merging.
- **Patch version**: No label needed — auto-increments the patch from the latest tag (e.g., `1.0.0` → `1.0.1`).

The release pipeline will: determine version → build WebGL + export `.unitypackage` (parallel) → commit wwwroot → create git tag → create GitHub Release with changelog and package attached.

## References

- [Unity Manual - WebGL: Interacting with browser scripting](https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html)
- [Introduction to ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0)
- [ASP.NET Core SignalR configuration](https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-10.0)
- [jirihybek/unity-websocket-webgl](https://github.com/jirihybek/unity-websocket-webgl)
