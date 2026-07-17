using AdamController.WebApi.Client.v1.ResponseModel;
using System;

namespace AdamStudio.Services.Interfaces
{

    public delegate void PythonStandartOutputEventHandler(object sender, string message);
    public delegate void PythonScriptExecuteStartEventHandler(object sender);
    public delegate void PythonScriptExecuteFinishEventHandler(object sender, ExtendedCommandExecuteResult remoteCommandExecuteResult);

    public interface IPythonRemoteRunnerService : IDisposable
    {

        public event PythonStandartOutputEventHandler RaisePythonStandartOutputEvent;
        public event PythonScriptExecuteStartEventHandler RaisePythonScriptExecuteStartEvent;
        public event PythonScriptExecuteFinishEventHandler RaisePythonScriptExecuteFinishEvent;

    }
}
