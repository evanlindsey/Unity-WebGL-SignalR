using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{

    public string SignalRHubURL = "http://localhost:5000/MainHub";
    public string HubListenerName = "ReceiveMessage";
    public string HubMethodName = "SendMessage";

    public string MessageToSend = "Hello World!";

    public string StatusText = "Awaiting Connection...";
    public string ConnectedText = "Connection Started";

    SignalRLib srLib;
    Text uiText;

    void Start()
    {
        uiText = GameObject.Find("Text").GetComponent<Text>();
        DisplayMessage(StatusText);

        srLib = new SignalRLib();
        srLib.Init(SignalRHubURL, HubListenerName);

        srLib.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
        {
            Debug.Log(e.ConnectionId);
            DisplayMessage(ConnectedText);
            srLib.SendMessage(HubMethodName, MessageToSend);
        };

        srLib.MessageReceived += (object sender, MessageEventArgs e) =>
        {
            DisplayMessage(e.Message);
        };
    }

    void DisplayMessage(string message)
    {
        uiText.text += $"\n{message}";
    }

}
