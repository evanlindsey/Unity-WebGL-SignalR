using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityWebGLSignalR;

namespace Tests.PlayMode
{
    /// <summary>
    /// Integration tests that require a running server (make server).
    /// Run these from the Unity Test Runner with the server running on localhost:5000.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class SignalRIntegrationTests
    {
        private const string HUB_URL = "http://localhost:5000/mainhub";
        private const string METHOD_SEND_CALLER = "SendPayloadCaller";
        private const string HANDLER_RECEIVE_CALLER = "ReceivePayloadCaller";
        private const float TIMEOUT = 10f;

        private SignalR signalR;

        [SetUp]
        public void SetUp()
        {
            signalR = new SignalR();
        }

        [TearDown]
        public void TearDown()
        {
            signalR?.Stop();
            signalR?.Dispose();
        }

        [UnityTest]
        public IEnumerator Connect_FiresConnectionStartedEvent()
        {
            string receivedConnectionId = null;
            signalR.Init(HUB_URL);
            signalR.ConnectionStarted += (sender, e) =>
            {
                receivedConnectionId = e.ConnectionId;
            };

            signalR.Connect();

            float elapsed = 0f;
            while (receivedConnectionId == null && elapsed < TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(receivedConnectionId, Is.Not.Null, "ConnectionStarted event did not fire within timeout");
            Assert.That(receivedConnectionId, Is.Not.Empty);
        }

        [UnityTest]
        public IEnumerator IsConnected_ReturnsTrueAfterConnect()
        {
            bool connected = false;
            signalR.Init(HUB_URL);
            signalR.ConnectionStarted += (sender, e) => connected = true;

            signalR.Connect();

            float elapsed = 0f;
            while (!connected && elapsed < TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(signalR.IsConnected, Is.True);
        }

        [UnityTest]
        public IEnumerator Stop_FiresConnectionClosedEvent()
        {
            bool connected = false;
            bool disconnected = false;
            signalR.Init(HUB_URL);
            signalR.ConnectionStarted += (sender, e) => connected = true;
            signalR.ConnectionClosed += (sender, e) => disconnected = true;

            signalR.Connect();

            float elapsed = 0f;
            while (!connected && elapsed < TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            Assert.That(connected, Is.True, "Failed to connect");

            signalR.Stop();

            elapsed = 0f;
            while (!disconnected && elapsed < TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(disconnected, Is.True, "ConnectionClosed event did not fire");
            Assert.That(signalR.IsConnected, Is.False);
        }

        [UnityTest]
        public IEnumerator Invoke_AndOn_SendAndReceivePayload()
        {
            bool connected = false;
            string receivedPayload = null;

            signalR.Init(HUB_URL);

            signalR.On(HANDLER_RECEIVE_CALLER, (string payload) =>
            {
                receivedPayload = payload;
            });

            signalR.ConnectionStarted += (sender, e) =>
            {
                connected = true;
                signalR.Invoke(METHOD_SEND_CALLER, "{\"message\":\"test\"}");
            };

            signalR.Connect();

            float elapsed = 0f;
            while (!connected && elapsed < TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            Assert.That(connected, Is.True, "Failed to connect");

            elapsed = 0f;
            while (receivedPayload == null && elapsed < TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(receivedPayload, Is.Not.Null, "Did not receive payload within timeout");
            Assert.That(receivedPayload, Does.Contain("test"));
        }

        [UnityTest]
        public IEnumerator Connect_WithOptions_Succeeds()
        {
            bool connected = false;
            var options = new SignalROptions
            {
                ServerTimeout = 60000,
                KeepAliveInterval = 30000
            };

            signalR.Init(HUB_URL, options);
            signalR.ConnectionStarted += (sender, e) => connected = true;

            signalR.Connect();

            float elapsed = 0f;
            while (!connected && elapsed < TIMEOUT)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(connected, Is.True, "Failed to connect with options");
            Assert.That(signalR.IsConnected, Is.True);
        }
    }
}
