using AdamController.WebApi.Client.v1.ResponseModel;
using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Core.Extensions;
using AdamStudio.Core.Model;
using AdamStudio.Core.Mvvm;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Navigation.Regions;
using System;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace AdamStudio.Modules.ToolBarRegion.ViewModels
{
    public class ToolBarViewModel : RegionViewModelBase
    {

        public DelegateCommand CleanExecuteEditorDelegateCommand { get; }

        private readonly ILogger<ToolBarViewModel> mLogger;
        private readonly ILogWriteEventAwareService mLogWriteEventAware;
        private readonly IPythonRemoteRunnerService mPythonRemoteRunner;
        private readonly ICultureProvider mCultureProvider;
        private readonly ICommunicationProviderService mCommunicationProviderService;
        private readonly IWebApiService mWebApiService;
        private readonly IFlyoutStateChecker mFlyoutStateChecker;
        private readonly ITcpPythonStreamServerService mTcpPythonStreamServerService;
       

        private bool mIsWarningStackOwerflowAlreadyShow;
        private string mFinishAppExecute;
        private string mWarningStackOwerflow1;
        private string mWarningStackOwerflow2;
        private string mWarningStackOwerflow3;

        public ToolBarViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            mLogger = serviceProvider.GetService<ILogger<ToolBarViewModel>>(); 
            mLogWriteEventAware = serviceProvider.GetService<ILogWriteEventAwareService>();
            mPythonRemoteRunner = serviceProvider.GetService<IPythonRemoteRunnerService>(); 
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            mCommunicationProviderService = serviceProvider.GetService<ICommunicationProviderService>();
            mWebApiService = serviceProvider.GetService<IWebApiService>();
            mFlyoutStateChecker = serviceProvider.GetService<IFlyoutStateChecker>();
            mTcpPythonStreamServerService = serviceProvider.GetService<ITcpPythonStreamServerService>();
            
            CleanExecuteEditorDelegateCommand = new DelegateCommand(CleanExecuteEditor, CleanExecuteEditorCanExecute);
            mLogger.LogTrace("Load ~");
        }

        private void CleanExecuteEditor()
        {
            ResultText = string.Empty;
            ResultExecutionTime = null;
        }

        private bool CleanExecuteEditorCanExecute()
        {
            bool isResultNotEmpty = ResultText?.Length > 0;
            return isResultNotEmpty;
        }

        public override void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            base.ConfirmNavigationRequest(navigationContext, continuationCallback);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Subscribe();

            base.OnNavigatedTo(navigationContext);
        }

        public override void Destroy()
        {
            Unsubscribe();

            base.Destroy();
        }

        private string applicationLogs;
        public string ApplicationLogs
        {
            get => applicationLogs;
            set => SetProperty(ref applicationLogs, value);
        }

        private string compilerLogs;
        public string CompilerLogs
        {
            get => compilerLogs;
            set => SetProperty(ref compilerLogs, value);
        }

        private string resultText;
        public string ResultText
        {
            get => resultText;
            set
            {
                bool isNewValue = SetProperty(ref resultText, value);

                if (isNewValue)
                    CleanExecuteEditorDelegateCommand.RaiseCanExecuteChanged();
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
            set => SetProperty(ref isPythonCodeExecute, value);
        }

        private bool mResultButtonIsChecked;
        public bool ResultButtonIsChecked
        {
            get => mResultButtonIsChecked; 
            set => SetProperty(ref mResultButtonIsChecked, value); 
        }

        private bool mLogsButtonIsChecked;
        public bool LogsButtonIsChecked
        {
            get => mLogsButtonIsChecked; 
            set => SetProperty(ref mLogsButtonIsChecked, value);
        }

        private void UpdateResultText(string text, bool isFinishMessage = false)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
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
            }));
        }

        private void ClearResults()
        {
            ResultText = string.Empty;
            ResultExecutionTime = null;
        }

        private string mPythonVersion;
        public string PythonVersion
        {
            get => mPythonVersion;
            set => SetProperty(ref mPythonVersion, value);
        }

        private string mPythonBinPath;
        public string PythonBinPath
        {
            get => mPythonBinPath;
            set => SetProperty(ref mPythonBinPath, value);
        }

        private string mPythonWorkDir;
        public string PythonWorkDir
        {
            get => mPythonWorkDir;
            set => SetProperty(ref mPythonWorkDir, value);
        }

        private void UpdatePythonInfo(string pythonVersion = null, string pythonBinPath = null, string pythonWorkDir = null)
        {
            PythonVersion = pythonVersion;
            PythonBinPath = pythonBinPath;
            PythonWorkDir = pythonWorkDir;
        }

        private void LoadResources()
        {
            mFinishAppExecute = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.FinishAppExecute");

            mWarningStackOwerflow1 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow1");
            mWarningStackOwerflow2 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow2");
            mWarningStackOwerflow3 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow3");
        }

        private void RaiseDelegateCommandsCanExecuteChanged()
        {
            CleanExecuteEditorDelegateCommand.RaiseCanExecuteChanged();
        }

        private void RaiseNewLogMessageWriteEvent(object sender, string message)
        {
            ApplicationLogs += $"{message}\n";
        }

        private void RaiseCurrentAppCultureLoadOrChangeEvent(object sender)
        {
            LoadResources();
        }

        private async void RaiseTcpCientConnectedEvent(object sender)
        {
            // This method is an async void event handler. Any unhandled exception
            // here would crash the entire process (async void propagates exceptions
            // to the SynchronizationContext, which for WPF terminates the app).
            // Wrap the entire body in try/catch so a missing or unreachable WebApi
            // server degrades gracefully instead of killing the app.
            try
            {
                _ = await mWebApiService.StopPythonExecute();

                var pythonVersionResult = await mWebApiService.GetPythonVersion();
                var pythonBinPathResult = await mWebApiService.GetPythonBinDir();
                var pythonWorkDirResult = await mWebApiService.GetPythonWorkDir();

                string pythonVersion = pythonVersionResult?.StandardOutput?.Replace("\n", "");
                string pythonBinPath = pythonBinPathResult?.StandardOutput?.Replace("\n", "");
                string pythonWorkDir = pythonWorkDirResult?.StandardOutput?.Replace("\n", "");

                UpdatePythonInfo(pythonVersion, pythonBinPath, pythonWorkDir);
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex,
                    "WebApi request failed in RaiseTcpCientConnectedEvent. " +
                    "The Adam-Servers WebApi is likely unreachable. " +
                    "Python info will not be loaded.");
                ApplicationLogs += $"WebApi unavailable: {ex.GetType().Name}: {ex.Message}\n";
            }
        }

        private void RaiseTcpClientDisconnectedEvent(object sender, bool isUserRequest)
        {
            
        }

        private void RaiseUdpServiceServerReceivedEvent(object sender, string message)
        {
            try
            {
                SyslogMessageModel syslogMessage = message.Parse();
                var messageString = syslogMessage.ToString();

                CompilerLogs += $"{messageString}\n";
            }
            catch
            {
                // If you couldn't read the message, it's okay, no one needs to know about it.
            }
        }

        private void IsNotificationFlyoutOpenedStateChangeEvent(object sender)
        {
            if (mFlyoutStateChecker.IsFlyoutsOpened)
            {
                LogsButtonIsChecked = false;
                ResultButtonIsChecked = false;
            }
        }

        private void RaiseClientConnectedEvent(object sender)
        {
            IsPythonCodeExecute = true;
            mIsWarningStackOwerflowAlreadyShow = false;
            ClearResults();
        }

        private void RaiseClientDataReceivedEvent(object sender, string data)
        {
            UpdateResultText(data);
        }

        private void RaiseClientDisconnectedEvent(object sender)
        {
            //if (remoteCommandExecuteResult == null)
            //    return;

            //UpdateResultText("", true);
            //UpdateResultExecutionTimeText(remoteCommandExecuteResult);
            IsPythonCodeExecute = false;
        }

        private void Subscribe()
        {
            //mTcpClientService.RaiseTcpCientConnectedEvent += RaiseTcpCientConnectedEvent;
            //mTcpClientService.RaiseTcpClientDisconnectedEvent += RaiseTcpClientDisconnectedEvent;
            mCommunicationProviderService.RaiseTcpServiceCientConnectedEvent += RaiseTcpCientConnectedEvent;
            mCommunicationProviderService.RaiseTcpServiceClientDisconnectEvent += RaiseTcpClientDisconnectedEvent;
            mCommunicationProviderService.RaiseUdpServiceServerReceivedEvent += RaiseUdpServiceServerReceivedEvent;

            mLogWriteEventAware.RaiseNewLogMessageWriteEvent += RaiseNewLogMessageWriteEvent;

            //mPythonRemoteRunner.RaisePythonScriptExecuteStartEvent += OnRaisePythonScriptExecuteStart;
            //mPythonRemoteRunner.RaisePythonStandartOutputEvent += OnRaisePythonStandartOutput;
            //mPythonRemoteRunner.RaisePythonScriptExecuteFinishEvent += OnRaisePythonScriptExecuteFinish;

            mCultureProvider.RaiseCurrentAppCultureLoadOrChangeEvent += RaiseCurrentAppCultureLoadOrChangeEvent;

            mFlyoutStateChecker.IsFlyoutsOpenedStateChangeEvent += IsNotificationFlyoutOpenedStateChangeEvent;

            mTcpPythonStreamServerService.RaiseClientConnectedEvent += RaiseClientConnectedEvent;
            mTcpPythonStreamServerService.RaiseClientDataReceivedEvent += RaiseClientDataReceivedEvent;
            mTcpPythonStreamServerService.RaiseClientDisconnectedEvent += RaiseClientDisconnectedEvent;
            
        }

        private void Unsubscribe()
        {
            //mTcpClientService.RaiseTcpCientConnectedEvent -= RaiseTcpCientConnectedEvent;
            //mTcpClientService.RaiseTcpClientDisconnectedEvent -= RaiseTcpClientDisconnectedEvent;
            mCommunicationProviderService.RaiseTcpServiceCientConnectedEvent -= RaiseTcpCientConnectedEvent;
            mCommunicationProviderService.RaiseTcpServiceClientDisconnectEvent -= RaiseTcpClientDisconnectedEvent;
            mCommunicationProviderService.RaiseUdpServiceServerReceivedEvent -= RaiseUdpServiceServerReceivedEvent;

            mLogWriteEventAware.RaiseNewLogMessageWriteEvent -= RaiseNewLogMessageWriteEvent;

            //mPythonRemoteRunner.RaisePythonScriptExecuteStartEvent -= OnRaisePythonScriptExecuteStart;
            //mPythonRemoteRunner.RaisePythonStandartOutputEvent -= OnRaisePythonStandartOutput;
            //mPythonRemoteRunner.RaisePythonScriptExecuteFinishEvent -= OnRaisePythonScriptExecuteFinish;

            mCultureProvider.RaiseCurrentAppCultureLoadOrChangeEvent -= RaiseCurrentAppCultureLoadOrChangeEvent;

            mFlyoutStateChecker.IsFlyoutsOpenedStateChangeEvent -= IsNotificationFlyoutOpenedStateChangeEvent;

            mTcpPythonStreamServerService.RaiseClientConnectedEvent -= RaiseClientConnectedEvent;
            mTcpPythonStreamServerService.RaiseClientDataReceivedEvent -= RaiseClientDataReceivedEvent;
            mTcpPythonStreamServerService.RaiseClientDisconnectedEvent += RaiseClientDisconnectedEvent;
        }

    }
}
