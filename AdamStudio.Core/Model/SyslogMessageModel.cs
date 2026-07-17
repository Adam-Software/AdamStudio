using System;
using System.Text;

namespace AdamStudio.Core.Model
{
    public class SyslogMessageModel
    {
        public int Prival { get;  set; }
        public int Version { get; set; }
        public DateTime TimeStamp { get; set; }
        public string HostName { get; set; }
        public string AppName { get; set; }
        public string ProcId { get; set; }
        public string MessageId { get; set; }
        public string StructuredData { get; set; }
        public string Message { get;  set; }
        public string RawMessage { get; set; }

        public override string ToString()
        {
            var message = new StringBuilder($@"<{Prival:###}>{Version:##} {TimeStamp:yyyy-MM-ddTHH:mm:ss.fffK} {HostName} {AppName} {ProcId} {MessageId} {StructuredData}");

            if (!string.IsNullOrWhiteSpace(Message))
            {
                message.Append($" {Message}");
            }

            return message.ToString();
        }
    }
}
