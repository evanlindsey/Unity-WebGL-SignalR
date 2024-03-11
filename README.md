# Unity WebGL SignalR

Currently tested against and targeting [Unity 2022.3.21f1 LTS](https://unity3d.com/unity/whats-new/2022.3.21).

## Repo Components

- **Unity**: Unity3D project with Core SignalR Plugin and example scene. For use in the editor and WebGL.
- **Server**: ASP.NET Core project with SignalR hub/methods for connection that serves built Unity WebGL files.

## Plugin

The [Asset Package](https://docs.unity3d.com/Manual/AssetPackages.html) needed for adding the Plugin to a project can be found in the [Releases](https://github.com/evanlindsey/Unity-WebGL-SignalR/releases).

## Client C# Packages

- [_Microsoft.AspNetCore.SignalR.Client - 8.0.2_](https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client/8.0.2)

To work with SignalR in the Unity Editor, package dependencies (targeting .NET Standard 2.0) are required.

First, you must have the [NuGet CLI](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) installed locally (**v6.9.1** tested as functional).

Once NuGet is installed, execute the following command in [PowerShell](https://github.com/PowerShell/PowerShell) from the Plugin's [lib](./Unity/Assets/Plugins/SignalR/lib) directory to import the target .dll files.

```powershell
./signalr.ps1
```

## Client JS File

Once the Unity WebGL project is built, SignalR must be referenced in the 'head' section of [index.html](./Server/wwwroot/index.html):

```html
<script src="https://www.unpkg.com/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js"></script>
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
- [Introduction to ASP.NET Core SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-6.0)
- [jirihybek/unity-websocket-webgl](https://github.com/jirihybek/unity-websocket-webgl)
