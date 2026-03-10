var SignalRLib = {

    $vars: {
        connection: null,
        lastConnectionId: '',
        connectedCallback: null,
        disconnectedCallback: null,
        invokeCallback: function (args, callback) {
            var sig = 'v';
            var buffers = [];
            for (var i = 0; i < args.length; i++) {
                var message = args[i].toString();
                var bufferSize = lengthBytesUTF8(message) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(message, buffer, bufferSize);
                buffers.push(buffer);
                sig += 'i';
            }
            dynCall(sig, callback, buffers);
            for (var j = 0; j < buffers.length; j++) {
                _free(buffers[j]);
            }
        },
        // Maps TransportType enum values to signalR.HttpTransportType
        mapTransport: function (value) {
            switch (value) {
                case 1: return signalR.HttpTransportType.WebSockets;
                case 2: return signalR.HttpTransportType.ServerSentEvents;
                case 4: return signalR.HttpTransportType.LongPolling;
                default: return undefined;
            }
        },
        // Maps SignalRLogLevel enum to signalR.LogLevel
        // Must match NULL_SENTINEL in SignalR.cs
        NULL_SENTINEL: '\x01__null__',
        mapLogLevel: function (value) {
            switch (value) {
                case 0: return signalR.LogLevel.None;
                case 1: return signalR.LogLevel.Trace;
                case 2: return signalR.LogLevel.Debug;
                case 3: return signalR.LogLevel.Information;
                case 4: return signalR.LogLevel.Warning;
                case 5: return signalR.LogLevel.Error;
                case 6: return signalR.LogLevel.Critical;
                default: return undefined;
            }
        }
    },

    InitJs: function (url, optionsJson) {
        url = UTF8ToString(url);

        // Parse options from JSON (null/0 pointer means no options)
        var opts = {};
        if (optionsJson !== 0) {
            try {
                opts = JSON.parse(UTF8ToString(optionsJson));
            } catch (e) {
                console.error('SignalR: Failed to parse options JSON: ' + e.toString());
            }
        }

        // Build withUrl connection options
        var urlOptions = {};

        if (opts.accessToken) {
            var token = opts.accessToken;
            urlOptions.accessTokenFactory = function () { return token; };
        }
        if (opts.headers) {
            urlOptions.headers = opts.headers;
        }
        if (opts.withCredentials !== undefined) {
            urlOptions.withCredentials = opts.withCredentials;
        }
        if (opts.transport !== undefined) {
            var mapped = vars.mapTransport(opts.transport);
            if (mapped !== undefined) {
                urlOptions.transport = mapped;
            }
        }
        if (opts.skipNegotiation !== undefined) {
            urlOptions.skipNegotiation = opts.skipNegotiation;
        }
        if (opts.timeout !== undefined) {
            urlOptions.timeout = opts.timeout;
        }
        if (opts.logMessageContent !== undefined) {
            urlOptions.logMessageContent = opts.logMessageContent;
        }
        if (opts.logLevel !== undefined) {
            var level = vars.mapLogLevel(opts.logLevel);
            if (level !== undefined) {
                urlOptions.logger = level;
            }
        }

        // Build the connection
        var builder = new signalR.HubConnectionBuilder()
            .withUrl(url, urlOptions);

        // Configure automatic reconnect
        if (opts.retryDelays && opts.retryDelays.length > 0) {
            builder = builder.withAutomaticReconnect(opts.retryDelays);
        } else {
            builder = builder.withAutomaticReconnect();
        }

        // Configure server timeout
        if (opts.serverTimeout !== undefined) {
            builder = builder.withServerTimeout(opts.serverTimeout);
        }

        // Configure keep-alive interval
        if (opts.keepAliveInterval !== undefined) {
            builder = builder.withKeepAliveInterval(opts.keepAliveInterval);
        }

        vars.connection = builder.build();

        // Register event handlers before start() to avoid missing events
        vars.connection.onclose(function (err) {
            if (err) {
                console.error('Connection closed due to error: "' + err.toString() + '".');
            }
            vars.invokeCallback([vars.lastConnectionId], vars.disconnectedCallback);
        });
        vars.connection.onreconnecting(function (err) {
            console.log('Connection lost due to error: "' + (err ? err.toString() : 'unknown') + '". Reconnecting.');
        });
        vars.connection.onreconnected(function (connectionId) {
            console.log('Connection reestablished. Connected with connectionId: "' + connectionId + '".');
            vars.lastConnectionId = connectionId;
            vars.invokeCallback([vars.lastConnectionId], vars.connectedCallback);
        });
    },

    ConnectJs: function (connectedCallback, disconnectedCallback) {
        vars.connectedCallback = connectedCallback;
        vars.disconnectedCallback = disconnectedCallback;
        vars.connection.start()
            .then(function () {
                vars.lastConnectionId = vars.connection.connectionId;
                vars.invokeCallback([vars.lastConnectionId], vars.connectedCallback);
            }).catch(function (err) {
                return console.error(err.toString());
            });
    },

    StopJs: function () {
        if (vars.connection) {
            vars.connection.stop()
                .catch(function (err) {
                    return console.error(err.toString());
                });
        }
    },

    IsConnectedJs: function () {
        return vars.connection !== null &&
            vars.connection.state === signalR.HubConnectionState.Connected;
    },

    InvokeJs: function (methodName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) {
        methodName = UTF8ToString(methodName);
        var rawArgs = [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10];
        var args = [];
        for (var i = 0; i < rawArgs.length; i++) {
            if (rawArgs[i] !== 0) {
                var val = UTF8ToString(rawArgs[i]);
                args.push(val === vars.NULL_SENTINEL ? null : val);
            } else {
                break;
            }
        }
        vars.connection.invoke.apply(vars.connection, [methodName].concat(args))
            .catch(function (err) {
                console.error(err.toString());
            });
    },

    OnJs: function (methodName, argCount, callback) {
        methodName = UTF8ToString(methodName);
        argCount = Number.parseInt(UTF8ToString(argCount));
        var storedCallback = callback;
        vars.connection.on(methodName, function () {
            var args = [methodName];
            for (var i = 0; i < arguments.length; i++) {
                args.push(arguments[i].toString());
            }
            vars.invokeCallback(args, storedCallback);
        });
    },

};

autoAddDeps(SignalRLib, '$vars');
mergeInto(LibraryManager.library, SignalRLib);
