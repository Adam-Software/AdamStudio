using AdamStudio.Services.Interfaces;
using AdamStudio.Services.UdpClientServiceDependency;
using AdamController.WebApi.Client.v1.ResponseModel;
using AdamController.WebApi.Client.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace AdamStudio.Services
{
    public partial class PythonRemoteRunnerService : IPythonRemoteRunnerService
    {
        #region Events

        public event PythonStandartOutputEventHandler RaisePythonStandartOutputEvent;
        public event PythonScriptExecuteStartEventHandler RaisePythonScriptExecuteStartEvent;
        public event PythonScriptExecuteFinishEventHandler RaisePythonScriptExecuteFinishEvent;

        #endregion

        #region Services

        private readonly IUdpClientService mUdpClientService;

        #endregion

        #region Const

        private const string cStartMessage = "start";
        private const string cErrorMessage = "error";
        private const string cFinishMessage = "finish";
        private const string cPattern = $"{cStartMessage}|{cErrorMessage}|{cFinishMessage}";

        #endregion

        #region var

        private readonly Regex mRegex;

        [GeneratedRegex($"{cPattern}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.NonBacktracking)]
        private static partial Regex MyRegex();

        #endregion

        #region ~

        public PythonRemoteRunnerService(IServiceProvider serviceProvider) 
        {
            mUdpClientService = serviceProvider.GetService<IUdpClientService>(); ;

            Subscribe();

            mRegex = MyRegex();
        }


        #endregion

        #region Public method

        public void Dispose()
        {
            Unsubscribe();
        }

        #endregion

        #region Private methods

        private void ParseEvents(Match match, string message)
        {
            switch (match.Value)
            {
                case cStartMessage:
                    OnRaisePythonScriptExecuteStartEvent();
                    break;

                case cErrorMessage:
                    break;

                case cFinishMessage:
                    {
                        string cleanMessage = message.Remove(0, 6);

                        ExtendedCommandExecuteResult executeResult = cleanMessage.ToExtendedCommandResult();
                        OnRaisePythonScriptExecuteFinishEvent(executeResult);
                        break;
                    }
            }
        }

        private void ParseMessage(string message)
        {
            MatchCollection matches = mRegex.Matches(message);

            if(matches.Count > 0)
            {
                foreach(Match match in matches.Cast<Match>())
                {
                    ParseEvents(match, message);
                }

                return;
            }
            
            OnRaisePythonStandartOutputEvent($"{message}\n");
            
        }

        #endregion

        #region Subscribses

        private void Subscribe()
        {
            mUdpClientService.RaiseUdpClientMessageEnqueueEvent += RaiseUdpClientMessageEnqueueEvent;
        }

        private void Unsubscribe() 
        {
            mUdpClientService.RaiseUdpClientMessageEnqueueEvent -= RaiseUdpClientMessageEnqueueEvent;
        }

        #endregion

        #region Event methods

        private void RaiseUdpClientMessageEnqueueEvent(object sender, ReceivedData data)
        {
            ParseMessage(data.ToString());
        }

        #endregion

        #region OnRaise events

        protected virtual void OnRaisePythonStandartOutputEvent(string message)
        {
            PythonStandartOutputEventHandler raiseEvent = RaisePythonStandartOutputEvent;
            raiseEvent?.Invoke(this, message);  
        }

        protected virtual void OnRaisePythonScriptExecuteStartEvent()
        {
            PythonScriptExecuteStartEventHandler raiseEvent = RaisePythonScriptExecuteStartEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaisePythonScriptExecuteFinishEvent(ExtendedCommandExecuteResult remoteCommandExecuteResult)
        {
            PythonScriptExecuteFinishEventHandler raiseEvent = RaisePythonScriptExecuteFinishEvent;
            raiseEvent?.Invoke(this, remoteCommandExecuteResult);
        }

        #endregion
    }
}
