using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Core.Properties;
using AdamStudio.Services;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Ioc;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using System;

namespace AdamStudio.Extensions
{
    /// <summary>
    /// Extension methods for registering AdamStudio services with the
    /// Prism DI container. Grouped by functional area to keep
    /// App.RegisterTypes readable.
    ///
    /// See docs/coding-guidelines.md section 3.2 for the registration
    /// style convention.
    /// </summary>
    public static class ServiceRegistrationExtensions
    {
        /// <summary>
        /// Core infrastructure services: settings, logging, culture,
        /// file/folder management.
        /// </summary>
        public static IContainerRegistry AddCoreServices(this IContainerRegistry registry)
        {
            registry.RegisterSingleton<IServiceSettings, DefaultServiceSettings>();
            registry.RegisterSingleton<ILogWriteEventAwareService, LogWriteEventAwareService>();
            registry.RegisterSingleton<ICultureProvider, CultureProvider>();
            registry.RegisterSingleton<IFileManagmentService, FileManagmentService>();
            registry.RegisterSingleton<IFolderManagmentService, FolderManagmentService>();
            return registry;
        }

        /// <summary>
        /// Serilog logger wired into Microsoft.Extensions.Logging.
        /// Writes to the in-app log buffer (via ILogWriteEventAwareService)
        /// and to rolling daily files in logs/.
        /// </summary>
        public static IContainerRegistry AddLogging(this IContainerRegistry registry, IContainerProvider container)
        {
            ILogWriteEventAwareService logWriteEventAware = container.Resolve<ILogWriteEventAwareService>();

            Logger mainLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.DelegatingTextSink(
                    writeAction => logWriteEventAware.WriteToBuffer(writeAction),
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 10,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var loggerFactory = new SerilogLoggerFactory(mainLogger, dispose: true);
            registry.RegisterInstance<ILoggerFactory>(loggerFactory);
            registry.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));
            return registry;
        }

        /// <summary>
        /// Network communication services: TCP, UDP, WebSocket, WebApi,
        /// and the composite CommunicationProvider that ties them together.
        /// </summary>
        public static IContainerRegistry AddCommunicationServices(this IContainerRegistry registry)
        {
            registry.RegisterSingleton<ITcpClientService, TcpClientService>();
            registry.RegisterSingleton<IUdpClientService, UdpClientService>();
            registry.RegisterSingleton<IUdpServerService, UdpServerService>();
            registry.RegisterSingleton<IWebSocketClientService, WebSocketClientService>();
            registry.RegisterSingleton<IWebApiService, WebApiService>();
            registry.RegisterSingleton<ICommunicationProviderService, CommunicationProviderService>();
            registry.RegisterSingleton<IPythonRemoteRunnerService, PythonRemoteRunnerService>();
            registry.RegisterSingleton<ITcpPythonStreamServerService, TcpPythonStreamServerService>();
            registry.RegisterSingleton<IFindRobotClientService, FindRobotClientService>();
            return registry;
        }

        /// <summary>
        /// Flyout panel infrastructure: manager, state checker, and
        /// region-change awareness.
        /// </summary>
        public static IContainerRegistry AddFlyoutServices(this IContainerRegistry registry)
        {
            registry.RegisterSingleton<IFlyoutStateChecker, FlyoutStateChecker>();
            registry.RegisterSingleton<IFlyoutManager, FlyoutManager>();
            registry.RegisterSingleton<IRegionChangeAwareService, RegionChangeAwareService>();
            return registry;
        }

        /// <summary>
        /// Editor and content rendering services: AvalonEdit (code editor),
        /// WebView2 provider (Blockly), and video view provider (FFME).
        /// </summary>
        public static IContainerRegistry AddEditorServices(this IContainerRegistry registry)
        {
            registry.RegisterSingleton<IAvalonEditService, AvalonEditService>();
            registry.RegisterSingleton<IWebViewProvider, WebViewProvider>();
            registry.RegisterSingleton<IVideoViewProvider, VideoViewProvider>();
            return registry;
        }

        /// <summary>
        /// UI theme and control helper services.
        /// </summary>
        public static IContainerRegistry AddThemeServices(this IContainerRegistry registry)
        {
            registry.RegisterSingleton<IThemeManagerService, ThemeManagerService>();
            registry.RegisterSingleton<IControlHelperService, ControlHelperService>();
            return registry;
        }

        /// <summary>
        /// Status bar notification delivery (user-facing log channel).
        /// </summary>
        public static IContainerRegistry AddStatusServices(this IContainerRegistry registry)
        {
            registry.RegisterSingleton<IStatusBarNotificationDeliveryService, StatusBarNotificationDeliveryService>();
            return registry;
        }

        /// <summary>
        /// System dialog services (file open/save, folder browse via
        /// Microsoft.Win32 dialogs).
        /// </summary>
        public static IContainerRegistry AddDialogServices(this IContainerRegistry registry)
        {
            registry.RegisterSingleton<ISystemDialogService, SystemDialogService>();
            return registry;
        }

        /// <summary>
        /// Registers all AdamStudio services in the correct dependency
        /// order. Core services first (logging depends on
        /// ILogWriteEventAwareService), then the rest.
        /// </summary>
        public static IContainerRegistry AddAdamStudioServices(this IContainerRegistry registry, IContainerProvider container)
        {
            registry
                .AddCoreServices()
                .AddLogging(container)
                .AddCommunicationServices()
                .AddFlyoutServices()
                .AddEditorServices()
                .AddThemeServices()
                .AddStatusServices()
                .AddDialogServices();
            return registry;
        }
    }
}
