using AdamStudio.Services.Interfaces;
using System;
using System.Text;

namespace AdamStudio.Services
{
    public class LogWriteEventAwareService : ILogWriteEventAwareService
    {
        #region Events

        public event NewLogMessageWriteEventHandler RaiseNewLogMessageWriteEvent;

        #endregion

        private readonly StringBuilder mLogEventBuffer = new();

        public LogWriteEventAwareService() { }

        
        public void Dispose()
        {
            mLogEventBuffer?.Clear();
        }

        public void WriteToBuffer(string formattedLogMessage)
        {
            var message = formattedLogMessage.TrimEnd(Environment.NewLine.ToCharArray());
            mLogEventBuffer.AppendLine(message);

            OnRaiseNewLogMessageWriteEvent(message);
        }

        public string GetLogMessages()
        {
            return mLogEventBuffer.ToString();
        }

        public void ClearLogMessages()
        {
            mLogEventBuffer?.Clear();
        }

        protected virtual void OnRaiseNewLogMessageWriteEvent(string message)
        {
            NewLogMessageWriteEventHandler raiseEvent = RaiseNewLogMessageWriteEvent;
            raiseEvent?.Invoke(this, message);
        }
    }
}
