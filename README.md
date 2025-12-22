# Unity WebGL SignalR

Currently tested against and targeting [Unity 6000.2.14f1](https://unity.com/releases/editor/whats-new/6000.2.14) (Unity 6).

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
# Requires PowerShell and NuGet CLI
cd Unity/Assets/Plugins/SignalR/lib
./signalr.ps1
```

## Client JS File

Once the Unity WebGL project is built, SignalR must be referenced in the 'head' section of [index.html](./Server/wwwroot/index.html):

```html
<script src="https://unpkg.com/@microsoft/signalr@10.0.0/dist/browser/signalr.min.js"></script>
```

## Usage

### Public Methods

- **Init**: Initialize a new instance of HubConnectionBuilder with the URL
- **Connect**: Start the connection to the hub and bind events
- **On**: Bind to the callback of a named client handler
- **Invoke**: Send arguments to a named hub method
- **Stop**: Stop the connection to the hub

### Public Events

- **ConnectionStarted**: Called on successful connection to the hub
- **ConnectionClosed**: Called when the hub connection is closed

### Arguments

As per the official SignalR API, up to 8 args can be received ([On](https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/clients/csharp/Client.Core/src/HubConnectionExtensions.cs)) and up to 10 args can be sent ([Invoke](https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/clients/csharp/Client.Core/src/HubConnectionExtensions.InvokeAsync.cs)).

- The example handler and hub method are set to serialize/deserialize a single argument as JSON.

## Example

```c#
void Start()
{
    // Initialize SignalR
    var signalR = new SignalR();
    signalR.Init("<SignalRHubURL>");

    // Handler callback
    signalR.On("<HandlerName>", (string payload) =>
    {
        // Deserialize payload from JSON
        var json = JsonUtility.FromJson<JsonPayload>(payload);
        Debug.Log($"<HandlerName>: {json.message}");
    });

    // Connection callback
    signalR.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
    {
        // Log the connected ID
        Debug.Log($"Connected: {e.ConnectionId}");

        // Send payload to hub as JSON
        var json1 = new JsonPayload
        {
            message = "<MessageToSend>"
        };
        signalR.Invoke("<HubMethod>", JsonUtility.ToJson(json1));
    };
    signalR.ConnectionClosed += (object sender, ConnectionEventArgs e) =>
    {
        // Log the disconnected ID
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

## References

- [Unity Manual - WebGL: Interacting with browser scripting](https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html)
- [Introduction to ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0)
- [jirihybek/unity-websocket-webgl](https://github.com/jirihybek/unity-websocket-webgl)
