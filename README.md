# Unity WebGL SignalR

## Repo Components

- **Unity**: Unity3D project with Core SignalR Plugin and example scene. For use in the editor and WebGL.
- **Server**: ASP.NET Core project with SignalR hub/methods for connection (CORS enabled).
- **Client**: Node.js/Express project to serve built Unity WebGL files.

## Plugin

The [Unity Package](https://docs.unity3d.com/Manual/AssetPackages.html) needed for adding the Plugin to a project can be found in the [Releases](https://github.com/evanlindsey/Unity-WebGL-SignalR/releases).

## Client C# Packages

- [_Microsoft.AspNetCore.SignalR.Client - 3.1.19_](https://packages.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client/3.1.19)

To work with SignalR in the Unity Editor, package dependencies (targeting .NET Standard 2.0) are required.

First, you must have the [NuGet CLI](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) installed locally ([v4.9.4](https://dist.nuget.org/win-x86-commandline/v4.9.4/nuget.exe) recommended).

Once NuGet is installed, execute the following command in [PowerShell](https://github.com/PowerShell/PowerShell) from the Plugin's [lib](./Unity/Assets/Plugins/SignalR/lib) directory to import the target .dll files.

```powershell
./signalr.ps1
```

## Client JS File

Once the Unity WebGL project is built, SignalR must be referenced in the 'head' section of index.html:

```javascript
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@3.1.19/dist/browser/signalr.min.js"></script>
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

As per the official SignalR API, up to 8 args can be received (On) and up to 10 args can be sent (Invoke).

- The example handlers and hub methods are set to serialize/deserialize a single argument as JSON.

## Example

```c#
void Start()
{
    // Initialize SignalR
    var signalR = new SignalR();
    signalR.Init("<SignalRHubURL>");

    // Handler callbacks
    signalR.On("<HandlerNameA>", (string payload) =>
    {
        // Deserialize payload A from JSON
        var json = JsonUtility.FromJson<JsonPayload>(payload);
        Debug.Log($"<HandlerNameA>: {json.message}");
    });
    signalR.On("<HandlerNameB>", (string payload) =>
    {
        // Deserialize payload B from JSON
        var json = JsonUtility.FromJson<JsonPayload>(payload);
        Debug.Log($"<HandlerNameB>: {json.message}");
    });

    // Connection callbacks
    signalR.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
    {
        // Log the connected ID
        Debug.Log($"Connected: {e.ConnectionId}");

        // Send payload A to hub as JSON
        var json1 = new JsonPayload
        {
            message = "<MessageToSendA>"
        };
        signalR.Invoke("<HubMethodA>", JsonUtility.ToJson(json1));

        // Send payload B to hub as JSON
        var json2 = new JsonPayload
        {
            message = "<MessageToSendB>"
        };
        signalR.Invoke("<HubMethodB>", JsonUtility.ToJson(json2));
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
- [Introduction to ASP.NET Core SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-3.1)
- [jirihybek/unity-websocket-webgl](https://github.com/jirihybek/unity-websocket-webgl)
