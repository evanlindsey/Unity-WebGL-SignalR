using System.Collections.Generic;
using NUnit.Framework;
using UnityWebGLSignalR;

namespace Tests.Editor
{
    [TestFixture]
    public class SignalROptionsTests
    {
        [Test]
        public void ToJson_EmptyOptions_ReturnsEmptyObject()
        {
            var options = new SignalROptions();
            Assert.That(options.ToJson(), Is.EqualTo("{}"));
        }

        [Test]
        public void ToJson_AccessToken_ProducesCorrectJson()
        {
            var options = new SignalROptions { AccessToken = "my-token-123" };
            var json = options.ToJson();
            Assert.That(json, Does.Contain("\"accessToken\":\"my-token-123\""));
        }

        [Test]
        public void ToJson_AccessToken_EscapesSpecialCharacters()
        {
            var options = new SignalROptions { AccessToken = "token\"with\\quotes" };
            var json = options.ToJson();
            Assert.That(json, Does.Contain("\"accessToken\":\"token\\\"with\\\\quotes\""));
        }

        [Test]
        public void ToJson_Headers_ProducesCorrectJson()
        {
            var options = new SignalROptions
            {
                Headers = new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer abc",
                    ["X-Custom"] = "value"
                }
            };
            var json = options.ToJson();
            Assert.That(json, Does.Contain("\"headers\":{"));
            Assert.That(json, Does.Contain("\"Authorization\":\"Bearer abc\""));
            Assert.That(json, Does.Contain("\"X-Custom\":\"value\""));
        }

        [Test]
        public void ToJson_Headers_EscapesSpecialCharacters()
        {
            var options = new SignalROptions
            {
                Headers = new Dictionary<string, string>
                {
                    ["Key\"Test"] = "Val\"ue"
                }
            };
            var json = options.ToJson();
            Assert.That(json, Does.Contain("\"Key\\\"Test\":\"Val\\\"ue\""));
        }

        [Test]
        public void ToJson_WithCredentials_ProducesCorrectJson()
        {
            var optionsTrue = new SignalROptions { WithCredentials = true };
            Assert.That(optionsTrue.ToJson(), Does.Contain("\"withCredentials\":true"));

            var optionsFalse = new SignalROptions { WithCredentials = false };
            Assert.That(optionsFalse.ToJson(), Does.Contain("\"withCredentials\":false"));
        }

        [Test]
        public void ToJson_Transport_ProducesCorrectEnumValue()
        {
            var options = new SignalROptions { Transport = TransportType.WebSockets };
            Assert.That(options.ToJson(), Does.Contain("\"transport\":1"));

            options.Transport = TransportType.ServerSentEvents;
            Assert.That(options.ToJson(), Does.Contain("\"transport\":2"));

            options.Transport = TransportType.LongPolling;
            Assert.That(options.ToJson(), Does.Contain("\"transport\":4"));
        }

        [Test]
        public void ToJson_TransportAll_IsIncluded()
        {
            // TransportType.All (0) means use default, should still be included
            var options = new SignalROptions { Transport = TransportType.All };
            Assert.That(options.ToJson(), Does.Contain("\"transport\":0"));
        }

        [Test]
        public void ToJson_SkipNegotiation_ProducesCorrectJson()
        {
            var options = new SignalROptions { SkipNegotiation = true };
            Assert.That(options.ToJson(), Does.Contain("\"skipNegotiation\":true"));
        }

        [Test]
        public void ToJson_HttpTimeout_ProducesCorrectJson()
        {
            var options = new SignalROptions { HttpTimeout = 5000 };
            Assert.That(options.ToJson(), Does.Contain("\"timeout\":5000"));
        }

        [Test]
        public void ToJson_ServerTimeout_ProducesCorrectJson()
        {
            var options = new SignalROptions { ServerTimeout = 60000 };
            Assert.That(options.ToJson(), Does.Contain("\"serverTimeout\":60000"));
        }

        [Test]
        public void ToJson_KeepAliveInterval_ProducesCorrectJson()
        {
            var options = new SignalROptions { KeepAliveInterval = 30000 };
            Assert.That(options.ToJson(), Does.Contain("\"keepAliveInterval\":30000"));
        }

        [Test]
        public void ToJson_LogMessageContent_ProducesCorrectJson()
        {
            var options = new SignalROptions { LogMessageContent = true };
            Assert.That(options.ToJson(), Does.Contain("\"logMessageContent\":true"));
        }

        [Test]
        public void ToJson_LogLevel_ProducesCorrectEnumValue()
        {
            var options = new SignalROptions { LogLevel = SignalRLogLevel.Warning };
            Assert.That(options.ToJson(), Does.Contain("\"logLevel\":4"));

            options.LogLevel = SignalRLogLevel.Trace;
            Assert.That(options.ToJson(), Does.Contain("\"logLevel\":1"));
        }

        [Test]
        public void ToJson_RetryDelays_ProducesCorrectArray()
        {
            var options = new SignalROptions { RetryDelays = new[] { 0, 1000, 5000, 30000 } };
            Assert.That(options.ToJson(), Does.Contain("\"retryDelays\":[0,1000,5000,30000]"));
        }

        [Test]
        public void ToJson_EmptyRetryDelays_IsOmitted()
        {
            var options = new SignalROptions { RetryDelays = new int[0] };
            Assert.That(options.ToJson(), Is.EqualTo("{}"));
        }

        [Test]
        public void ToJson_AllOptions_ProducesValidJson()
        {
            var options = new SignalROptions
            {
                AccessToken = "token",
                Headers = new Dictionary<string, string> { ["H1"] = "V1" },
                WithCredentials = false,
                Transport = TransportType.WebSockets,
                SkipNegotiation = true,
                HttpTimeout = 5000,
                ServerTimeout = 60000,
                KeepAliveInterval = 30000,
                LogMessageContent = true,
                LogLevel = SignalRLogLevel.Debug,
                RetryDelays = new[] { 0, 2000 }
            };

            var json = options.ToJson();

            // Should be parseable (basic structure check)
            Assert.That(json, Does.StartWith("{"));
            Assert.That(json, Does.EndWith("}"));

            // All fields present
            Assert.That(json, Does.Contain("\"accessToken\":\"token\""));
            Assert.That(json, Does.Contain("\"headers\":{\"H1\":\"V1\"}"));
            Assert.That(json, Does.Contain("\"withCredentials\":false"));
            Assert.That(json, Does.Contain("\"transport\":1"));
            Assert.That(json, Does.Contain("\"skipNegotiation\":true"));
            Assert.That(json, Does.Contain("\"timeout\":5000"));
            Assert.That(json, Does.Contain("\"serverTimeout\":60000"));
            Assert.That(json, Does.Contain("\"keepAliveInterval\":30000"));
            Assert.That(json, Does.Contain("\"logMessageContent\":true"));
            Assert.That(json, Does.Contain("\"logLevel\":2"));
            Assert.That(json, Does.Contain("\"retryDelays\":[0,2000]"));
        }

        [Test]
        public void TransportType_EnumValues_MatchSignalRSpec()
        {
            Assert.That((int)TransportType.All, Is.EqualTo(0));
            Assert.That((int)TransportType.WebSockets, Is.EqualTo(1));
            Assert.That((int)TransportType.ServerSentEvents, Is.EqualTo(2));
            Assert.That((int)TransportType.LongPolling, Is.EqualTo(4));
        }

        [Test]
        public void SignalRLogLevel_EnumValues_AreSequential()
        {
            Assert.That((int)SignalRLogLevel.None, Is.EqualTo(0));
            Assert.That((int)SignalRLogLevel.Trace, Is.EqualTo(1));
            Assert.That((int)SignalRLogLevel.Debug, Is.EqualTo(2));
            Assert.That((int)SignalRLogLevel.Information, Is.EqualTo(3));
            Assert.That((int)SignalRLogLevel.Warning, Is.EqualTo(4));
            Assert.That((int)SignalRLogLevel.Error, Is.EqualTo(5));
            Assert.That((int)SignalRLogLevel.Critical, Is.EqualTo(6));
        }

        [Test]
        public void ToJson_NullAccessToken_IsOmitted()
        {
            var options = new SignalROptions { AccessToken = null };
            Assert.That(options.ToJson(), Is.EqualTo("{}"));
        }

        [Test]
        public void ToJson_NullHeaders_IsOmitted()
        {
            var options = new SignalROptions { Headers = null };
            Assert.That(options.ToJson(), Is.EqualTo("{}"));
        }

        [Test]
        public void ToJson_EmptyHeaders_IsOmitted()
        {
            var options = new SignalROptions { Headers = new Dictionary<string, string>() };
            Assert.That(options.ToJson(), Is.EqualTo("{}"));
        }
    }
}
