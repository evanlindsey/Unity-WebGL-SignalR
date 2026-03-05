using AOT;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityWebGLSignalR
{
    public class ConnectionEventArgs : EventArgs
    {
        public string ConnectionId { get; set; }
    }

    public enum TransportType
    {
        All = 0,
        WebSockets = 1,
        ServerSentEvents = 2,
        LongPolling = 4
    }

    public enum SignalRLogLevel
    {
        None = 0,
        Trace = 1,
        Debug = 2,
        Information = 3,
        Warning = 4,
        Error = 5,
        Critical = 6
    }

    [Serializable]
    public class SignalROptions
    {
        /// <summary>
        /// A static access token for Bearer authentication.
        /// On WebGL, this is passed to accessTokenFactory as a fixed value.
        /// For dynamic tokens on the Editor path, use AccessTokenFactory instead.
        /// </summary>
        public string AccessToken;

        /// <summary>
        /// Custom HTTP headers sent with every HTTP request.
        /// Note: headers do not work for WebSocket or SSE transports in browsers.
        /// </summary>
        public Dictionary<string, string> Headers;

        /// <summary>
        /// Whether to send credentials (cookies) in cross-origin requests. Default: true.
        /// </summary>
        public bool? WithCredentials;

        /// <summary>
        /// Which transport(s) to use. Default: All (auto-negotiate).
        /// </summary>
        public TransportType? Transport;

        /// <summary>
        /// Skip the negotiation step. Only valid when Transport is WebSockets.
        /// </summary>
        public bool? SkipNegotiation;

        /// <summary>
        /// Timeout in milliseconds for HTTP requests (not WebSocket/SSE streams). Default: 100000.
        /// </summary>
        public int? HttpTimeout;

        /// <summary>
        /// Server activity timeout in milliseconds. If no message received in this interval,
        /// the client considers the server disconnected. Default: 30000.
        /// </summary>
        public int? ServerTimeout;

        /// <summary>
        /// Keep-alive ping interval in milliseconds. Default: 15000.
        /// </summary>
        public int? KeepAliveInterval;

        /// <summary>
        /// Whether to log message content (may contain sensitive data). Default: false.
        /// </summary>
        public bool? LogMessageContent;

        /// <summary>
        /// Log level for the SignalR client. Default: Information.
        /// </summary>
        public SignalRLogLevel? LogLevel;

        /// <summary>
        /// Custom retry delays in milliseconds for automatic reconnection.
        /// Default: [0, 2000, 10000, 30000].
        /// </summary>
        public int[] RetryDelays;

        /// <summary>
        /// Editor/Standalone only: async function that provides a fresh access token.
        /// Takes precedence over AccessToken when set. Not available on WebGL
        /// (use AccessToken for a static token instead).
        /// </summary>
        [NonSerialized]
        public Func<Task<string>> AccessTokenFactory;

        public string ToJson()
        {
            var parts = new List<string>();

            if (AccessToken != null)
                parts.Add($"\"accessToken\":\"{EscapeJson(AccessToken)}\"");
            if (Headers != null && Headers.Count > 0)
            {
                var headerParts = new List<string>();
                foreach (var kv in Headers)
                    headerParts.Add($"\"{EscapeJson(kv.Key)}\":\"{EscapeJson(kv.Value)}\"");
                parts.Add($"\"headers\":{{{string.Join(",", headerParts)}}}");
            }
            if (WithCredentials.HasValue)
                parts.Add($"\"withCredentials\":{(WithCredentials.Value ? "true" : "false")}");
            if (Transport.HasValue)
                parts.Add($"\"transport\":{(int)Transport.Value}");
            if (SkipNegotiation.HasValue)
                parts.Add($"\"skipNegotiation\":{(SkipNegotiation.Value ? "true" : "false")}");
            if (HttpTimeout.HasValue)
                parts.Add($"\"timeout\":{HttpTimeout.Value}");
            if (ServerTimeout.HasValue)
                parts.Add($"\"serverTimeout\":{ServerTimeout.Value}");
            if (KeepAliveInterval.HasValue)
                parts.Add($"\"keepAliveInterval\":{KeepAliveInterval.Value}");
            if (LogMessageContent.HasValue)
                parts.Add($"\"logMessageContent\":{(LogMessageContent.Value ? "true" : "false")}");
            if (LogLevel.HasValue)
                parts.Add($"\"logLevel\":{(int)LogLevel.Value}");
            if (RetryDelays != null && RetryDelays.Length > 0)
                parts.Add($"\"retryDelays\":[{string.Join(",", RetryDelays)}]");

            return "{" + string.Join(",", parts) + "}";
        }

        private static string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }
    }

    public class SignalR : IDisposable
    {
        private static SignalR instance;

        public SignalR()
        {
            if (instance != null)
                Debug.LogWarning("SignalR: Creating a new instance replaces the previous one. Only one instance is supported.");
            instance = this;
        }

        public event EventHandler<ConnectionEventArgs> ConnectionStarted;
        public event EventHandler<ConnectionEventArgs> ConnectionClosed;

        private static void OnConnectionStarted(string connectionId)
        {
            var args = new ConnectionEventArgs { ConnectionId = connectionId };
            instance?.ConnectionStarted?.Invoke(instance, args);
        }

        private static void OnConnectionClosed(string connectionId)
        {
            var args = new ConnectionEventArgs { ConnectionId = connectionId };
            instance?.ConnectionClosed?.Invoke(instance, args);
        }

#if UNITY_EDITOR || PLATFORM_SUPPORTS_MONO

        private HubConnection connection;
        private static string lastConnectionId;

        public bool IsConnected => connection?.State == HubConnectionState.Connected;

        public void Init(string url) => Init(url, null);

        public void Init(string url, SignalROptions options)
        {
            try
            {
                var builder = new HubConnectionBuilder()
                    .WithUrl(url, httpOptions =>
                    {
                        if (options == null) return;

                        if (options.AccessTokenFactory != null)
                            httpOptions.AccessTokenProvider = options.AccessTokenFactory;
                        else if (options.AccessToken != null)
                            httpOptions.AccessTokenProvider = () => Task.FromResult(options.AccessToken);

                        if (options.Headers != null)
                        {
                            foreach (var kv in options.Headers)
                                httpOptions.Headers[kv.Key] = kv.Value;
                        }

                        if (options.Transport.HasValue && options.Transport.Value != TransportType.All)
                            httpOptions.Transports = (HttpTransportType)(int)options.Transport.Value;

                        if (options.SkipNegotiation.HasValue)
                            httpOptions.SkipNegotiation = options.SkipNegotiation.Value;
                    });

                if (options?.RetryDelays != null && options.RetryDelays.Length > 0)
                {
                    var delays = new TimeSpan[options.RetryDelays.Length];
                    for (int i = 0; i < options.RetryDelays.Length; i++)
                        delays[i] = TimeSpan.FromMilliseconds(options.RetryDelays[i]);
                    builder.WithAutomaticReconnect(delays);
                }
                else
                {
                    builder.WithAutomaticReconnect();
                }

                if (options?.ServerTimeout.HasValue == true)
                    builder.WithServerTimeout(TimeSpan.FromMilliseconds(options.ServerTimeout.Value));

                if (options?.KeepAliveInterval.HasValue == true)
                    builder.WithKeepAliveInterval(TimeSpan.FromMilliseconds(options.KeepAliveInterval.Value));

                connection = builder.Build();

                connection.Closed += OnConnectionClosedEvent;
                connection.Reconnecting += OnConnectionReconnectingEvent;
                connection.Reconnected += OnConnectionReconnectedEvent;
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
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Closed -= OnConnectionClosedEvent;
                connection.Reconnecting -= OnConnectionReconnectingEvent;
                connection.Reconnected -= OnConnectionReconnectedEvent;
                connection.DisposeAsync().AsTask().Wait();
                connection = null;
            }
            if (instance == this)
                instance = null;
        }

        private static Task OnConnectionClosedEvent(Exception exception)
        {
            if (exception != null)
                Debug.LogError(exception.Message);
            OnConnectionClosed(lastConnectionId);
            return Task.CompletedTask;
        }

        private static Task OnConnectionReconnectingEvent(Exception exception)
        {
            Debug.Log($"Connection started reconnecting due to an error: {exception?.Message}");
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
        public async void Invoke(string methodName, params object[] args)
        {
            try
            {
                // SendCoreAsync avoids the params-forwarding ambiguity of SendAsync
                await connection.SendCoreAsync(methodName, args);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
        #endregion

        #region On Editor
        public void On<T1>(string methodName, Action<T1> handler) =>
            connection.On(methodName, handler);
        public void On<T1, T2>(string methodName, Action<T1, T2> handler) =>
            connection.On(methodName, handler);
        public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler) =>
            connection.On(methodName, handler);
        public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler) =>
            connection.On(methodName, handler);
        public void On<T1, T2, T3, T4, T5>(string methodName, Action<T1, T2, T3, T4, T5> handler) =>
            connection.On(methodName, handler);
        public void On<T1, T2, T3, T4, T5, T6>(string methodName, Action<T1, T2, T3, T4, T5, T6> handler) =>
            connection.On(methodName, handler);
        public void On<T1, T2, T3, T4, T5, T6, T7>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7> handler) =>
            connection.On(methodName, handler);
        public void On<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7, T8> handler) =>
            connection.On(methodName, handler);
        #endregion

#elif UNITY_WEBGL

        public bool IsConnected => IsConnectedJs();

        #region Init JS
        [DllImport("__Internal")]
        private static extern void InitJs(string hubUrl, string optionsJson);

        public void Init(string url) => Init(url, null);

        public void Init(string url, SignalROptions options)
        {
            InitJs(url, options?.ToJson());
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

        #region IsConnected JS
        [DllImport("__Internal")]
        private static extern bool IsConnectedJs();
        #endregion

        #region Invoke JS
        [DllImport("__Internal")]
        private static extern void InvokeJs(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10);

        public void Invoke(string methodName, params object[] args)
        {
            var a = new string[10];
            for (int i = 0; i < args.Length && i < 10; i++)
                a[i] = args[i]?.ToString();
            InvokeJs(methodName, a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9]);
        }
        #endregion

        #region On JS
        private delegate void HandlerAction(params object[] args);
        private static readonly Dictionary<string, List<Type>> types = new();
        private static readonly Dictionary<string, HandlerAction> handlers = new();

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

        private static bool TryGetHandler(string methodName, out HandlerAction handler, out List<Type> type)
        {
            if (handlers.TryGetValue(methodName, out handler) && types.TryGetValue(methodName, out type))
                return true;
            Debug.LogWarning($"SignalR: No handler registered for method '{methodName}'");
            handler = null;
            type = null;
            return false;
        }

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        private static void HandlerCallback1(string methodName, string arg1)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]));
        }
        [MonoPInvokeCallback(typeof(Action<string, string, string>))]
        private static void HandlerCallback2(string methodName, string arg1, string arg2)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]));
        }
        [MonoPInvokeCallback(typeof(Action<string, string, string, string>))]
        private static void HandlerCallback3(string methodName, string arg1, string arg2, string arg3)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]));
        }
        [MonoPInvokeCallback(typeof(Action<string, string, string, string, string>))]
        private static void HandlerCallback4(string methodName, string arg1, string arg2, string arg3, string arg4)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]));
        }
        [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string>))]
        private static void HandlerCallback5(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]));
        }
        [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string, string>))]
        private static void HandlerCallback6(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]), Convert.ChangeType(arg6, type[5]));
        }
        [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string, string, string>))]
        private static void HandlerCallback7(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]), Convert.ChangeType(arg6, type[5]), Convert.ChangeType(arg7, type[6]));
        }
        [MonoPInvokeCallback(typeof(Action<string, string, string, string, string, string, string, string, string>))]
        private static void HandlerCallback8(string methodName, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8)
        {
            if (TryGetHandler(methodName, out var handler, out var type))
                handler.Invoke(Convert.ChangeType(arg1, type[0]), Convert.ChangeType(arg2, type[1]), Convert.ChangeType(arg3, type[2]), Convert.ChangeType(arg4, type[3]), Convert.ChangeType(arg5, type[4]), Convert.ChangeType(arg6, type[5]), Convert.ChangeType(arg7, type[6]), Convert.ChangeType(arg8, type[7]));
        }

        public void On<T1>(string methodName, Action<T1> handler)
        {
            types[methodName] = new List<Type> { typeof(T1) };
            handlers[methodName] = args => handler((T1)args[0]);
            OnJs(methodName, "1", HandlerCallback1);
        }
        public void On<T1, T2>(string methodName, Action<T1, T2> handler)
        {
            types[methodName] = new List<Type> { typeof(T1), typeof(T2) };
            handlers[methodName] = args => handler((T1)args[0], (T2)args[1]);
            OnJs(methodName, "2", HandlerCallback2);
        }
        public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler)
        {
            types[methodName] = new List<Type> { typeof(T1), typeof(T2), typeof(T3) };
            handlers[methodName] = args => handler((T1)args[0], (T2)args[1], (T3)args[2]);
            OnJs(methodName, "3", HandlerCallback3);
        }
        public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler)
        {
            types[methodName] = new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
            handlers[methodName] = args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]);
            OnJs(methodName, "4", HandlerCallback4);
        }
        public void On<T1, T2, T3, T4, T5>(string methodName, Action<T1, T2, T3, T4, T5> handler)
        {
            types[methodName] = new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
            handlers[methodName] = args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4]);
            OnJs(methodName, "5", HandlerCallback5);
        }
        public void On<T1, T2, T3, T4, T5, T6>(string methodName, Action<T1, T2, T3, T4, T5, T6> handler)
        {
            types[methodName] = new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
            handlers[methodName] = args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5]);
            OnJs(methodName, "6", HandlerCallback6);
        }
        public void On<T1, T2, T3, T4, T5, T6, T7>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7> handler)
        {
            types[methodName] = new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
            handlers[methodName] = args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6]);
            OnJs(methodName, "7", HandlerCallback7);
        }
        public void On<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, Action<T1, T2, T3, T4, T5, T6, T7, T8> handler)
        {
            types[methodName] = new List<Type> { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
            handlers[methodName] = args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7]);
            OnJs(methodName, "8", HandlerCallback8);
        }
        #endregion

        public void Dispose()
        {
            StopJs();
            types.Clear();
            handlers.Clear();
            if (instance == this)
                instance = null;
        }

#else
        public void Dispose() { }
#endif
    }
}
