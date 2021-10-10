using AOT;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class ConnectionEventArgs : EventArgs
{
    public string ConnectionId { get; set; }
}

public class SignalR
{
    private static SignalR instance;
    public SignalR()
    {
        instance = this;
    }

    private static void OnConnectionStarted(string connectionId)
    {
        var args = new ConnectionEventArgs
        {
            ConnectionId = connectionId
        };
        instance.ConnectionStarted?.Invoke(instance, args);
    }
    public event EventHandler<ConnectionEventArgs> ConnectionStarted;

    private static void OnConnectionClosed(string connectionId)
    {
        var args = new ConnectionEventArgs
        {
            ConnectionId = connectionId
        };
        instance.ConnectionClosed?.Invoke(instance, args);
    }
    public event EventHandler<ConnectionEventArgs> ConnectionClosed;

#if UNITY_EDITOR || PLATFORM_SUPPORTS_MONO

    private HubConnection connection;
    private static string lastConnectionId;

    public void Init(string url)
    {
        try
        {
            connection = new HubConnectionBuilder()
            .WithUrl(url)
            .Build();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void Connect()
    {
        try
        {
            await connection.StartAsync();

            lastConnectionId = connection.ConnectionId;

            connection.Closed -= OnConnectionClosedEvent;
            connection.Reconnecting -= OnConnectionReconnectingEvent;
            connection.Reconnected -= OnConnectionReconnectedEvent;

            connection.Closed += OnConnectionClosedEvent;
            connection.Reconnecting += OnConnectionReconnectingEvent;
            connection.Reconnected += OnConnectionReconnectedEvent;

            OnConnectionStarted(lastConnectionId);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void Stop()
    {
        try
        {
            await connection.StopAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private static Task OnConnectionClosedEvent(Exception exception)
    {
        if (exception != null)
        {
            Debug.LogError(exception.Message);
        }

        OnConnectionClosed(lastConnectionId);

        return Task.CompletedTask;
    }

    private static Task OnConnectionReconnectingEvent(Exception exception)
    {
        Debug.Log($"Connection started reconnecting due to an error: {exception.Message}");

        return Task.CompletedTask;
    }

    private static Task OnConnectionReconnectedEvent(string connectionId)
    {
        Debug.Log($"Connection successfully reconnected. The ConnectionId is now: {connectionId}");

        lastConnectionId = connectionId;

        OnConnectionStarted(lastConnectionId);

        return Task.CompletedTask;
    }

    #region Invoke Editor
    public async void Invoke(string methodName, object arg1) =>
        await connection.InvokeAsync(methodName, arg1);
    public async void Invoke(string methodName, object arg1, object arg2) =>
        await connection.InvokeAsync(methodName, arg1, arg2);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3, arg4);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3, arg4, arg5);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3, arg4, arg5, arg6);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    public async void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10) =>
        await connection.InvokeAsync(methodName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    #endregion

    #region On Editor
    public void On<T1>(string methodName, Action<T1> handler) =>
        connection.On(methodName, (T1 arg1) => handler.Invoke(arg1));
    public void On<T1, T2>(string methodName, Action<T1, T2> handler) =>
        connection.On(methodName, (T1 arg1, T2 arg2) => handler.Invoke(arg1, arg2));
    public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler) =>
        connection.On(methodName, (T1 arg1, T2 arg2, T3 arg3) => handler.Invoke(arg1, arg2, arg3));
    public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler) =>
        connection.On(methodName, (T1 arg1, T2 arg2, T3 arg3, T4 arg4) => handler.Invoke(arg1, arg2, arg3, arg4));
    public void On<T1, T2, T3, T4, T5>(string methodName, Action<T1, T2, T3, T4, T5> handler) =>
        connection.On(methodName, (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => handler.Invoke(arg1, arg2, arg3, arg4, arg5));
    public void On<T1, T2, T3, T4, T5, T6>(string methodName, Action<T1, T2, T3, T4, T5, T6> handler) =>
        connection.On(methodName, (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => handler.Invoke(arg1, arg2, arg3, arg4, arg5, arg6));
    public void On<T1, T2, T3, T4, T5, T6, T7>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7> handler) =>
        connection.On(methodName, (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => handler.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
    public void On<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7, T8> handler) =>
        connection.On(methodName, (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => handler.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
    #endregion

#elif UNITY_WEBGL

    #region Init JS
    [DllImport("__Internal")]
    private static extern void InitJs(string hubUrl);

    public void Init(string url)
    {
        InitJs(url);
    }
    #endregion

    #region Stop JS
    [DllImport("__Internal")]
    private static extern void StopJs();

    public void Stop()
    {
        StopJs();
    }
    #endregion

    #region Connect JS
    [DllImport("__Internal")]
    private static extern void ConnectJs(Action<string> connectedCallback, Action<string> disconnectedCallback);

    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void ConnectedCallback(string connectionId)
    {
        OnConnectionStarted(connectionId);
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void DisconnectedCallback(string connectionId)
    {
        OnConnectionClosed(connectionId);
    }

    public void Connect()
    {
        ConnectJs(ConnectedCallback, DisconnectedCallback);
    }
    #endregion

    #region Invoke JS
    [DllImport("__Internal")]
    private static extern void InvokeJs(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10);

    public void Invoke(string methodName, object arg1) =>
        InvokeJs(methodName, arg1.ToString(), null, null, null, null, null, null, null, null, null);
    public void Invoke(string methodName, object arg1, object arg2) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), null, null, null, null, null, null, null, null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), null, null, null, null, null, null, null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), arg4.ToString(), null, null, null, null, null, null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), arg4.ToString(), arg5.ToString(), null, null, null, null, null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), arg4.ToString(), arg5.ToString(), arg6.ToString(), null, null, null, null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), arg4.ToString(), arg5.ToString(), arg6.ToString(), arg7.ToString(), null, null, null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), arg4.ToString(), arg5.ToString(), arg6.ToString(), arg7.ToString(), arg8.ToString(), null, null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), arg4.ToString(), arg5.ToString(), arg6.ToString(), arg7.ToString(), arg8.ToString(), arg9.ToString(), null);
    public void Invoke(string methodName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10) =>
        InvokeJs(methodName, arg1.ToString(), arg2.ToString(), arg3.ToString(), arg4.ToString(), arg5.ToString(), arg6.ToString(), arg7.ToString(), arg8.ToString(), arg9.ToString(), arg10.ToString());
    #endregion

    #region On JS
    private delegate void HandlerAction(params object[] args);
    private static readonly Dictionary<string, List<Type>> types = new Dictionary<string, List<Type>>();
    private static readonly Dictionary<string, HandlerAction> handlers = new Dictionary<string, HandlerAction>();

    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string> handlerCallback);
    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string, string> handlerCallback);
    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string, string, string> handlerCallback);
    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string, string, string, string> handlerCallback);
    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string, string, string, string, string> handlerCallback);
    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string, string, string, string, string, string> handlerCallback);
    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string, string, string, string, string, string, string> handlerCallback);
    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, string argCount, Action<string, string, string, string, string, string, string, string, string> handlerCallback);

    [MonoPInvokeCallback(typeof(Action<string, string>))]
    private static void HandlerCallback1(string methodName, string arg1)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]));
    }
    [MonoPInvokeCallback(typeof(Action<string, string, string>))]
    private static void HandlerCallback2(string methodName, string arg1, string arg2)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]));
    }
    [MonoPInvokeCallback(typeof(Action<string, string, string, string>))]
    private static void HandlerCallback3(string methodName, string arg1, string arg2, string arg3)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]));
    }
    [MonoPInvokeCallback(typeof(Action<string, string, string, string, string>))]
    private static void HandlerCallback4(string methodName, string arg1, string arg2, string arg3, string arg4)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]));
    }
    [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string>))]
    private static void HandlerCallback5(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]));
    }
    [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string, string>))]
    private static void HandlerCallback6(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]), Convert.ChangeType(arg6, type[5]));
    }
    [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string, string, string>))]
    private static void HandlerCallback7(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]), Convert.ChangeType(arg6, type[5]), Convert.ChangeType(arg7, type[6]));
    }
    [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string, string, string, string>))]
    private static void HandlerCallback8(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8)
    {
        handlers.TryGetValue(methodName, out HandlerAction handler);
        types.TryGetValue(methodName, out List<Type> type);
        handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]), Convert.ChangeType(arg6, type[5]), Convert.ChangeType(arg7, type[6]), Convert.ChangeType(arg8, type[7]));
    }

    public void On<T1>(string methodName, Action<T1> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1) });
        handlers.Add(methodName, args => handler((T1)args[0]));
        OnJs(methodName, "1", HandlerCallback1);
    }
    public void On<T1, T2>(string methodName, Action<T1, T2> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1), typeof(T2) });
        handlers.Add(methodName, args => handler((T1)args[0], (T2)args[1]));
        OnJs(methodName, "2", HandlerCallback2);
    }
    public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1), typeof(T2), typeof(T3) });
        handlers.Add(methodName, args => handler((T1)args[0], (T2)args[1], (T3)args[2]));
        OnJs(methodName, "3", HandlerCallback3);
    }
    public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        handlers.Add(methodName, args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]));
        OnJs(methodName, "4", HandlerCallback4);
    }
    public void On<T1, T2, T3, T4, T5>(string methodName, Action<T1, T2, T3, T4, T5> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        handlers.Add(methodName, args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4]));
        OnJs(methodName, "5", HandlerCallback5);
    }
    public void On<T1, T2, T3, T4, T5, T6>(string methodName, Action<T1, T2, T3, T4, T5, T6> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
        handlers.Add(methodName, args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5]));
        OnJs(methodName, "6", HandlerCallback6);
    }
    public void On<T1, T2, T3, T4, T5, T6, T7>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
        handlers.Add(methodName, args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6]));
        OnJs(methodName, "7", HandlerCallback7);
    }
    public void On<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7, T8> handler)
    {
        types.Add(methodName, new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });
        handlers.Add(methodName, args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7]));
        OnJs(methodName, "8", HandlerCallback8);
    }
    #endregion

#else

#error PLATFORM NOT SUPPORTED

#endif
}
