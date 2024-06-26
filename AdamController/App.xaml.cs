﻿#region system

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

#region innerhit

using AdamController.Views;
using AdamController.Modules.MenuRegion;
using AdamController.Modules.ContentRegion;
using AdamController.Services;
using AdamController.Modules.StatusBarRegion;
using AdamController.Modules.FlayoutsRegion;
using AdamController.Services.Interfaces;
using AdamController.Controls.CustomControls.Services;
using AdamController.Controls.CustomControls.RegionAdapters;

#endregion

#region mahapps

using MahApps.Metro.Controls;

#endregion

#region other

using AdamController.Core.Properties;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using AdamController.Services.TcpClientDependency;
using System.ComponentModel;
using AdamController.Core.Dialog.ViewModels;
using AdamController.Core.Dialog.Views;
using Microsoft.Win32;
using System.IO;

#endregion

namespace AdamController
{
    public partial class App : PrismApplication
    {
        #region ~

        public App()
        {
            Subscribe();
            LoadSharedFFmpegLibrary();
        }

        #endregion

        protected override Window CreateShell()
        {
            MainWindow window = Container.Resolve<MainWindow>();
            return window;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IFlyoutStateChecker, FlyoutStateChecker>();

            containerRegistry.RegisterSingleton<ICultureProvider, CultureProvider>();
            containerRegistry.RegisterSingleton<IFileManagmentService, FileManagmentService>();
            containerRegistry.RegisterSingleton<IFolderManagmentService, FolderManagmentService>();

            containerRegistry.RegisterSingleton<IAvalonEditService>(containerRegistry =>
            {
                IFileManagmentService fileManagment = containerRegistry.Resolve<IFileManagmentService>();
                AvalonEditService avalonService = new(fileManagment);
                return avalonService;
            });

            containerRegistry.RegisterSingleton<IWebViewProvider, WebViewProvider>();
            containerRegistry.RegisterSingleton<ISubRegionChangeAwareService, SubRegionChangeAwareService>();
            containerRegistry.RegisterSingleton<IStatusBarNotificationDeliveryService, StatusBarNotificationDeliveryService>();

            containerRegistry.RegisterSingleton<IFlyoutManager>(containerRegistry =>
            {
                DryIoc.IContainer container = containerRegistry.GetContainer();
                IRegionManager regionManager = containerRegistry.Resolve<IRegionManager>();

                return new FlyoutManager(container, regionManager);
            });

            containerRegistry.RegisterSingleton<ITcpClientService>(() =>
            {
                TcpClientOption option = new()
                {
                    ReconnectCount = Settings.Default.ReconnectQtyComunicateTcpClient,
                    ReconnectTimeout = Settings.Default.ReconnectTimeoutComunicateTcpClient
                };

                string ip = Settings.Default.ServerIP;
                int port = Settings.Default.TcpConnectStatePort;
                
                TcpClientService client;

                if (!string.IsNullOrEmpty(ip))
                {
                    client = new(ip, port, option);
                }
                else
                {
                    client = new("127.0.0.1", port, option);
                }

                
                return client;
            });

            containerRegistry.RegisterSingleton<IUdpClientService>(() =>
            {
                IPAddress ip = IPAddress.Any;
                int port = int.Parse(Settings.Default.MessageDataExchangePort);

                UdpClientService client = new(ip, port)
                {
                    OptionDualMode = true,
                    OptionReuseAddress = true
                };

                return client;
            });

            containerRegistry.RegisterSingleton<IUdpServerService>(() =>
            {
                IPAddress ip = IPAddress.Any;
                int port = Settings.Default.LogServerPort;

                UdpServerService server = new(ip, port)
                {
                    OptionDualMode = true,
                    OptionReuseAddress = true
                };

                return server;
            });

            containerRegistry.RegisterSingleton<IWebSocketClientService>(() =>
            {
                string ip = Settings.Default.ServerIP;
                int port = Settings.Default.SoketServerPort;
                Uri uri;

                if (!string.IsNullOrEmpty(ip))
                {
                    uri = new($"ws://{ip}:{port}/adam-2.7/movement");
                }
                else
                {
                    uri = new($"ws://127.0.0.1:9001/adam-2.7/movement");
                }

                // debug, use only with debug server, which runs separately, not as a service
                //Uri uri = new($"ws://{Settings.Default.ServerIP}:9001/adam-2.7/movement");

                // work in production, connect to socket-server run as service
                // Uri uri = new($"ws://{ip}:{port}/adam-2.7/movement");

                WebSocketClientService client = new(uri);
                return client;
            });

            containerRegistry.RegisterSingleton<IWebApiService>(() =>
            {
                string ip = Settings.Default.ServerIP;
                int port = Settings.Default.ApiPort;
                string login = Settings.Default.ApiLogin;
                string password = Settings.Default.ApiPassword;

                if (string.IsNullOrEmpty(ip))
                {
                    ip = "127.0.0.1";
                }
                
                WebApiService client = new(ip, port, login, password);
                return client;

            });

            containerRegistry.RegisterSingleton<ICommunicationProviderService>(containerRegistry =>
            {
                ITcpClientService tcpClientService = containerRegistry.Resolve<ITcpClientService>();
                IUdpClientService udpClientService = containerRegistry.Resolve<IUdpClientService>();
                IUdpServerService udpServerService = containerRegistry.Resolve<IUdpServerService>();
                IWebSocketClientService socketClientService = containerRegistry.Resolve<IWebSocketClientService>();
                
                CommunicationProviderService communicationProvider = new(tcpClientService, udpClientService, udpServerService, socketClientService);

                return communicationProvider;
            });

            containerRegistry.RegisterSingleton<IPythonRemoteRunnerService>(containerRegistry =>
            {
                IUdpClientService udpClient = containerRegistry.Resolve<IUdpClientService>();

                PythonRemoteRunnerService remoteRunnerService = new(udpClient);
                return remoteRunnerService;
            });

            containerRegistry.RegisterSingleton<IThemeManagerService, ThemeManagerService>();
            containerRegistry.RegisterSingleton<IControlHelper>(containerRegistry =>
            {
                bool isVideoShowLastValue = Settings.Default.ShowVideo;
                return new ControlHelper(isVideoShowLastValue);
            });

            containerRegistry.RegisterSingleton<IVideoViewProvider, VideoViewProvider>();

            RegisterDialogs(containerRegistry);
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            OnAppCrashOrExit();

            base.OnExit(e);
        }

        private void OnAppCrashOrExit()
        {
            Unsubscribe();
            DisposeServices();
            Current.Shutdown();
        }

        /// <summary>
        /// The priority of resource release is important!
        /// FirstRun/LastStop
        /// </summary>
        private void DisposeServices()
        {
            Container.Resolve<IThemeManagerService>().Dispose();
            Container.Resolve<IFileManagmentService>().Dispose();
            Container.Resolve<IFolderManagmentService>().Dispose();
            Container.Resolve<IAvalonEditService>().Dispose();

            Container.Resolve<ISubRegionChangeAwareService>().Dispose();
            Container.Resolve<IStatusBarNotificationDeliveryService>().Dispose();
            Container.Resolve<IWebViewProvider>().Dispose();
            Container.Resolve<ISystemDialogService>().Dispose();

            Container.Resolve<IPythonRemoteRunnerService>().Dispose();
            Container.Resolve<ICommunicationProviderService>().Dispose();
            Container.Resolve<IWebApiService>().Dispose();
            Container.Resolve<ICultureProvider>().Dispose();

            Container.Resolve<IControlHelper>().Dispose();
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

        #region On raise event
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
            Dispatcher.UnhandledException += (sender, args) =>
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

        #endregion
    }
}
