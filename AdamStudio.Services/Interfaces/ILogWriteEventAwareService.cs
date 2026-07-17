using System;

namespace AdamStudio.Services.Interfaces
{

    public delegate void NewLogMessageWriteEventHandler(object sender, string message);

    public interface ILogWriteEventAwareService : IDisposable
    {

        public event NewLogMessageWriteEventHandler RaiseNewLogMessageWriteEvent;

        public void WriteToBuffer(string formattedLogMessage);
    }
}
