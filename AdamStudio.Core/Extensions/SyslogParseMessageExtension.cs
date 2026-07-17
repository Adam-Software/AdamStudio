using AdamStudio.Core.Model;
using System;
using System.Text.RegularExpressions;

namespace AdamStudio.Core.Extensions
{
    public static class SyslogParseMessageExtension
    {
        private static readonly string mSyslogMsgHeaderPattern = @"\<(?<PRIVAL>\d{1,3})\>(?<VERSION>[1-9]{0,2}) (?<TIMESTAMP>(\S|\w)+) (?<HOSTNAME>-|(\S|\w){1,255}) (?<APPNAME>-|(\S|\w){1,48}) (?<PROCID>-|(\S|\w){1,128}) (?<MSGID>-|(\S|\w){1,32})";
        private static readonly string mSyslogMsgStructuredDataPattern = @"(?<STRUCTUREDDATA>-|\[[^\[\=\x22\]\x20]{1,32}( ([^\[\=\x22\]\x20]{1,32}=\x22.+\x22))?\])";
        private static readonly string mSyslogMsgMessagePattern = @"( (?<MESSAGE>.+))?";
        private static readonly Regex mExpression = new($@"^{mSyslogMsgHeaderPattern} {mSyslogMsgStructuredDataPattern}{mSyslogMsgMessagePattern}$", RegexOptions.None, new TimeSpan(0, 0, 5));

        /// <summary>
        /// Parses a Syslog message in RFC 5424 format. 
        /// </summary>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static SyslogMessageModel Parse(this string rawMessage)
        {
            if (string.IsNullOrWhiteSpace(rawMessage)) 
            { throw new ArgumentNullException(nameof(rawMessage)); }

            var match = mExpression.Match(rawMessage);
            if (match.Success)
            {
                return new SyslogMessageModel
                {
                    Prival = Convert.ToInt32(match.Groups["PRIVAL"].Value),
                    Version = Convert.ToInt32(match.Groups["VERSION"].Value),
                    TimeStamp = Convert.ToDateTime(match.Groups["TIMESTAMP"].Value),
                    HostName = match.Groups["HOSTNAME"].Value,
                    AppName = match.Groups["APPNAME"].Value,
                    ProcId = match.Groups["PROCID"].Value,
                    MessageId = match.Groups["MSGID"].Value,
                    StructuredData = match.Groups["STRUCTUREDDATA"].Value,
                    Message = match.Groups["MESSAGE"].Value,
                    RawMessage = rawMessage
                };
            }
            else
            {
                throw new InvalidOperationException("Invalid message.");
            }
        }
    }
}
