# Unity WebGL SignalR

A basic library with example components for using SignalR in Unity WebGL.

The hub is set to receive and broadcast a global message that invokes the listeners of all connected clients.

This functionality could be greatly extended for actual use in games or applications.

## Repo Components

- **Unity**: Example Unity3D project making use of all components
- **Server**: ASP.NET Core starter project with single hub for connection
- **Client**: Node/Express project to serve built WebGL files

## Library

The files needed for importing as a 'Plugin' in a Unity3D project are located at:

[Unity/Assets/Plugins/SignalR](./Unity/Assets/Plugins/SignalR)

Note: The C# client packages for SignalR are only needed for use in the editor.

### Client JS File

Once the WebGL project is built, the following must be referenced in the 'head' section of index.html:

[https://cdn.jsdelivr.net/npm/@microsoft/signalr@3.1.2/dist/browser/signalr.min.js](https://cdn.jsdelivr.net/npm/@microsoft/signalr@3.1.2/dist/browser/signalr.min.js)

![Source](./Screenshots/src.png)

## Usage

Attach a script file to a GameObject in your scene and add the following code to your 'Start' method:

```c#
srLib = new SignalRLib();
srLib.Init("<SignalRHubURL>", "<HubListenerName>");

srLib.ConnectionStarted += (object sender, MessageEventArgs e) =>
{
    // DisplayMessage(e.Message);
    srLib.SendMessage("<HubMethodName>", "<MessageToSend>");
};

srLib.MessageReceived += (object sender, MessageEventArgs e) =>
{
    // DisplayMessage(e.Message);
};
```

### Properties

![Properties](./Screenshots/props.png)

![Hub](./Screenshots/hub.png)

- **SignalR Hub URL**: Endpoint for the target SignalR hub
- **Hub Listener Name**: Name of listener to be invoked by the hub
- **Hub Method Name**: Name of method in the Hub to receive message
- **Message to Send**: Message to send to the hub
- **Status Text**: Initial text to be displayed

### Successful Result

![Result](./Screenshots/res.png)

## References

- [Unity Manual - WebGL: Interacting with browser scripting](https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html)
- [Introduction to ASP.NET Core SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-3.1)
- [jirihybek/unity-websocket-webgl](https://github.com/jirihybek/unity-websocket-webgl)
