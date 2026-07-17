using System;
using AdamStudio.Core.Extensions;
using AdamStudio.Core.Model;
using Xunit;

namespace AdamStudio.Tests.Core
{
    /// <summary>
    /// Tests for the RFC 5424 Syslog message parser (SyslogParseMessageExtension).
    ///
    /// AdamStudio receives syslog messages from Adam-Servers over UDP and parses
    /// them for display in the status bar. A parser regression would either crash
    /// the UDP receive handler or show garbled output.
    ///
    /// RFC 5424 format:
    ///   &lt;PRIVAL&gt;VERSION TIMESTAMP HOSTNAME APPNAME PROCID MSGID STRUCTUREDDATA MSG
    ///
    /// Example:
    ///   &lt;165&gt;1 2024-01-15T12:34:56.789Z myhost myapp 1234 ID01 [exampleSDID@32473 iut="3"] Hello
    /// </summary>
    public class SyslogParseMessageExtensionTests
    {
        private const string cValidMessage =
            "<165>1 2024-01-15T12:34:56.789Z myhost myapp 1234 ID01 [exampleSDID@32473 iut=\"3\"] Hello, world!";

        [Fact]
        public void Parse_ValidMessage_ReturnsPrival()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal(165, result.Prival);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsVersion()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal(1, result.Version);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsHostName()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal("myhost", result.HostName);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsAppName()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal("myapp", result.AppName);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsProcId()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal("1234", result.ProcId);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsMessageId()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal("ID01", result.MessageId);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsStructuredData()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal("[exampleSDID@32473 iut=\"3\"]", result.StructuredData);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsMessage()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal("Hello, world!", result.Message);
        }

        [Fact]
        public void Parse_ValidMessage_ReturnsRawMessage()
        {
            SyslogMessageModel result = cValidMessage.Parse();
            Assert.Equal(cValidMessage, result.RawMessage);
        }

        [Fact]
        public void Parse_MinimalMessage_WithNilValues_ParsesSuccessfully()
        {
            // RFC 5424 allows nil-values (-) for optional fields.
            const string minimal = "<34>1 2024-01-15T12:34:56.789Z - - - - - Hello";

            SyslogMessageModel result = minimal.Parse();

            Assert.Equal(34, result.Prival);
            Assert.Equal("-", result.HostName);
            Assert.Equal("-", result.AppName);
            Assert.Equal("-", result.ProcId);
            Assert.Equal("-", result.MessageId);
            Assert.Equal("-", result.StructuredData);
            Assert.Equal("Hello", result.Message);
        }

        [Fact]
        public void Parse_MessageWithoutStructuredData_ParsesSuccessfully()
        {
            // Structured data can be "-" when no structured data is present.
            const string noSd = "<34>1 2024-01-15T12:34:56.789Z host app 123 MSGID - Message body";

            SyslogMessageModel result = noSd.Parse();

            Assert.Equal("-", result.StructuredData);
            Assert.Equal("Message body", result.Message);
        }

        [Fact]
        public void Parse_MessageWithoutMessageSection_ParsesSuccessfully()
        {
            // The MSG field is optional in RFC 5424.
            const string noMsg = "<34>1 2024-01-15T12:34:56.789Z host app 123 MSGID -";

            SyslogMessageModel result = noMsg.Parse();

            Assert.Equal("-", result.StructuredData);
            Assert.Equal(string.Empty, result.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Parse_NullOrWhitespace_ThrowsArgumentNullException(string? input)
        {
            // The parser guards against null/empty input explicitly.
            Assert.Throws<ArgumentNullException>(() => input!.Parse());
        }

        [Fact]
        public void Parse_InvalidFormat_ThrowsInvalidOperationException()
        {
            // A completely malformed string that does not match the RFC 5424
            // pattern should throw InvalidOperationException, not return null.
            const string garbage = "this is not a syslog message at all";

            Assert.Throws<InvalidOperationException>(() => garbage.Parse());
        }

        [Fact]
        public void Parse_PrivalOutOfRange_HighValue_Throws()
        {
            // PRIVAL is limited to 3 digits (1-999) by the regex \d{1,3}.
            // A 4-digit prival should fail to match.
            const string badPrival = "<1234>1 2024-01-15T12:34:56.789Z host app 123 MSGID - msg";

            Assert.Throws<InvalidOperationException>(() => badPrival.Parse());
        }

        [Fact]
        public void Parse_MissingClosingBracket_ThrowsInvalidOperationException()
        {
            const string badFormat = "165>1 2024-01-15T12:34:56.789Z host app 123 MSGID - msg";

            Assert.Throws<InvalidOperationException>(() => badFormat.Parse());
        }

        [Fact]
        public void Parse_HighPriorityEmergency_ParsesCorrectly()
        {
            // Priority 0 = emergency (most severe). Prival = facility*8 + severity.
            const string emergency = "<0>1 2024-01-15T12:34:56.789Z host app 123 MSGID - Emergency";

            SyslogMessageModel result = emergency.Parse();

            Assert.Equal(0, result.Prival);
            Assert.Equal("Emergency", result.Message);
        }

        [Fact]
        public void Parse_MaxPrival_ParsesCorrectly()
        {
            // Max 3-digit prival = 999.
            const string maxPrival = "<999>1 2024-01-15T12:34:56.789Z host app 123 MSGID - msg";

            SyslogMessageModel result = maxPrival.Parse();

            Assert.Equal(999, result.Prival);
        }

        [Fact]
        public void ToString_ContainsAllFields()
        {
            SyslogMessageModel model = new()
            {
                Prival = 165,
                Version = 1,
                TimeStamp = new DateTime(2024, 1, 15, 12, 34, 56, DateTimeKind.Utc),
                HostName = "myhost",
                AppName = "myapp",
                ProcId = "1234",
                MessageId = "ID01",
                StructuredData = "-",
                Message = "Hello"
            };

            string result = model.ToString();

            Assert.Contains("<165>", result);
            Assert.Contains("1", result);
            Assert.Contains("myhost", result);
            Assert.Contains("myapp", result);
            Assert.Contains("1234", result);
            Assert.Contains("ID01", result);
            Assert.Contains("Hello", result);
        }

        [Fact]
        public void ToString_WithEmptyMessage_OmitsTrailingSpace()
        {
            SyslogMessageModel model = new()
            {
                Prival = 34,
                Version = 1,
                TimeStamp = new DateTime(2024, 1, 15, 12, 34, 56, DateTimeKind.Utc),
                HostName = "host",
                AppName = "app",
                ProcId = "1",
                MessageId = "ID",
                StructuredData = "-",
                Message = ""
            };

            string result = model.ToString();

            // The ToString() should not append the message (and thus no leading
            // space) when Message is empty/whitespace.
            Assert.False(result.EndsWith(" ", StringComparison.Ordinal));
        }
    }
}
