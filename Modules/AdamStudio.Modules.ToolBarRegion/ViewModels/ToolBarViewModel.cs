using AdamController.WebApi.Client.v1.ResponseModel;
using AdamStudio.Core.Mvvm;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Regions;
using System;
using System.Windows;
using System.Windows.Threading;

namespace AdamStudio.Modules.ToolBarRegion.ViewModels
{
    public class ToolBarViewModel : RegionViewModelBase
    {
        private readonly ILogger<ToolBarViewModel> mLogger;
        private readonly ILogWriteEventAwareService mLogWriteEventAware;
        private readonly IPythonRemoteRunnerService mPythonRemoteRunner;
        private readonly ICultureProvider mCultureProvider;

        private bool mIsWarningStackOwerflowAlreadyShow;
        private string mFinishAppExecute;
        private string mWarningStackOwerflow1;
        private string mWarningStackOwerflow2;
        private string mWarningStackOwerflow3;

        public ToolBarViewModel(IRegionManager regionManager, ILogger<ToolBarViewModel> logger, ILogWriteEventAwareService logWriteEventAware, IPythonRemoteRunnerService pythonRemoteRunner, ICultureProvider cultureProvider) : base(regionManager)
        {
            mLogger = logger;
            mLogWriteEventAware = logWriteEventAware;
            mPythonRemoteRunner = pythonRemoteRunner;
            mCultureProvider = cultureProvider;

            //LoadResources();
            mLogger.LogTrace("Load ~");
        }

        #region Navigation

        public override void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            base.ConfirmNavigationRequest(navigationContext, continuationCallback);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Subscribe();
           //LoadResources();

            base.OnNavigatedTo(navigationContext);
        }

        public override void Destroy()
        {
            Unsubscribe();

            base.Destroy();
        }

        #endregion

        #region Public fields

        private string logs;
        public string ApplicationLogs
        {
            get => logs;
            set => SetProperty(ref logs, value);
        }

        private string resultText;
        public string ResultText
        {
            get => resultText;
            set
            {
                bool isNewValue = SetProperty(ref resultText, value);

                //if (isNewValue)
                    //CleanExecuteEditorDelegateCommand.RaiseCanExecuteChanged();
            }
        }

        private ExtendedCommandExecuteResult resultExecutionTime;
        public ExtendedCommandExecuteResult ResultExecutionTime
        {
            get => resultExecutionTime;
            set => SetProperty(ref resultExecutionTime, value);
        }

        private bool isPythonCodeExecute;
        public bool IsPythonCodeExecute
        {
            get => isPythonCodeExecute;
            set
            {
                bool isNewValue = SetProperty(ref isPythonCodeExecute, value);

                if (isNewValue)
                {
                    //OnPythonCodeExecuteStatusChange(IsPythonCodeExecute);
                    //RaiseDelegateCommandsCanExecuteChanged();
                }
            }
        }

        #endregion

        #region PrivateMethods

        private void UpdateResultText(string text, bool isFinishMessage = false)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (isFinishMessage)
                {
                    ResultText += "\n======================\n";
                    ResultText += $"<<{mFinishAppExecute}>>\n";
                }

                if (!isFinishMessage)
                {
                    if (ResultText?.Length > 500)
                    {
                        if (!mIsWarningStackOwerflowAlreadyShow)
                        {
                            string warningMessage = $"\n{mWarningStackOwerflow1}\n{mWarningStackOwerflow2}\n{mWarningStackOwerflow3}\n";
                            ResultText += warningMessage;
                            mIsWarningStackOwerflowAlreadyShow = true;
                        }

                        return;
                    }

                    ResultText += text;
                }
            }));
        }

        private void UpdateResultExecutionTimeText(ExtendedCommandExecuteResult executeResult)
        {

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ExtendedCommandExecuteResult fixResult = new()
                {
                    StandardOutput = executeResult.StandardOutput,
                    StandardError = executeResult.StandardError,

                    StartTime = executeResult.StartTime,
                    EndTime = executeResult.EndTime,
                    RunTime = executeResult.RunTime,

                    ExitCode = executeResult.ExitCode,

                    // The server always returns False
                    // Therefore, the success of completion is determined by the exit code
                    Succeesed = executeResult.ExitCode == 0
                };

                ResultExecutionTime = fixResult;
                IsPythonCodeExecute = false;
            }));
        }

        private void ClearResultText()
        {
            ResultText = string.Empty;
            ResultExecutionTime = null;
        }

        private void LoadResources()
        {
            mFinishAppExecute = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.FinishAppExecute");

            mWarningStackOwerflow1 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow1");
            mWarningStackOwerflow2 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow2");
            mWarningStackOwerflow3 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow3");
        }

        #endregion

        #region Events

        private void RaiseNewLogMessageWriteEvent(object sender, string message)
        {
            ApplicationLogs += $"{message}\n";
        }

        private void OnRaisePythonScriptExecuteStart(object sender)
        {
            IsPythonCodeExecute = true;
        }

        private void OnRaisePythonScriptExecuteFinish(object sender, ExtendedCommandExecuteResult remoteCommandExecuteResult)
        {
            if (remoteCommandExecuteResult == null)
                return;

            UpdateResultText("", true);
            UpdateResultExecutionTimeText(remoteCommandExecuteResult);
        }

        private void OnRaisePythonStandartOutput(object sender, string message)
        {
            UpdateResultText(message);
        }


        private void RaiseCurrentAppCultureLoadOrChangeEvent(object sender)
        {
            LoadResources();
           
        }

        #endregion

        #region Subscribes

        private void Subscribe()
        {
            mLogWriteEventAware.RaiseNewLogMessageWriteEvent += RaiseNewLogMessageWriteEvent;

            mPythonRemoteRunner.RaisePythonScriptExecuteStartEvent += OnRaisePythonScriptExecuteStart;
            mPythonRemoteRunner.RaisePythonStandartOutputEvent += OnRaisePythonStandartOutput;
            mPythonRemoteRunner.RaisePythonScriptExecuteFinishEvent += OnRaisePythonScriptExecuteFinish;

            mCultureProvider.RaiseCurrentAppCultureLoadOrChangeEvent += RaiseCurrentAppCultureLoadOrChangeEvent;
        }

        private void Unsubscribe()
        {
            mLogWriteEventAware.RaiseNewLogMessageWriteEvent -= RaiseNewLogMessageWriteEvent;

            mPythonRemoteRunner.RaisePythonScriptExecuteStartEvent -= OnRaisePythonScriptExecuteStart;
            mPythonRemoteRunner.RaisePythonStandartOutputEvent -= OnRaisePythonStandartOutput;
            mPythonRemoteRunner.RaisePythonScriptExecuteFinishEvent -= OnRaisePythonScriptExecuteFinish;

            mCultureProvider.RaiseCurrentAppCultureLoadOrChangeEvent -= RaiseCurrentAppCultureLoadOrChangeEvent;
        }

        #endregion
    }
}
