using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using UnityEngine;

public class SignalRInterface : SignalRLib
{
    public ConcurrentDictionary<string, Action<object[]>> Functions = new ConcurrentDictionary<string, Action<object[]>>();
    public bool IsConnected { get; set; } = false;

    public SignalRInterface()
    {
        HandlerInvoked += SignalRInterface_HandlerInvoked;
        ConnectionStarted += SignalRInterface_ConnectionStarted;
    }

    private void SignalRInterface_ConnectionStarted(object sender, ConnectionEventArgs e)
    {
        Debug.Log(e.ConnectionId);
        IsConnected = true;
    }

    private void SignalRInterface_HandlerInvoked(object sender, HandlerEventArgs e)
    {
        try 
        { 
            Action<object[]> action;

            if (Functions.TryGetValue(e.HandlerName, out action))
            {
                var arrayParameters = JsonSerializer.Deserialize<List<object>>(e.Payload);
                action.Invoke(arrayParameters.ToArray());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    [Serializable]
    public class JsonPayload
    {
        public string[] message;
    }
}


public static class SignalRInterfaceExtension
{
    public static T ToObject<T>(this JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<T>(json);
    }

    public static T ToObject<T>(this object element) =>
        ((JsonElement)element).ToObject<T>();

    public static void On<T1>(this SignalRInterface signalR,string methodName, Action<T1> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>()));
        signalR.AddHandler(methodName);
    }


    public static void On<T1, T2>(this SignalRInterface signalR, string methodName, Action<T1, T2> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>(), args[1].ToObject<T2>()));
        signalR.AddHandler(methodName);
    }

    public static void On<T1, T2, T3>(this SignalRInterface signalR, string methodName, Action<T1, T2, T3> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>(), args[1].ToObject<T2>(), args[2].ToObject<T3>() ));
        signalR.AddHandler(methodName);
    }

    public static void On<T1, T2, T3, T4>(this SignalRInterface signalR, string methodName, Action<T1, T2, T3, T4> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>(), args[1].ToObject<T2>(), args[2].ToObject<T3>(), args[3].ToObject<T4>()));
        signalR.AddHandler(methodName);
    }

    public static void On<T1, T2, T3, T4, T5>(this SignalRInterface signalR, string methodName, Action<T1, T2, T3, T4, T5> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>(), args[1].ToObject<T2>(), args[2].ToObject<T3>(), args[3].ToObject<T4>(), args[4].ToObject<T5>() ));
        signalR.AddHandler(methodName);
    }

    public static void On<T1, T2, T3, T4, T5, T6>(this SignalRInterface signalR, string methodName, Action<T1, T2, T3, T4, T5, T6> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>(), args[1].ToObject<T2>(), args[2].ToObject<T3>(), args[3].ToObject<T4>(), args[4].ToObject<T5>(), args[5].ToObject<T6>() ));
        signalR.AddHandler(methodName);
    }

    public static void On<T1, T2, T3, T4, T5, T6,T7>(this SignalRInterface signalR, string methodName, Action<T1, T2, T3, T4, T5, T6,T7> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>(), args[1].ToObject<T2>(), args[2].ToObject<T3>(), args[3].ToObject<T4>(), args[4].ToObject<T5>(), args[5].ToObject<T6>(), args[6].ToObject<T7>()));
        signalR.AddHandler(methodName);
    }

    public static void On<T1, T2, T3, T4, T5, T6,T7,T8>(this SignalRInterface signalR, string methodName, Action<T1, T2, T3, T4, T5, T6,T7,T8> handler)
    {
        signalR.Functions.TryAdd(methodName, args => handler(args[0].ToObject<T1>(), args[1].ToObject<T2>(), args[2].ToObject<T3>(), args[3].ToObject<T4>(), args[4].ToObject<T5>(), args[5].ToObject<T6>(), args[6].ToObject<T7>(), args[7].ToObject<T8>()));
        signalR.AddHandler(methodName);
    }
}