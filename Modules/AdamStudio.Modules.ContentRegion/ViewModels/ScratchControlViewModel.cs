using AdamBlocklyLibrary;
using AdamBlocklyLibrary.Enum;
using AdamBlocklyLibrary.Struct;
using AdamBlocklyLibrary.Toolbox;
using AdamBlocklyLibrary.ToolboxSets;
using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Core;
using AdamStudio.Core.Extensions;
using AdamStudio.Core.Model;
using AdamStudio.Core.Mvvm;
using AdamStudio.Core.Properties;
using AdamStudio.Services.Interfaces;
using AdamStudio.Services.SystemDialogServiceDependency;
using AdamStudio.Services.WebViewProviderDependency;
using AdamController.WebApi.Client.v1.ResponseModel;
using ICSharpCode.AvalonEdit.Highlighting;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace AdamStudio.Modules.ContentRegion.ViewModels
{
    public class ScratchControlViewModel : RegionViewModelBase 
    {
        #region DelegateCommands

        public DelegateCommand CopyToClipboardDelegateCommand { get; }
        public DelegateCommand ReloadWebViewDelegateCommand { get; }
        public DelegateCommand<string> ShowSaveFileDialogDelegateCommand { get; }
        public DelegateCommand<string> ShowOpenFileDialogDelegateCommand { get; }
        public DelegateCommand CleanExecuteEditorDelegateCommand { get; }
        public DelegateCommand RunPythonCodeDelegateCommand { get; }
        public DelegateCommand StopPythonCodeExecuteDelegateCommand { get; }
        public DelegateCommand ToZeroPositionDelegateCommand { get; }
        public DelegateCommand<string> DirectionButtonDownDelegateCommand { get; }
        public DelegateCommand<string> DirectionButtonUpDelegateCommand { get; }

        #endregion

        #region Services

        private readonly ICommunicationProviderService mCommunicationProvider;
        private readonly IPythonRemoteRunnerService mPythonRemoteRunner;
        private readonly IStatusBarNotificationDeliveryService mStatusBarNotificationDelivery;
        private readonly IWebViewProvider mWebViewProvider;
        private readonly IFileManagmentService mFileManagment;
        private readonly IWebApiService mWebApiService;
        private readonly ICultureProvider mCultureProvider;
        private readonly ISystemDialogService mSystemDialog;
        private readonly IControlHelper mControlHelper;
        private readonly IVideoViewProvider mVideoViewProvider;
        private readonly IRegionChangeAwareService mRegionChangeAwareService;
        private readonly ILogger<ScratchControlViewModel> mLogger;

        #endregion

        #region Const

        private const string cFilter = "XML documents (.xml) | *.xml";

        #endregion

        #region Var

        private bool mIsWarningStackOwerflowAlreadyShow;

        private string mCompileLogMessageStartDebug; 
        private string mCompileLogMessageEndDebug;
        private string mFinishAppExecute;
        private string mWarningStackOwerflow1;
        private string mWarningStackOwerflow2;
        private string mWarningStackOwerflow3;

        private string mSaveWorkspaceDialogTitle;
        private string mSaveScriptFileDialogTitle;

        private string mOpenFileDialogDialogTitle;
        private string mFileNotSavedLogMessage;
        private string mFileNotSelectedLogMessage;
        private string mFileSavedLogMessage;

        private string mScretchLoadedCompleteLogMessage;
        private string mScretchLoadedErrorLogMessage;

        private string mExtNotSupport1;
        private string mExtNotSupport2;

        private string mOpenFile;


        #endregion

        #region ~

        public ScratchControlViewModel(ILogger<ScratchControlViewModel> logger, IRegionManager regionManager, ICommunicationProviderService communicationProvider, IPythonRemoteRunnerService pythonRemoteRunner, 
                        IStatusBarNotificationDeliveryService statusBarNotificationDelivery, IWebViewProvider webViewProvider,
                        IFileManagmentService fileManagment, IWebApiService webApiService, IAvalonEditService avalonEditService,
                        ICultureProvider cultureProvider, ISystemDialogService systemDialogService, IControlHelper controlHelper,
                        IVideoViewProvider videoViewProvider, IRegionChangeAwareService regionChangeAwareService) : base(regionManager)
        {

            mLogger = logger;
            mCommunicationProvider = communicationProvider;
            mPythonRemoteRunner = pythonRemoteRunner;
            mStatusBarNotificationDelivery = statusBarNotificationDelivery;
            mWebViewProvider = webViewProvider;
            mFileManagment = fileManagment;
            mWebApiService = webApiService;
            mCultureProvider = cultureProvider;
            mSystemDialog = systemDialogService;
            mControlHelper = controlHelper;
            mVideoViewProvider = videoViewProvider;
            mRegionChangeAwareService = regionChangeAwareService;

            ShowSaveFileDialogDelegateCommand = new DelegateCommand<string>(ShowSaveFileDialog, ShowSaveFileDialogCanExecute);
            ShowOpenFileDialogDelegateCommand = new DelegateCommand<string>(ShowOpenFileDialog, ShowOpenFileDialogCanExecute);

            CopyToClipboardDelegateCommand = new DelegateCommand(CopyToClipboard, CopyToClipboardCanExecute);
            ReloadWebViewDelegateCommand = new DelegateCommand(ReloadWebView, ReloadWebViewCanExecute);
            CleanExecuteEditorDelegateCommand = new DelegateCommand(CleanExecuteEditor, CleanExecuteEditorCanExecute);
            RunPythonCodeDelegateCommand = new DelegateCommand(RunPythonCode, RunPythonCodeCanExecute);
            StopPythonCodeExecuteDelegateCommand = new DelegateCommand(StopPythonCodeExecute, StopPythonCodeExecuteCanExecute);
            ToZeroPositionDelegateCommand = new DelegateCommand(ToZeroPosition, ToZeroPositionCanExecute);

            DirectionButtonDownDelegateCommand = new DelegateCommand<string>(DirectionButtonDown, DirectionButtonDownCanExecute);
            DirectionButtonUpDelegateCommand = new DelegateCommand<string>(DirectionButtonUp, DirectionButtonUpCanExecute);

            HighlightingDefinition = avalonEditService.GetDefinition(HighlightingName.AdamPython);
        }

        #endregion

        #region Navigation

        public override void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            base.ConfirmNavigationRequest(navigationContext, continuationCallback);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            mRegionChangeAwareService.RegionNavigationTargetName = RegionNames.ScratchView;

            Subscribe();
            LoadResources();
            UpdateIsShowVideo(Settings.Default.ShowVideo);

            mWebViewProvider.ReloadWebView();

            

            base.OnNavigatedTo(navigationContext);
        }

        public override void Destroy()
        {
            Unsubscribe();

            base.Destroy();
        }

        #endregion

        #region Public fields

        private string videoFrameRate;
        public string VideoFrameRate
        {
            get { return videoFrameRate; }
            set { SetProperty(ref videoFrameRate, value); }
        }

        private bool isShowVideo; //= Settings.Default.ShowVideo;
        public bool IsShowVideo
        {
            get => isShowVideo;
            set
            {
                bool isNewValue = SetProperty(ref isShowVideo, value);
                
                if (isNewValue)
                {
                    Settings.Default.ShowVideo = IsShowVideo;
                }
            }
        }

        private bool isTcpClientConnected;
        public bool IsTcpClientConnected
        {
            get => isTcpClientConnected;
            set
            {
                bool isNewValue = SetProperty(ref isTcpClientConnected, value);

                if (isNewValue)
                {
                    RaiseDelegateCommandsCanExecuteChanged();
                }
            }
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
                    OnPythonCodeExecuteStatusChange(IsPythonCodeExecute);
                    RaiseDelegateCommandsCanExecuteChanged();
                }
            }
        }

        private string sourceTextEditor;
        public string SourceTextEditor
        {
            get => sourceTextEditor;
            set
            {
                bool isNewValue = SetProperty(ref sourceTextEditor, value);

                if (isNewValue)
                {
                    RaiseDelegateCommandsCanExecuteChanged();
                }
            }
        }

        private string resultText;
        public string ResultText
        {
            get => resultText;
            set 
            {
                bool isNewValue = SetProperty(ref resultText, value);

                if(isNewValue)
                    CleanExecuteEditorDelegateCommand.RaiseCanExecuteChanged();
            } 
        }

        private ExtendedCommandExecuteResult resultExecutionTime;
        public ExtendedCommandExecuteResult ResultExecutionTime
        {
            get => resultExecutionTime;
            set => SetProperty(ref resultExecutionTime, value);    
        }

        private ExtendedCommandExecuteResult resultInitilizationTime;
        public ExtendedCommandExecuteResult ResultInitilizationTime
        {
            get => resultInitilizationTime;
            set => SetProperty(ref resultInitilizationTime, value);
        }

        private string pythonVersion;
        public string PythonVersion
        {
            get => pythonVersion;
            set => SetProperty(ref pythonVersion, value);
        }

        private float sliderValue;
        public float SliderValue
        {
            get => sliderValue;
            set => SetProperty(ref sliderValue, value);
        }

        /*private string pythonBinPath;
        public string PythonBinPath
        {
            get => pythonBinPath;
            set => SetProperty(ref pythonBinPath, value);
        }

        private string pythonWorkDir;
        public string PythonWorkDir
        {
            get => pythonWorkDir;
            set => SetProperty(ref pythonWorkDir, value);
        }*/

        private IHighlightingDefinition highlightingDefinition;
        public IHighlightingDefinition HighlightingDefinition
        {
            get => highlightingDefinition;
            set => SetProperty(ref highlightingDefinition, value);
        }

        public string StopDirrection { get; private set; } = "{\"move\":{\"x\": 0, \"y\": 0, \"z\": 0}}";

        // left/right/up/down +
        public string ForwardDirection { get; private set; } = "{\"move\":{\"x\": 0, \"y\": 1, \"z\": 0}}";
        public string BackDirection { get; private set; } = "{\"move\":{\"x\": 0, \"y\": -1, \"z\": 0}}";
        public string LeftDirection { get; private set; } = "{\"move\":{\"x\": -1, \"y\": 0, \"z\": 0}}";
        public string RightDirection { get; private set; } = "{\"move\":{\"x\": 1, \"y\": 0, \"z\": 0}}";

        //
        public string ForwardLeftDirection { get; private set; } = "{\"move\":{\"x\": -1, \"y\": 1, \"z\": 0}}";
        public string ForwardRightDirection { get; private set; } = "{\"move\":{\"x\": 1, \"y\": 1, \"z\": 0}}";
        public string BackLeftDirection { get; private set; } = "{\"move\":{\"x\": -1, \"y\": -1, \"z\": 0}}";
        public string BackRightDirection { get; private set; } = "{\"move\":{\"x\": 1, \"y\": -1, \"z\": 0}}";

        //rotate +
        public string RotateRightDirrection { get; private set; } = "{\"move\":{\"x\": 0.0, \"y\": 0.0, \"z\": 1.0 }}";
        public string RotateLeftDirrection { get; private set; } = "{\"move\":{\"x\": 0.0, \"y\": 0.0, \"z\": -1.0}}";

        #endregion

        #region Subscribes

        private void Subscribe()
        {
            mCommunicationProvider.RaiseTcpServiceCientConnectedEvent += OnRaiseTcpServiceCientConnected;
            mCommunicationProvider.RaiseTcpServiceClientDisconnectEvent += OnRaiseTcpServiceClientDisconnect;

            mPythonRemoteRunner.RaisePythonScriptExecuteStartEvent += OnRaisePythonScriptExecuteStart;
            mPythonRemoteRunner.RaisePythonStandartOutputEvent += OnRaisePythonStandartOutput;
            mPythonRemoteRunner.RaisePythonScriptExecuteFinishEvent += OnRaisePythonScriptExecuteFinish;

            mWebViewProvider.RaiseWebViewMessageReceivedEvent += RaiseWebViewbMessageReceivedEvent;
            mWebViewProvider.RaiseWebViewNavigationCompleteEvent += RaiseWebViewNavigationCompleteEvent;

            mControlHelper.IsVideoShowChangeEvent += OnRaiseIsVideoShowChangeEvent;

            mVideoViewProvider.RaiseFrameRateUpdateEvent += RaiseFrameRateUpdateEvent;
        }

        private void Unsubscribe()
        {
            mCommunicationProvider.RaiseTcpServiceCientConnectedEvent -= OnRaiseTcpServiceCientConnected;
            mCommunicationProvider.RaiseTcpServiceClientDisconnectEvent -= OnRaiseTcpServiceClientDisconnect;

            mPythonRemoteRunner.RaisePythonScriptExecuteStartEvent -= OnRaisePythonScriptExecuteStart;
            mPythonRemoteRunner.RaisePythonStandartOutputEvent -= OnRaisePythonStandartOutput;
            mPythonRemoteRunner.RaisePythonScriptExecuteFinishEvent -= OnRaisePythonScriptExecuteFinish;

            mWebViewProvider.RaiseWebViewMessageReceivedEvent -= RaiseWebViewbMessageReceivedEvent;
            mWebViewProvider.RaiseWebViewNavigationCompleteEvent -= RaiseWebViewNavigationCompleteEvent;

            mControlHelper.IsVideoShowChangeEvent -= OnRaiseIsVideoShowChangeEvent;

            mVideoViewProvider.RaiseFrameRateUpdateEvent -= RaiseFrameRateUpdateEvent;
        }

        #endregion

        #region DelegateCommands methods

        private void CopyToClipboard()
        {
            Clipboard.SetText(SourceTextEditor);
        }

        private bool CopyToClipboardCanExecute()
        {
            bool isPythonCodeNotExecute = !IsPythonCodeExecute;
            bool isSourceNotEmpty = SourceTextEditor?.Length > 0;

            return isPythonCodeNotExecute && isSourceNotEmpty;
        }
        
        private void ReloadWebView()
        {
            mWebViewProvider.NeedReloadOnLoad = true;
            mWebViewProvider.ReloadWebView();
        }

        private bool ReloadWebViewCanExecute()
        {
            bool isPythonCodeNotExecute = !IsPythonCodeExecute;
            return isPythonCodeNotExecute;
        }

        private void ShowOpenFileDialog(string param)
        {
            string initialPath = string.Empty;

            if (param.Equals("Workspace"))
                initialPath = Settings.Default.SavedUserWorkspaceFolderPath;

            if (param.Equals("Script"))
                initialPath = Settings.Default.SavedUserScriptsFolderPath;

            string title = mOpenFileDialogDialogTitle;

            var dialogParametrs = new DialogParameters
            {
                { DialogParametrsKeysName.TitleParametr, title },
                { DialogParametrsKeysName.InitialDirectoryParametr, initialPath}
            };

            OpenFileDialogResult result = mSystemDialog.ShowOpenFileDialog(dialogParametrs);

            if (result.IsOpenFileCanceled)
            {
                mLogger.LogWarning(mFileNotSelectedLogMessage);
                return;
            }

            OpenSupportedFile(result);
        }

        private void ShowSaveFileDialog(string param)
        {
            string title = string.Empty;
            string initialPath = string.Empty;
            SupportFileType fileType = SupportFileType.Undefined;
            
            if (param.Equals("Workspace"))
            {
                title = mSaveWorkspaceDialogTitle;
                initialPath = Settings.Default.SavedUserWorkspaceFolderPath;
                fileType = SupportFileType.Workspace;
            }

            if (param.Equals("Script"))
            {
                title = mSaveScriptFileDialogTitle;
                initialPath = Settings.Default.SavedUserScriptsFolderPath;
                fileType = SupportFileType.Script;
            };
            
            var dialogParametrs = new DialogParameters
            {
                { DialogParametrsKeysName.TitleParametr, title },
                { DialogParametrsKeysName.InitialDirectoryParametr, initialPath },
                { DialogParametrsKeysName.SavedFileTypeParametr, fileType }
            };

            SaveFileDialogResult result = mSystemDialog.ShowSaveFileDialog(dialogParametrs);

            if (result.IsSaveFileCanceled)
            {
                mLogger.LogWarning(mFileNotSavedLogMessage);
                return;
            }

            SaveSupportedFile(result, fileType);
        }

        private bool ShowSaveFileDialogCanExecute(string param)
        {
            bool isPythonCodeNotExecute = !IsPythonCodeExecute;
            bool isSourceNotEmpty = SourceTextEditor?.Length > 0;

            return isPythonCodeNotExecute && isSourceNotEmpty;
        }

        private bool ShowOpenFileDialogCanExecute(string param)
        {
            bool isPythonCodeNotExecute = !IsPythonCodeExecute;
            return isPythonCodeNotExecute;
        }

        private void CleanExecuteEditor()
        {
            ClearResultText();   
        }

        private bool CleanExecuteEditorCanExecute()
        {
            bool isPythonCodeNotExecute = !IsPythonCodeExecute;
            var isResultNotEmpty = ResultText?.Length > 0;
            return isPythonCodeNotExecute &&  isResultNotEmpty;
        }

        private async void RunPythonCode()
        {

            string source = SourceTextEditor;
            
            try
            {
                ClearResultText();

                var command = new AdamController.WebApi.Client.v1.RequestModel.PythonCommandModel
                {
                    Command = source
                };

                ExtendedCommandExecuteResult executeResult = await mWebApiService.PythonExecuteAsync(command);
                UpdateResultInitilizationTimeText(executeResult);
            }
            catch
            {
                
            }
            finally
            {
               
            }
        }

        private bool RunPythonCodeCanExecute()
        {
            bool isSourceNotEmpty = SourceTextEditor?.Length > 0;
            bool isTcpConnected = IsTcpClientConnected;
            bool isPythonCodeNotExecute = !IsPythonCodeExecute;

            return isSourceNotEmpty && isTcpConnected && isPythonCodeNotExecute;
        }

        private async void StopPythonCodeExecute()
        {
            IsPythonCodeExecute = false;

            try
            {
                await mWebApiService.StopPythonExecute();
            }
            catch
            {

            }
        }

        private bool StopPythonCodeExecuteCanExecute()
        {
            bool isConnected = IsTcpClientConnected;
            bool isPythonCodeExecute = IsPythonCodeExecute;

            return isConnected && isPythonCodeExecute;
        }

        private async void ToZeroPosition()
        {
            try
            {
                await mWebApiService.StopPythonExecute();
                await mWebApiService.MoveToZeroPosition();
            }
            catch
            {

            }
        }

        private bool ToZeroPositionCanExecute()
        {
            bool isTcpConnected = IsTcpClientConnected;
            bool isPythonCodeNotExecute = !IsPythonCodeExecute;

            return isTcpConnected && isPythonCodeNotExecute;
        }

        private void DirectionButtonDown(string obj)
        {
            VectorModel vectorSource = JsonSerializer.Deserialize<VectorModel>(obj);

            if (vectorSource == null)
                return;

            if (vectorSource.Move.X == 1)
            {
                vectorSource.Move.X = SliderValue;
            }
            else if (vectorSource.Move.X == -1)
            {
                vectorSource.Move.X = -SliderValue;
            }

            if (vectorSource.Move.Y == 1)
            {
                vectorSource.Move.Y = SliderValue;
            }
            else if (vectorSource.Move.Y == -1)
            {
                vectorSource.Move.Y = -SliderValue;
            }

            if (vectorSource.Move.Z == 1)
            {
                vectorSource.Move.Z = SliderValue;
            }
            else if (vectorSource.Move.Z == -1)
            {
                vectorSource.Move.Z = -SliderValue;
            }

            var json = JsonSerializer.Serialize(vectorSource);
            mCommunicationProvider.WebSocketSendTextMessage(json);
        }

        private bool DirectionButtonDownCanExecute(string arg)
        {
            return true;
        }

        private void DirectionButtonUp(string obj)
        {
            VectorModel vector = new()
            {
                Move = new VectorItem
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                }
            };

            var json = JsonSerializer.Serialize(vector);
            mCommunicationProvider.WebSocketSendTextMessage(json);
        }

        private bool DirectionButtonUpCanExecute(string arg)
        {
            return true;
        }

        #endregion

        #region Private methods

        private void UpdateIsShowVideo(bool isShowVideo)
        {
            IsShowVideo = isShowVideo;
        }

        private void UpdateResultText(string text, bool isFinishMessage = false)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (isFinishMessage)
                {
                    ResultText += "\n======================\n";
                    ResultText += $"<<{mFinishAppExecute}>>\n";
                }

                if(!isFinishMessage)
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

        private void UpdateResultInitilizationTimeText(ExtendedCommandExecuteResult executeResult)
        {
            ResultInitilizationTime = executeResult;
        }

        private void ClearResultText()
        {
            ResultText = string.Empty;
            ResultExecutionTime = null;
            ResultInitilizationTime = null;
        }

        private void OnPythonCodeExecuteStatusChange(bool isPythonCodeExecute)
        {
            
            if (!isPythonCodeExecute)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(async () =>
                {
                    await mWebViewProvider.ExecuteJavaScript(Scripts.ShadowDisable);
                }));

                mStatusBarNotificationDelivery.CompileLogMessage = mCompileLogMessageEndDebug;
                mStatusBarNotificationDelivery.ProgressRingStart = false;
                return;
            }

            mIsWarningStackOwerflowAlreadyShow = false;
            mStatusBarNotificationDelivery.CompileLogMessage = mCompileLogMessageStartDebug;
            mStatusBarNotificationDelivery.ProgressRingStart = true;


            if (!Settings.Default.ShadowWorkspaceInDebug) return;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(async () =>
            {
                await mWebViewProvider.ExecuteJavaScript(Scripts.ShadowEnable);
            }));
        }

        private void UpdatePythonInfo(string pythonVersion = "", string pythonBinPath = "", string pythonWorkDir = "")
        {
            if (string.IsNullOrEmpty(pythonVersion))
            {
                PythonVersion = string.Empty;
                //PythonBinPath = string.Empty;
                //PythonWorkDir = string.Empty;
                return;
            }

            PythonVersion = pythonVersion;
            //PythonBinPath = $"[{pythonBinPath}]";
            //PythonWorkDir = $"Рабочая дирректория {pythonWorkDir}";
        }

        private async void OpenSupportedFile(OpenFileDialogResult result)
        {
            string path = result.OpenFilePath;

            switch (result.OpenFileType)
            {
                case SupportFileType.Undefined:
                    mLogger.LogInformation($"{mExtNotSupport1} {Path.GetExtension(path)} {mExtNotSupport2}");
                    break;
                case SupportFileType.Script:
                    SourceTextEditor = await mFileManagment.ReadTextAsStringAsync(path);
                    mLogger.LogInformation($"{mOpenFile} {path}");
                    break;
                case SupportFileType.Workspace:
                    string xml = await mFileManagment.ReadTextAsStringAsync(path);
                    _ = await ExecuteScriptFunctionAsync("loadSavedWorkspace", new object[] { xml });
                    mLogger.LogInformation($"{mOpenFile} {path}");
                    break;
            }
        }

        private async void SaveSupportedFile(SaveFileDialogResult result, SupportFileType fileType)
        {
            string path = result.SavedFilePath;

            if (fileType == SupportFileType.Script)
            {
                string file = SourceTextEditor;
                await mFileManagment.WriteAsync(path, file);
            }
            if (fileType == SupportFileType.Workspace)
            {
                string file = await mWebViewProvider.ExecuteJavaScript("getSavedWorkspace()", true);
                await mFileManagment.WriteAsync(path, file);
            }

            mLogger.LogInformation($"{mFileSavedLogMessage} {path}");
        }

        private void RaiseDelegateCommandsCanExecuteChanged()
        {
            CopyToClipboardDelegateCommand.RaiseCanExecuteChanged();
            ReloadWebViewDelegateCommand.RaiseCanExecuteChanged();
            ShowSaveFileDialogDelegateCommand.RaiseCanExecuteChanged();
            ShowOpenFileDialogDelegateCommand.RaiseCanExecuteChanged();
            CleanExecuteEditorDelegateCommand.RaiseCanExecuteChanged();
            RunPythonCodeDelegateCommand.RaiseCanExecuteChanged();
            StopPythonCodeExecuteDelegateCommand.RaiseCanExecuteChanged();
            ToZeroPositionDelegateCommand.RaiseCanExecuteChanged();
        }

        private void LoadResources()
        {
            mCompileLogMessageStartDebug = mCultureProvider.FindResource("DebuggerMessages.CompileLogMessageStartDebug");
            mCompileLogMessageEndDebug = mCultureProvider.FindResource("DebuggerMessages.CompileLogMessageEndDebug");
            mFinishAppExecute = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.FinishAppExecute");

            mWarningStackOwerflow1 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow1");
            mWarningStackOwerflow2 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow2");
            mWarningStackOwerflow3 = mCultureProvider.FindResource("DebuggerMessages.ResultMessages.WarningStackOwerflow3");

            mSaveWorkspaceDialogTitle = mCultureProvider.FindResource("ScratchControlViewModel.SaveWorkspaceFileDialog.DialogTitle");
            mSaveScriptFileDialogTitle = mCultureProvider.FindResource("ScratchControlViewModel.SaveScriptFileDialog.DialogTitle");

            mOpenFileDialogDialogTitle = mCultureProvider.FindResource("ScratchControlViewModel.OpenFileDialog.Title");
            mFileNotSavedLogMessage = mCultureProvider.FindResource("ScratchControlViewModel.Dialogs.FileNotSaved.LogMessage");
            mFileNotSelectedLogMessage = mCultureProvider.FindResource("ScratchControlViewModel.Dialogs.FileNotSelected.LogMessage");
           
            mFileSavedLogMessage = mCultureProvider.FindResource("ScratchControlViewModel.Dialogs.FileSaved.LogMessage");
            mScretchLoadedCompleteLogMessage = mCultureProvider.FindResource("ScratchControlViewModel.ScretchLoadedComplete.LogMessage");
            mScretchLoadedErrorLogMessage = mCultureProvider.FindResource("ScratchControlViewModel.ScretchLoadedError.LogMessage");

            mExtNotSupport1 = mCultureProvider.FindResource("ScratchControlViewModel.OpenFileDialog.ExtensionNotSupported1");
            mExtNotSupport2 = mCultureProvider.FindResource("ScratchControlViewModel.OpenFileDialog.ExtensionNotSupported2");
            mOpenFile = mCultureProvider.FindResource("ScratchControlViewModel.OpenFileDialog.FileOpened.LogMessage");
        }

        #endregion

        #region Event methods

        private void RaiseWebViewNavigationCompleteEvent(object sender)
        {
            InitBlockly();
        }

        private void RaiseWebViewbMessageReceivedEvent(object sender, WebMessageJsonReceived webMessageReceived)
        {
            if (webMessageReceived.Action == "sendSourceCode")
            {
                SourceTextEditor = webMessageReceived.Data;
            }
        }

        private async void OnRaiseTcpServiceCientConnected(object sender)
        {
            IsTcpClientConnected = mCommunicationProvider.IsTcpClientConnected;

            var pythonVersionResult = await mWebApiService.GetPythonVersion();
            var pythonBinPathResult = await mWebApiService.GetPythonBinDir();
            var pythonWorkDirResult = await mWebApiService.GetPythonWorkDir();

            string pythonVersion = pythonVersionResult?.StandardOutput?.Replace("\n", "");
            string pythonBinPath = pythonBinPathResult?.StandardOutput?.Replace("\n", "");
            string pythonWorkDir = pythonWorkDirResult?.StandardOutput?.Replace("\n", "");

            UpdatePythonInfo(pythonVersion, pythonBinPath, pythonWorkDir);
        }

        private void OnRaiseTcpServiceClientDisconnect(object sender, bool isUserRequest)
        {
            IsTcpClientConnected = mCommunicationProvider.IsTcpClientConnected;

            UpdatePythonInfo();
        }

        private void OnRaisePythonScriptExecuteStart(object sender)
        {
            IsPythonCodeExecute = true;
        }

        private void OnRaisePythonStandartOutput(object sender, string message)
        {
            UpdateResultText(message);
        }

        private void OnRaisePythonScriptExecuteFinish(object sender, ExtendedCommandExecuteResult remoteCommandExecuteResult)
        {
            if (remoteCommandExecuteResult == null)
                return;
   
            UpdateResultText("", true);
            UpdateResultExecutionTimeText(remoteCommandExecuteResult);
        }

        private void OnRaiseIsVideoShowChangeEvent(object sender)
        {
            UpdateIsShowVideo(mControlHelper.IsShowVideo);
        }

        private void RaiseFrameRateUpdateEvent(object sender)
        {
            double rate = double.Round(mVideoViewProvider.FrameRate, 2);

            if (double.IsNaN(rate))
            {
                VideoFrameRate = string.Empty;
                return;
            }

            VideoFrameRate = $"{rate} FPS";
        }

        #endregion

        #region Initialize Blockly

        private async void InitBlockly()
        {
            BlocklyLanguage language = Settings.Default.BlocklyWorkspaceLanguage;

            try
            {
                await LoadBlocklySrc();
                await LoadBlocklyBlockLocalLangSrc(language);

                await mWebViewProvider.ExecuteJavaScript(InitWorkspace());
                await mWebViewProvider.ExecuteJavaScript(Scripts.ListenerCreatePythonCode);
                await mWebViewProvider.ExecuteJavaScript(Scripts.ListenerSavedBlocks);

                if (Settings.Default.BlocklyRestoreBlockOnLoad)
                {
                    await mWebViewProvider.ExecuteJavaScript(Scripts.RestoreSavedBlocks);
                }

                mLogger.LogInformation(mScretchLoadedCompleteLogMessage);
            }
            catch
            {
                mLogger.LogWarning(mScretchLoadedErrorLogMessage);
            }
        }

        private static string InitWorkspace()
        {
            Toolbox toolbox = InitDefaultToolbox(Settings.Default.BlocklyToolboxLanguage);
            BlocklyGrid blocklyGrid = InitGrid();

            Workspace workspace = new()
            {
                Toolbox = toolbox,
                BlocklyGrid = blocklyGrid,
                Theme = Settings.Default.BlocklyTheme,
                ShowTrashcan = Settings.Default.BlocklyShowTrashcan,
                Render = Render.thrasos,
                Collapse = true
            };

            string workspaceString = Scripts.SerealizeObjectToJsonString("init", new object[] { workspace });
            return workspaceString;
        }

        private static BlocklyGrid InitGrid()
        {
            BlocklyGrid blocklyGrid = new();

            if (Settings.Default.BlocklyShowGrid)
            {
                blocklyGrid.Length = Settings.Default.BlocklyGridSpacing;
                blocklyGrid.Spacing = Settings.Default.BlocklyGridSpacing;
            }
            else
            {
                blocklyGrid.Length = 0;
                blocklyGrid.Spacing = 0;
            }

            blocklyGrid.Colour = Settings.Default.BlocklyGridColour.HexToRbgColor();
            blocklyGrid.Snap = Settings.Default.BlocklySnapToGridNodes;

            return blocklyGrid;
        }

        private static Toolbox InitDefaultToolbox(BlocklyLanguage language)
        {
            Toolbox toolbox = new DefaultSimpleCategoryToolbox(language)
            {
                LogicCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyLogicCategoryState,
                    AlternateName = Settings.Default.BlocklyLogicCategoryAlternateName
                },
                ColourCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyColorCategoryState,
                    AlternateName = Settings.Default.BlocklyColourCategoryAlternateName
                },
                ListsCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyListsCategoryState,
                    AlternateName = Settings.Default.BlocklyListsCategoryAlternateName
                },
                LoopsCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyLoopCategoryState,
                    AlternateName = Settings.Default.BlocklyLoopCategoryAlternateName
                },
                MathCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyMathCategoryState,
                    AlternateName = Settings.Default.BlocklyMathCategoryAlternateName
                },
                TextCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyTextCategoryState,
                    AlternateName = Settings.Default.BlocklyTextCategoryAlternateName
                },
                ProcedureCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyProcedureCategoryState,
                    AlternateName = Settings.Default.BlocklyProcedureCategoryAlternateName
                },
                VariableDynamicCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyDynamicVariableCategoryState,
                    AlternateName = Settings.Default.BlocklyDynamicVariableCategoryAlternateName
                },
                VariableCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyVariableCategoryState,
                    AlternateName = Settings.Default.BlocklyVariableCategoryAlternateName
                },
                DateTimeCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyDateTimeCategoryState,
                    AlternateName = Settings.Default.BlocklyDateTimeCategoryAlternateName
                },
                AdamCommonCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyAdamCommonCategoryState,
                    AlternateName = Settings.Default.BlocklyAdamCommonCategoryAlternateName
                },
                AdamTwoCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyAdamTwoCategoryState,
                    AlternateName = Settings.Default.BlocklyAdamTwoCategoryAlternateName
                },
                AdamThreeCategoryParam = new ToolboxParam
                {
                    Hidden = !Settings.Default.BlocklyAdamThreeCategoryState,
                    AlternateName = Settings.Default.BlocklyAdamThreeCategoryAlternateName
                }
            }.Toolbox;

            return toolbox;
        }

        #endregion

        #region Blockly method

        private async Task LoadBlocklySrc()
        {
            string loadLocalSrc = Scripts.SerealizeObject("loadSrcs", new object[]
            {
                Scripts.BlocksCompressedSrc,
                Scripts.JavascriptCompressedSrc,
                Scripts.PythonCompressedSrc,
            });

            string loadLocalAdamBlockSrc = Scripts.SerealizeObject("loadSrcs", new object[]
            {
                Scripts.DateTimeBlockSrc,
                Scripts.ThreadBlockSrc,
                Scripts.SystemsBlockSrc,
                Scripts.AdamThreeBlockSrc,
                Scripts.AdamTwoBlockSrc,
                Scripts.AdamCommonBlockSrc,
            });


            string loadLocalAdamPythonGenSrc = Scripts.SerealizeObject("loadSrcs", new object[]
            {
                Scripts.DateTimePytnonGenSrc,
                Scripts.ThreadPytnonGenSrc,
                Scripts.SystemsPythonGenSrc,
                Scripts.AdamThreePytnonGenSrc,
                Scripts.AdamTwoPytnonGenSrc,
                Scripts.AdamCommonPytnonGenSrc
            });

            await mWebViewProvider.ExecuteJavaScript(loadLocalSrc);
            await mWebViewProvider.ExecuteJavaScript(loadLocalAdamBlockSrc);
            await mWebViewProvider.ExecuteJavaScript(loadLocalAdamPythonGenSrc);
        }

        /// <summary>
        /// Loading block language
        /// </summary>
        /// <param name="language"></param>
        private async Task LoadBlocklyBlockLocalLangSrc(BlocklyLanguage language)
        {
            switch (language)
            {
                case BlocklyLanguage.ru:
                    {
                        _ = await ExecuteScriptFunctionAsync("loadSrc", Scripts.BlockLanguageRu);
                        break;
                    }
                case BlocklyLanguage.en:
                    {
                        _ = await ExecuteScriptFunctionAsync("loadSrc", Scripts.BlockLanguageEn);
                        break;
                    }
            }
        }

        #endregion

        #region ExecuteScripts

        private async Task<string> ExecuteScriptFunctionAsync(string functionName, params object[] parameters)
        {
            string script = Scripts.SerealizeObject(functionName, parameters);
            return await mWebViewProvider.ExecuteJavaScript(script);
        }

        #endregion

    }
}
