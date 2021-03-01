using UnityEngine;
using UnityEngine.UI;
using System;

public class TestScript : MonoBehaviour
{
    public string signalRHubURL = "http://localhost:5000/MainHub";

    public string hubMethodA = "SendPayloadA";
    public string hubMethodB = "SendPayloadB";

    public string messageToSendA = "Hello World A";
    public string messageToSendB = "Hello World B";

    public string statusText = "Awaiting Connection...";
    public string connectedText = "Connection Started";

    private const string HANDLER_A = "ReceivePayloadA";
    private const string HANDLER_B = "ReceivePayloadB";

    private Text uiText;

    void Start()
    {
        uiText = GameObject.Find("Text").GetComponent<Text>();
        DisplayMessage(statusText);

        var signalR = new SignalR();
        signalR.Init(signalRHubURL);

        signalR.On(HANDLER_A, (string payload) =>
        {
            var json = JsonUtility.FromJson<JsonPayload>(payload);
            DisplayMessage($"{HANDLER_A}: {json.message}");
        });
        signalR.On(HANDLER_B, (string payload) =>
        {
            var json = JsonUtility.FromJson<JsonPayload>(payload);
            DisplayMessage($"{HANDLER_B}: {json.message}");
        });

        signalR.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
        {
            Debug.Log(e.ConnectionId);
            DisplayMessage(connectedText);

            var json1 = new JsonPayload
            {
                message = messageToSendA
            };
            signalR.Invoke(hubMethodA, JsonUtility.ToJson(json1));
            var json2 = new JsonPayload
            {
                message = messageToSendB
            };
            signalR.Invoke(hubMethodB, JsonUtility.ToJson(json2));
        };

        signalR.Connect();
    }

    void DisplayMessage(string message)
    {
        uiText.text += $"\n{message}";
    }

    [Serializable]
    public class JsonPayload
    {
        public string message;
    }
}
