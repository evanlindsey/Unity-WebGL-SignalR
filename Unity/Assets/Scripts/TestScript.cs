using UnityEngine;
using UnityEngine.UI;
using System;

public class TestScript : MonoBehaviour
{

    public string SignalRHubURL = "http://localhost:5000/MainHub";

    public string HubMethodA = "SendPayloadA";
    public string HubMethodB = "SendPayloadB";

    public string MessageToSendA = "Hello World A";
    public string MessageToSendB = "Hello World B";

    public string StatusText = "Awaiting Connection...";
    public string ConnectedText = "Connection Started";

    private const string HandlerNameA = "ReceivePayloadA";
    private const string HandlerNameB = "ReceivePayloadB";

    private SignalRLib srLib;
    private Text uiText;

    void Start()
    {
        uiText = GameObject.Find("Text").GetComponent<Text>();
        DisplayMessage(StatusText);

        srLib = new SignalRLib();

        srLib.Init(SignalRHubURL);
        srLib.AddHandler(HandlerNameA);
        srLib.AddHandler(HandlerNameB);
        srLib.Connect();

        srLib.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
        {
            Debug.Log(e.ConnectionId);
            DisplayMessage(ConnectedText);

            var json1 = new JsonPayload
            {
                message = MessageToSendA
            };
            srLib.SendToHub(HubMethodA, JsonUtility.ToJson(json1));

            var json2 = new JsonPayload
            {
                message = MessageToSendB
            };
            srLib.SendToHub(HubMethodB, JsonUtility.ToJson(json2));
        };

        srLib.HandlerInvoked += (object sender, HandlerEventArgs e) =>
        {
            var json = JsonUtility.FromJson<JsonPayload>(e.Payload);

            switch (e.HandlerName)
            {
                case HandlerNameA:
                    DisplayMessage($"{HandlerNameA}: {json.message}");
                    break;
                case HandlerNameB:
                    DisplayMessage($"{HandlerNameB}: {json.message}");
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
