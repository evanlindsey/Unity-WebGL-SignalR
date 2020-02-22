using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{

    public string SignalRHubURL = "http://localhost:52611/MainHub";
    public string HubListenerName = "ReceiveMessage";
    public string HubMethodName = "SendMessage";
    public string MessageToSend = "Hello World!";
    public string StatusText = "Awaiting Connection...";

    SignalRLib srLib;
    Text uiText;

    void Start()
    {
        uiText = GameObject.Find("Text").GetComponent<Text>();
        DisplayMessage(StatusText);

        srLib = new SignalRLib();
        srLib.Init(SignalRHubURL, HubListenerName);

        srLib.ConnectionStarted += (object sender, MessageEventArgs e) =>
        {
            DisplayMessage(e.Message);
            srLib.SendMessage(HubMethodName, MessageToSend);
        };

        srLib.MessageReceived += (object sender, MessageEventArgs e) =>
        {
            DisplayMessage(e.Message);
        };
    }

    void DisplayMessage(string message)
    {
        Debug.Log(message);
        uiText.text += $"\n{message}";
    }

}
