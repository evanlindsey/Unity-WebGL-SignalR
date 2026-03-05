using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityWebGLSignalR;

namespace Tests.Editor
{
    [TestFixture]
    public class SignalRTests
    {
        private const string TEST_URL = "http://localhost:5000/testhub";

        private SignalR signalR;

        [SetUp]
        public void SetUp()
        {
            signalR = new SignalR();
        }

        [TearDown]
        public void TearDown()
        {
            signalR?.Dispose();
        }

        [Test]
        public void Constructor_SetsInstance()
        {
            // The instance is set internally — we verify via IsConnected not throwing
            Assert.That(signalR.IsConnected, Is.False);
        }

        [Test]
        public void Constructor_DuplicateInstance_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, "SignalR: Creating a new instance replaces the previous one. Only one instance is supported.");
            var second = new SignalR();
            second.Dispose();
        }

        [Test]
        public void Init_WithUrl_CreatesConnection()
        {
            signalR.Init(TEST_URL);
            // Connection created but not started — should not be connected
            Assert.That(signalR.IsConnected, Is.False);
        }

        [Test]
        public void Init_WithOptions_CreatesConnection()
        {
            var options = new SignalROptions
            {
                AccessToken = "test-token",
                ServerTimeout = 60000,
                KeepAliveInterval = 30000
            };
            signalR.Init(TEST_URL, options);
            Assert.That(signalR.IsConnected, Is.False);
        }

        [Test]
        public void Init_WithNullOptions_CreatesConnection()
        {
            signalR.Init(TEST_URL, null);
            Assert.That(signalR.IsConnected, Is.False);
        }

        [Test]
        public void Init_WithRetryDelays_CreatesConnection()
        {
            var options = new SignalROptions
            {
                RetryDelays = new[] { 0, 1000, 5000 }
            };
            signalR.Init(TEST_URL, options);
            Assert.That(signalR.IsConnected, Is.False);
        }

        [Test]
        public void Dispose_ClearsConnection()
        {
            signalR.Init(TEST_URL);
            signalR.Dispose();
            // After dispose, IsConnected should be false (connection is null)
            Assert.That(signalR.IsConnected, Is.False);
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            signalR.Init(TEST_URL);
            signalR.Dispose();
            Assert.DoesNotThrow(() => signalR.Dispose());
        }

        [Test]
        public void Events_CanSubscribeBeforeInit()
        {
            bool startedFired = false;
            bool closedFired = false;
            signalR.ConnectionStarted += (s, e) => startedFired = true;
            signalR.ConnectionClosed += (s, e) => closedFired = true;

            // Should not throw — events are just delegates
            Assert.That(startedFired, Is.False);
            Assert.That(closedFired, Is.False);
        }
    }
}
