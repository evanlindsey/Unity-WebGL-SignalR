using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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

    private SignalRLib srLib;
    private Text uiText;

    void Start()
    {
        uiText = GameObject.Find("Text").GetComponent<Text>();
        DisplayMessage(statusText);

        var handlers = new List<string>() { HANDLER_A, HANDLER_B };
        srLib = new SignalRLib(signalRHubURL, handlers, true);

        srLib.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
        {
            Debug.Log(e.ConnectionId);
            DisplayMessage(connectedText);

            var json1 = new JsonPayload
            {
                message = messageToSendA
            };
            srLib.SendToHub(hubMethodA, JsonUtility.ToJson(json1));

            var json2 = new JsonPayload
            {
                message = messageToSendB
            };
            srLib.SendToHub(hubMethodB, JsonUtility.ToJson(json2));
        };

        srLib.HandlerInvoked += (object sender, HandlerEventArgs e) =>
        {
            var json = JsonUtility.FromJson<JsonPayload>(e.Payload);

            switch (e.HandlerName)
            {
                case HANDLER_A:
                    DisplayMessage($"{HANDLER_A}: {json.message}");
                    break;
                case HANDLER_B:
                    DisplayMessage($"{HANDLER_B}: {json.message}");
                    break;
                default:
                    Debug.Log($"Handler: '{e.HandlerName}' not defined");
                    break;
            }
        };
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
