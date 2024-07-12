using System;

namespace AdamStudio.Services.Interfaces
{
    #region Delegates

    public delegate void NewLogMessageWriteEventHandler(object sender, string message);

    #endregion

    public interface ILogWriteEventAwareService : IDisposable
    {
        #region Events

        public event NewLogMessageWriteEventHandler RaiseNewLogMessageWriteEvent;

        #endregion

        public void WriteToBuffer(string formattedLogMessage);
    }
}
