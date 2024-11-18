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
using Microsoft.Extensions.DependencyInjection;
using AdamStudio.Services;

namespace AdamStudio.Modules.ContentRegion.ViewModels
{
    public class ScratchControlViewModel : RegionViewModelBase 
    {
        #region DelegateCommands

        public DelegateCommand RotateScreenDelegateCommand { get; }
        public DelegateCommand CopyToClipboardDelegateCommand { get; }
        public DelegateCommand ReloadWebViewDelegateCommand { get; }
        public DelegateCommand<string> ShowSaveFileDialogDelegateCommand { get; }
        public DelegateCommand<string> ShowOpenFileDialogDelegateCommand { get; }
        public DelegateCommand RunPythonCodeDelegateCommand { get; }
        public DelegateCommand StopPythonCodeExecuteDelegateCommand { get; }
        public DelegateCommand ToZeroPositionDelegateCommand { get; }
        public DelegateCommand<string> DirectionButtonDownDelegateCommand { get; }
        public DelegateCommand<string> DirectionButtonUpDelegateCommand { get; }


        #endregion

        #region Services

        private readonly ILogger<ScratchControlViewModel> mLogger;
        private readonly ICommunicationProviderService mCommunicationProvider;
        private readonly IPythonRemoteRunnerService mPythonRemoteRunner;
        private readonly IStatusBarNotificationDeliveryService mStatusBarNotificationDelivery;
        private readonly IWebViewProvider mWebViewProvider;
        private readonly IFileManagmentService mFileManagment;
        private readonly IWebApiService mWebApiService;
        private readonly ICultureProvider mCultureProvider;
        private readonly ISystemDialogService mSystemDialog;
        private readonly IControlHelperService mControlHelper;
        private readonly IVideoViewProvider mVideoViewProvider;
        private readonly IRegionChangeAwareService mRegionChangeAwareService;

        #endregion

        #region Var

        private string mCompileLogMessageStartDebug; 
        private string mCompileLogMessageEndDebug;

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

        public ScratchControlViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            mLogger = serviceProvider.GetService<ILogger<ScratchControlViewModel>>();
            mCommunicationProvider = serviceProvider.GetService<ICommunicationProviderService>();
            mPythonRemoteRunner = serviceProvider.GetService<IPythonRemoteRunnerService>();
            mStatusBarNotificationDelivery = serviceProvider.GetService<IStatusBarNotificationDeliveryService>();
            mWebViewProvider = serviceProvider.GetService<IWebViewProvider>();
            mFileManagment = serviceProvider.GetService<IFileManagmentService>();
            mWebApiService = serviceProvider.GetService<IWebApiService>();
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            mSystemDialog = serviceProvider.GetService<ISystemDialogService>();
            mControlHelper = serviceProvider.GetService<IControlHelperService>();
            mVideoViewProvider = serviceProvider.GetService<IVideoViewProvider>();
            mRegionChangeAwareService = serviceProvider.GetService<IRegionChangeAwareService>();

            RotateScreenDelegateCommand = new DelegateCommand(RotateScreen, RotateScreenCanExecute);
            ShowSaveFileDialogDelegateCommand = new DelegateCommand<string>(ShowSaveFileDialog, ShowSaveFileDialogCanExecute);
            ShowOpenFileDialogDelegateCommand = new DelegateCommand<string>(ShowOpenFileDialog, ShowOpenFileDialogCanExecute);

            CopyToClipboardDelegateCommand = new DelegateCommand(CopyToClipboard, CopyToClipboardCanExecute);
            ReloadWebViewDelegateCommand = new DelegateCommand(ReloadWebView, ReloadWebViewCanExecute);

            RunPythonCodeDelegateCommand = new DelegateCommand(RunPythonCode, RunPythonCodeCanExecute);
            StopPythonCodeExecuteDelegateCommand = new DelegateCommand(StopPythonCodeExecute, StopPythonCodeExecuteCanExecute);
            ToZeroPositionDelegateCommand = new DelegateCommand(ToZeroPosition, ToZeroPositionCanExecute);

            DirectionButtonDownDelegateCommand = new DelegateCommand<string>(DirectionButtonDown, DirectionButtonDownCanExecute);
            DirectionButtonUpDelegateCommand = new DelegateCommand<string>(DirectionButtonUp, DirectionButtonUpCanExecute);

            HighlightingDefinition = serviceProvider.GetService<IAvalonEditService>().GetDefinition(HighlightingName.AdamPython);

            Subscribe();
        }

        #endregion

        #region Navigation

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            mRegionChangeAwareService.RegionNavigationTargetName = ViewNames.ScratchView;

            LoadResources();
            UpdateIsShowVideo(Settings.Default.ShowVideo);
            UpdateScreenRotate(Settings.Default.VideoScreenAngle);

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

        private ushort videoViewRotateAngle;
        public ushort VideoViewRotateAngle
        {
            get { return videoViewRotateAngle; }
            set 
            { 
                bool isNewValue = SetProperty(ref videoViewRotateAngle, value);  
                
                if(isNewValue)
                    Settings.Default.VideoScreenAngle = VideoViewRotateAngle;
            }
        }

        private string videoFrameRate;
        public string VideoFrameRate
        {
            get { return videoFrameRate; }
            set { SetProperty(ref videoFrameRate, value); }
        }

        private bool isShowVideo; 
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

        private float sliderValue;
        public float SliderValue
        {
            get => sliderValue;
            set => SetProperty(ref sliderValue, value);
        }

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

            mPythonRemoteRunner.RaisePythonScriptExecuteFinishEvent -= OnRaisePythonScriptExecuteFinish;

            mWebViewProvider.RaiseWebViewMessageReceivedEvent -= RaiseWebViewbMessageReceivedEvent;
            mWebViewProvider.RaiseWebViewNavigationCompleteEvent -= RaiseWebViewNavigationCompleteEvent;

            mControlHelper.IsVideoShowChangeEvent -= OnRaiseIsVideoShowChangeEvent;

            mVideoViewProvider.RaiseFrameRateUpdateEvent -= RaiseFrameRateUpdateEvent;
        }

        #endregion

        #region DelegateCommands methods

        private void RotateScreen()
        {
            UpdateScreenRotate(VideoViewRotateAngle);
        }

        private bool RotateScreenCanExecute()
        {
            return true;
        }

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

        private async void RunPythonCode()
        {
            string source = SourceTextEditor;
            
            try
            {
                var command = new AdamController.WebApi.Client.v1.RequestModel.PythonCommandModel
                {
                    Command = source
                };

                IsPythonCodeExecute = true;
                
                _ = await mWebApiService.PythonExecuteAsync(command);
            }
            catch
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
            catch {}
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
            catch {}
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

        private void UpdateScreenRotate(ushort rotateAngle)
        {
            if (rotateAngle == 180)
                VideoViewRotateAngle = 360;
            else
                VideoViewRotateAngle = 180;
        }

        private void OnPythonCodeExecuteStatusChange(bool isPythonCodeExecute)
        {
            
            if (!isPythonCodeExecute)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(async () =>
                {
                    await mWebViewProvider.ExecuteJavaScript(Scripts.ShadowDisable);
                }));

                mLogger.LogInformation(mCompileLogMessageEndDebug);
                mStatusBarNotificationDelivery.ProgressRingStart = false;
                return;
            }

            mLogger.LogInformation(mCompileLogMessageStartDebug);
            mStatusBarNotificationDelivery.ProgressRingStart = true;

            if (!Settings.Default.ShadowWorkspaceInDebug) return;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(async () =>
            {
                await mWebViewProvider.ExecuteJavaScript(Scripts.ShadowEnable);
            }));
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
            RunPythonCodeDelegateCommand.RaiseCanExecuteChanged();
            StopPythonCodeExecuteDelegateCommand.RaiseCanExecuteChanged();
            ToZeroPositionDelegateCommand.RaiseCanExecuteChanged();
        }

        private void LoadResources()
        {
            mCompileLogMessageStartDebug = mCultureProvider.FindResource("DebuggerMessages.CompileLogMessageStartDebug");
            mCompileLogMessageEndDebug = mCultureProvider.FindResource("DebuggerMessages.CompileLogMessageEndDebug");

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
            mLogger.LogTrace("Called RaiseWebViewNavigationCompleteEvent");
        }

        private void RaiseWebViewbMessageReceivedEvent(object sender, WebMessageJsonReceived webMessageReceived)
        {
            if (webMessageReceived.Action == "sendSourceCode")
            {
                SourceTextEditor = webMessageReceived.Data;
            }
        }

        private void OnRaiseTcpServiceCientConnected(object sender)
        {
            IsTcpClientConnected = mCommunicationProvider.IsTcpClientConnected;
        }

        private void OnRaiseTcpServiceClientDisconnect(object sender, bool isUserRequest)
        {
            IsTcpClientConnected = mCommunicationProvider.IsTcpClientConnected;
        }

        private void OnRaisePythonScriptExecuteFinish(object sender, ExtendedCommandExecuteResult remoteCommandExecuteResult)
        {
           IsPythonCodeExecute = false;
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
