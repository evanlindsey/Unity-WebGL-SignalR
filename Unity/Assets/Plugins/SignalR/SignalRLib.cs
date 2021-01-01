using AOT;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SignalRLib
{

    private static SignalRLib instance;

    public SignalRLib()
    {
        instance = this;
    }

    public SignalRLib(string hubUrl, List<string> handlerNames = null, bool connect = false) : this()
    {
        Init(hubUrl);
        if (handlerNames != null)
        {
            AddHandlers(handlerNames);
        }
        if (connect)
        {
            Connect();
        }
    }

    public void AddHandlers(List<string> handlerNames)
    {
        foreach (string handlerName in handlerNames)
        {
            AddHandler(handlerName);
        }
    }

#if UNITY_EDITOR

    private HubConnection connection;

    public void Init(string hubUrl)
    {
        connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();
    }

    public void AddHandler(string handlerName)
    {
        connection.On<string>(handlerName, (payload) =>
        {
            OnHandlerInvoked(handlerName, payload);
        });
    }

    public async void Connect()
    {
        try
        {
            await connection.StartAsync();
            OnConnectionStarted(connection.ConnectionId);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void SendToHub(string hubMethod, string payload)
    {
        await connection.InvokeAsync(hubMethod, payload);
    }

#elif UNITY_WEBGL

    private delegate void OnConnectionCallback(string connectionId);
    private delegate void OnHandlerCallback(string handlerName, string payload);

    [MonoPInvokeCallback(typeof(OnConnectionCallback))]
    private static void ConnectionCallback(string connectionId)
    {
        OnConnectionStarted(connectionId);
    }

    [MonoPInvokeCallback(typeof(OnHandlerCallback))]
    private static void HandlerCallback(string handlerName, string payload)
    {
        OnHandlerInvoked(handlerName, payload);
    }

    [DllImport("__Internal")]
    private static extern void InitJs(string hubUrl);

    [DllImport("__Internal")]
    private static extern void AddHandlerJs(string handlerName, OnHandlerCallback handlerCallback);

    [DllImport("__Internal")]
    private static extern void ConnectJs(OnConnectionCallback connectionCallback);

    [DllImport("__Internal")]
    private static extern void SendToHubJs(string hubMethod, string payload);

    public void Init(string hubUrl)
    {
        InitJs(hubUrl);
    }

    public void AddHandler(string handlerName)
    {
        AddHandlerJs(handlerName, HandlerCallback);
    }

    public void Connect()
    {
        ConnectJs(ConnectionCallback);
    }

    public void SendToHub(string hubMethod, string payload)
    {
        SendToHubJs(hubMethod, payload);
    }

#endif

    public event EventHandler<ConnectionEventArgs> ConnectionStarted;
    public event EventHandler<HandlerEventArgs> HandlerInvoked;

    private static void OnConnectionStarted(string connectionId)
    {
        var args = new ConnectionEventArgs
        {
            ConnectionId = connectionId
        };
        instance.ConnectionStarted?.Invoke(instance, args);
    }

    private static void OnHandlerInvoked(string handlerName, string payload)
    {
        var args = new HandlerEventArgs
        {
            HandlerName = handlerName,
            Payload = payload
        };
        instance.HandlerInvoked?.Invoke(instance, args);
    }

}

public class ConnectionEventArgs : EventArgs
{
    public string ConnectionId { get; set; }
}

public class HandlerEventArgs : EventArgs
{
    public string HandlerName { get; set; }
    public string Payload { get; set; }
}
