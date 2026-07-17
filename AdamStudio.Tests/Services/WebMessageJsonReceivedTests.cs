using System;
using System.Text.Json;
using AdamStudio.Services.WebViewProviderDependency;
using Xunit;

namespace AdamStudio.Tests.Services
{
    /// <summary>
    /// Tests for WebMessageJsonReceived DTO.
    ///
    /// This DTO crosses the WebView2 <-> C# boundary. The JavaScript side
    /// (Google Blockly) sends JSON with "action" and "data" fields, which
    /// C# deserializes into this type. A serialization contract regression
    /// would break all Blockly communication silently.
    ///
    /// The property names must stay camelCase (JsonPropertyName) even though
    /// C# convention is PascalCase — the JS side sends lowercase keys.
    /// </summary>
    public class WebMessageJsonReceivedTests
    {
        private static readonly JsonSerializerOptions mJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [Fact]
        public void Deserialize_ValidJson_ReturnsActionAndData()
        {
            const string json = """{"action":"run_python","data":"print('hello')"}""";

            WebMessageJsonReceived? result = JsonSerializer.Deserialize<WebMessageJsonReceived>(json, mJsonOptions);

            Assert.NotNull(result);
            Assert.Equal("run_python", result!.Action);
            Assert.Equal("print('hello')", result.Data);
        }

        [Fact]
        public void Deserialize_CamelCaseKeys_MapsToPascalCaseProperties()
        {
            // Verify the JsonPropertyName attributes are present and correct.
            // The JS side sends "action" / "data" (camelCase), C# properties
            // are Action / Data (PascalCase).
            const string json = """{"action":"test","data":"payload"}""";

            WebMessageJsonReceived? result = JsonSerializer.Deserialize<WebMessageJsonReceived>(json, mJsonOptions);

            Assert.NotNull(result);
            Assert.Equal("test", result!.Action);
            Assert.Equal("payload", result.Data);
        }

        [Fact]
        public void Deserialize_MissingDataField_ReturnsNullData()
        {
            // If JS sends only "action" without "data", the Data property
            // should be null (not throw).
            const string json = """{"action":"no_data"}""";

            WebMessageJsonReceived? result = JsonSerializer.Deserialize<WebMessageJsonReceived>(json, mJsonOptions);

            Assert.NotNull(result);
            Assert.Equal("no_data", result!.Action);
            Assert.Null(result.Data);
        }

        [Fact]
        public void Deserialize_EmptyValues_ParsesSuccessfully()
        {
            const string json = """{"action":"","data":""}""";

            WebMessageJsonReceived? result = JsonSerializer.Deserialize<WebMessageJsonReceived>(json, mJsonOptions);

            Assert.NotNull(result);
            Assert.Equal(string.Empty, result!.Action);
            Assert.Equal(string.Empty, result.Data);
        }

        [Fact]
        public void Serialize_RoundTrip_PreservesValues()
        {
            WebMessageJsonReceived original = new()
            {
                Action = "execute",
                Data = "x = 42"
            };

            string json = JsonSerializer.Serialize(original);
            WebMessageJsonReceived? roundTripped = JsonSerializer.Deserialize<WebMessageJsonReceived>(json, mJsonOptions);

            Assert.NotNull(roundTripped);
            Assert.Equal(original.Action, roundTripped!.Action);
            Assert.Equal(original.Data, roundTripped.Data);
        }

        [Fact]
        public void Serialize_UsesCamelCasePropertyNames()
        {
            // Verify the serialized JSON uses "action" and "data" (camelCase)
            // to match the JS contract. This is enforced by JsonPropertyName.
            WebMessageJsonReceived message = new()
            {
                Action = "test_action",
                Data = "test_data"
            };

            string json = JsonSerializer.Serialize(message);

            Assert.Contains("\"action\":\"test_action\"", json);
            Assert.Contains("\"data\":\"test_data\"", json);
        }

        [Fact]
        public void Deserialize_InvalidJson_ThrowsJsonException()
        {
            // Malformed JSON should fail fast, not return null silently.
            const string badJson = """{"action":}""";

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<WebMessageJsonReceived>(badJson, mJsonOptions));
        }

        [Fact]
        public void IsEventArgs_SubclassOfEventArgs()
        {
            // WebMessageJsonReceived extends EventArgs so it can be passed
            // directly to event handlers in the WebView communication layer.
            WebMessageJsonReceived message = new();
            Assert.IsAssignableFrom<EventArgs>(message);
        }
    }
}
