#region system

using System;
using System.Windows;

#endregion

#region prism

using Prism.Ioc;
using Prism.DryIoc;
using Prism.Modularity;
using Prism.Regions;
using DryIoc;

#endregion

#region mahapps

using MahApps.Metro.Controls;

#endregion

#region other

using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;
using Serilog.Core;

using AdamStudio.Controls.CustomControls.RegionAdapters;
using AdamStudio.Modules.ContentRegion;
using AdamStudio.Modules.FlayoutsRegion;
using AdamStudio.Modules.MenuRegion;
using AdamStudio.Modules.StatusBarRegion;
using AdamStudio.Modules.ToolBarRegion;
using AdamStudio.Core.Properties;
using AdamStudio.Services.Interfaces;
using AdamStudio.Views;
using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Services.TcpClientDependency;
using AdamStudio.Services;
using Microsoft.Extensions.Options;
using ICSharpCode.AvalonEdit.Highlighting;
using AdamStudio.Core;
using Bluegrams.Application;

#endregion

namespace AdamStudio
{
    public partial class App : PrismApplication
    {

        #region ~

        public App()
        {
            Subscribe();
            LoadSharedFFmpegLibrary();
            InitPortableSettings();
        }

        #endregion

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IServiceSettings, DefaultServiceSettings>();
            containerRegistry.RegisterSingleton<ILogWriteEventAwareService, LogWriteEventAwareService>();
            containerRegistry.RegisterSingleton<IFlyoutStateChecker, FlyoutStateChecker>();
            containerRegistry.RegisterSingleton<ICultureProvider, CultureProvider>();
            containerRegistry.RegisterSingleton<IFileManagmentService, FileManagmentService>();
            containerRegistry.RegisterSingleton<IFolderManagmentService, FolderManagmentService>();
            containerRegistry.RegisterSingleton<IAvalonEditService, AvalonEditService>();
            containerRegistry.RegisterSingleton<IWebViewProvider, WebViewProvider>();
            containerRegistry.RegisterSingleton<IRegionChangeAwareService, RegionChangeAwareService>();
            containerRegistry.RegisterSingleton<IStatusBarNotificationDeliveryService, StatusBarNotificationDeliveryService>();
            containerRegistry.RegisterSingleton<IFlyoutManager, FlyoutManager>();
            containerRegistry.RegisterSingleton<ITcpClientService, TcpClientService>();
            containerRegistry.RegisterSingleton<IUdpClientService, UdpClientService>();
            containerRegistry.RegisterSingleton<IUdpServerService, UdpServerService>();
            containerRegistry.RegisterSingleton<IWebSocketClientService, WebSocketClientService>();
            containerRegistry.RegisterSingleton<IWebApiService, WebApiService>();
            containerRegistry.RegisterSingleton<ICommunicationProviderService, CommunicationProviderService>();
            containerRegistry.RegisterSingleton<IPythonRemoteRunnerService, PythonRemoteRunnerService>();
            containerRegistry.RegisterSingleton<IThemeManagerService, ThemeManagerService>();
            containerRegistry.RegisterSingleton<IControlHelperService, ControlHelperService>();
            containerRegistry.RegisterSingleton<IVideoViewProvider, VideoViewProvider>();

            RegisterDialogs(containerRegistry);
            RegisterService(containerRegistry);
            RegisterAvalonHighlightingDefinition();
        }

        private void RegisterService(IContainerRegistry containerRegistry)
        {
            
            containerRegistry.RegisterServices(services =>
            {
                ILogWriteEventAwareService logWriteEventAware = Container.Resolve<ILogWriteEventAwareService>();

                Logger mainLogger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.DelegatingTextSink(writeAction => logWriteEventAware.WriteToBuffer(writeAction),
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("logs/log-.txt",
                            rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                services.AddLogging(s => s.AddSerilog(mainLogger, dispose: true));
            });
        }

        private static void RegisterDialogs(IContainerRegistry containerRegistry)
        {
            // used for call system dialog for open save/open/select file/folder (Microsoft.Win32 dialogs)
            containerRegistry.RegisterSingleton<ISystemDialogService, SystemDialogService>();

            //Dialog boxes are not used, but implemented
            //containerRegistry.RegisterDialog<SettingsView, SettingsViewModel>();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);

            regionAdapterMappings.RegisterMapping(typeof(FlyoutsControl), Container.Resolve<FlyoutsControlRegionAdapter>());
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MenuRegionModule>();
            moduleCatalog.AddModule<ContentRegionModule>();
            moduleCatalog.AddModule<StatusBarRegionModule>();
            moduleCatalog.AddModule<FlayoutsRegionModule>();
            moduleCatalog.AddModule<ToolBarRegionModule>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            OnAppCrashOrExit();

            base.OnExit(e);
        }

        private void OnAppCrashOrExit()
        {
            Unsubscribe();
            Dispose();
            Current.Shutdown();
        }

        public void Dispose()
        {
            IRegionManager regionManager = Container.Resolve<IRegionManager>();

            //Called Destroy() in module
            foreach (IRegion region in regionManager.Regions)
            {
                region.RemoveAll();
            }

            Container.Resolve<IThemeManagerService>().Dispose();
            Container.Resolve<IFileManagmentService>().Dispose();
            Container.Resolve<IFolderManagmentService>().Dispose();
            Container.Resolve<IAvalonEditService>().Dispose();
            Container.Resolve<IRegionChangeAwareService>().Dispose();
            Container.Resolve<IStatusBarNotificationDeliveryService>().Dispose();
            Container.Resolve<IWebViewProvider>().Dispose();
            Container.Resolve<ISystemDialogService>().Dispose();
            Container.Resolve<IPythonRemoteRunnerService>().Dispose();
            Container.Resolve<ICommunicationProviderService>().Dispose();
            Container.Resolve<IWebApiService>().Dispose();
            Container.Resolve<ICultureProvider>().Dispose();
            Container.Resolve<IControlHelperService>().Dispose();
            Container.Resolve<ILogWriteEventAwareService>().Dispose();
        }

        #region Subscribes

        private void Subscribe()
        {
            SubscribeUnhandledExceptionHandling();
            Settings.Default.PropertyChanged += OnPropertyChange;
        }

        private void Unsubscribe()
        {
            Settings.Default.PropertyChanged -= OnPropertyChange;
        }

        #endregion

        #region OnRaise event
        
        private void OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        #endregion

        #region Intercepting Unhandled Exception

        private void SubscribeUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Current.Dispatcher.UnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };
        }

        private void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            if (e.HResult == -2146233088)
            {
                // This message disables an error about the inability to connect to the websocket server.
                // As a temporary measure. Service errors should be handled in the services themselves
                if (e.InnerException.Source == "Websocket.Client")
                    return;
            }
            var messageBoxTitle = $"An unexpected error has occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nTo continue working, you need to exit the application. Can I do it now?";
                messageBoxButtons = MessageBoxButton.YesNo;
            }

            // Let the user decide if the app should die or not (if applicable).
            if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
            {
                OnAppCrashOrExit();
            }
        }

        #endregion

        #region PrivateMethods

        private static void LoadSharedFFmpegLibrary()
        {
            var ffmpegPath = AppDomain.CurrentDomain.BaseDirectory;
            Unosquare.FFME.Library.FFmpegDirectory = ffmpegPath;
        }

        private void RegisterAvalonHighlightingDefinition()
        {
            IAvalonEditService avalonService = Container.Resolve<IAvalonEditService>();
            avalonService.RegisterHighlighting(HighlightingName.AdamPython, Resource.AdamPython);
        }

        private static void InitPortableSettings()
        {
            PortableSettingsProvider.SettingsFileName = "settings.config";
            PortableSettingsProvider.ApplyProvider(Settings.Default);
        }

        #endregion
    }
}
