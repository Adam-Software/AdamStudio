#region system

using System;
using System.IO;
using System.Windows;

#endregion

#region prism

using Prism.Ioc;
using Prism.DryIoc;
using Prism.Modularity;
using DryIoc;

#endregion

#region mahapps

using MahApps.Metro.Controls;

#endregion

#region other

using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;

using AdamStudio.Controls.CustomControls.RegionAdapters;
using AdamStudio.Modules.ContentRegion;
using AdamStudio.Modules.FlyoutsRegion;
using AdamStudio.Modules.MenuRegion;
using AdamStudio.Modules.StatusBarRegion;
using AdamStudio.Modules.ToolBarRegion;
using AdamStudio.Core.Properties;
using AdamStudio.Services.Interfaces;
using AdamStudio.Views;
using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Services;
using AdamStudio.Core;
using AdamStudio.Extensions;
using Bluegrams.Application;
using Prism.Navigation.Regions;

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
            containerRegistry.AddAdamStudioServices(Container);
            RegisterAvalonHighlightingDefinition();
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
            moduleCatalog.AddModule<FlyoutsRegionModule>();
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
            // COR_E_EXCEPTION (0x80131500) — the base CLR exception HResult.
            // Websocket.Client connection failures surface with this HResult
            // and an InnerException whose Source is "Websocket.Client". They
            // are noisy and non-fatal (the WebSocketClientService already
            // handles reconnection), so we suppress the crash dialog for
            // them. Service-level errors should be handled in the services
            // themselves.
            const int cClrExceptionHResult = unchecked((int)0x80131500);

            if (e.HResult == cClrExceptionHResult
                && e.InnerException?.Source == "Websocket.Client")
            {
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
            // FFME expects the FFmpeg shared libraries (avcodec-*.dll, etc.)
            // to be in FFmpegDirectory. Placing them in the app root pollutes
            // the output folder; prefer a "ffmpeg" subdirectory. Fall back to
            // the base directory if the subdirectory does not exist (e.g.
            // when running from the IDE with a flat layout).
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string subDir = Path.Combine(baseDir, "ffmpeg");

            string ffmpegPath = Directory.Exists(subDir) ? subDir : baseDir;

            try
            {
                Unosquare.FFME.Library.FFmpegDirectory = ffmpegPath;
            }
            catch (Exception ex)
            {
                // FFmpeg loading is best-effort — the app can still run
                // without video playback. Log and continue.
                System.Diagnostics.Debug.WriteLine(
                    $"Failed to set FFmpegDirectory to '{ffmpegPath}': {ex.Message}");
            }
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
