using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR.Client;
using AOT;

public class SignalRLib
{

    private static SignalRLib instance;

    public SignalRLib()
    {
        instance = this;
    }

#if UNITY_EDITOR

    private HubConnection connection;

    public async void Init(string hubUrl, string hubListener)
    {
        connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

        connection.On<string>(hubListener, (message) =>
        {
            OnMessageReceived(message);
        });

        try
        {
            await connection.StartAsync();

            OnConnectionStarted("Connection Started");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void SendMessage(string hubMethod, string hubMessage)
    {
        await connection.InvokeAsync(hubMethod, hubMessage);
    }

#elif UNITY_WEBGL

    [DllImport("__Internal")]
    private static extern void Connect(string url, string listener, Action<string> cnx, Action<string> msg);

    [DllImport("__Internal")]
    private static extern void Invoke(string method, string message);

    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void ConnectionCallback(string message)
    {
        OnConnectionStarted(message);
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void MessageCallback(string message)
    {
        OnMessageReceived(message);
    }

    public void Init(string hubUrl, string hubListener)
    {
        Connect(hubUrl, hubListener, ConnectionCallback, MessageCallback);
    }

    public void SendMessage(string hubMethod, string hubMessage)
    {
        Invoke(hubMethod, hubMessage);
    }

#endif

    public event EventHandler<MessageEventArgs> MessageReceived;
    public event EventHandler<MessageEventArgs> ConnectionStarted;

    private static void OnMessageReceived(string message)
    {
        var args = new MessageEventArgs();
        args.Message = message;
        instance.MessageReceived?.Invoke(instance, args);
    }

    private static void OnConnectionStarted(string message)
    {
        var args = new MessageEventArgs();
        args.Message = message;
        instance.ConnectionStarted?.Invoke(instance, args);
    }

}

public class MessageEventArgs : EventArgs
{
    public string Message { get; set; }
}
