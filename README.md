# Unity WebGL SignalR

## Repo Components

- **Unity**: Unity3D project with Core SignalR Plugin and example scene. For use in the editor and WebGL.
- **Server**: ASP.NET Core project with SignalR hub/methods for connection (CORS enabled).
- **Client**: Node.js/Express project to serve built Unity WebGL files.

## Plugin

The [Unity Package](https://docs.unity3d.com/Manual/AssetPackages.html) needed for adding the Plugin to a project can be found in the [Releases](https://github.com/evanlindsey/Unity-WebGL-SignalR/releases).

## Client C# Packages

- _Microsoft.AspNetCore.SignalR.Client - 3.1.10_

To work with SignalR in the Unity Editor, package dependencies (targeting .NET Standard 2.0) are required.

First, you must have the [NuGet CLI](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) installed locally.

Once NuGet is installed, execute the following command in [PowerShell](https://github.com/PowerShell/PowerShell) from the Plugin's [lib](./Unity/Assets/Plugins/SignalR/lib) directory to import the target .dll files.

```powershell
./signalr.ps1
```

## Client JS File

Once the Unity WebGL project is built, SignalR must be referenced in the 'head' section of index.html:

```javascript
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@3.1.10/dist/browser/signalr.min.js"></script>
```

## Usage

### Public Methods

- **Init**: Initialize a new instance of SignalRLib
- **AddHandler**: Add the name of a handler to bind to the callback
- **Connect**: Connect to the target SignalRHub
- **SendToHub**: Send a payload to the target hub method

### Public Events

- **ConnectionStarted**: Called on successful connection to the hub
- **HandlerInvoked**: Called each time one of the added handlers is invoked

## Arguments

Arguments sent to/from client handlers and server hub methods are single strings.

- If multiple arguments or structures (such as an array) are needed, JSON is the best option.
- The example handlers and hub methods are set to serialize/deserialize arguments as JSON.
- It is still possible to use plain strings. Adjust your code accordingly.

## Example

```c#
void Start()
{
    // Initialize SignalR
    var srLib = new SignalRLib();
    srLib.Init("<SignalRHubURL>");

    // Add all client handlers
    srLib.AddHandler("<HandlerNameA>");
    srLib.AddHandler("<HandlerNameB>");

    // Connect to hub
    srLib.Connect();

    // Connection callback
    srLib.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
    {
        // Get current connection ID
        Debug.Log(e.ConnectionId);

        // Send payload A to hub
        var json1 = new JsonPayload
        {
            message = "<MessageToSendA>"
        };
        srLib.SendToHub("<HubMethodA>", JsonUtility.ToJson(json1));

        // Send payload B to hub
        var json2 = new JsonPayload
        {
            message = "<MessageToSendB>"
        };
        srLib.SendToHub("<HubMethodB>", JsonUtility.ToJson(json2));
    };

    // Handler callback
    srLib.HandlerInvoked += (object sender, HandlerEventArgs e) =>
    {
        // Deserialize payload from JSON
        var json = JsonUtility.FromJson<JsonPayload>(e.Payload);

        // Logic for multiple added handlers
        switch (e.HandlerName)
        {
            case "<HandlerNameA>":
                Debug.Log($"<HandlerNameA>: {json.message}");
                break;
            case "<HandlerNameB>":
                Debug.Log($"<HandlerNameB>: {json.message}");
                break;
            default:
                Debug.Log($"Handler: '{e.HandlerName}' not defined");
                break;
        }
    };
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
