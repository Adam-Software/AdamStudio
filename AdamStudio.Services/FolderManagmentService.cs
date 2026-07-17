using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace AdamStudio.Services
{
    public class FolderManagmentService : IFolderManagmentService
    {

        public FolderManagmentService(IServiceProvider serviceProvider) 
        {
            ILogger<FolderManagmentService> logger = serviceProvider.GetService<ILogger<FolderManagmentService>>();
            logger.LogTrace("Init FolderManagmentService");
        }

        public string AssemblyTitle => Assembly.GetEntryAssembly().GetName().Name;

        public string MyDocumentsUserDir => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string SpecialProgramDocumentsDir => MyDocumentsUserDir + Path.DirectorySeparatorChar + AssemblyTitle;

        public string SavedWorkspaceDocumentsDir => SpecialProgramDocumentsDir + Path.DirectorySeparatorChar + "MyWorkspaces";

        public string SavedUserScriptsDocumentsDir => SpecialProgramDocumentsDir + Path.DirectorySeparatorChar + "MyScripts";

        public string DirAppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + AssemblyTitle;

        public string DirFileAppSessionData => Path.Combine(DirAppData, string.Format(CultureInfo.InvariantCulture, "{0}.App.session", AssemblyTitle));

        public string CommonDirAppData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar + AssemblyTitle;

        public bool CreateAppDataFolder()
        {
            try
            {
                if (!Directory.Exists(DirAppData))
                {
                    _ = Directory.CreateDirectory(DirAppData);
                }

                if (!Directory.Exists(SpecialProgramDocumentsDir))
                {
                    _ = Directory.CreateDirectory(SpecialProgramDocumentsDir);
                }

                if (!Directory.Exists(SavedWorkspaceDocumentsDir))
                {
                    _ = Directory.CreateDirectory(SavedWorkspaceDocumentsDir);
                }

                if (!Directory.Exists(SavedUserScriptsDocumentsDir))
                {
                    _ = Directory.CreateDirectory(SavedUserScriptsDocumentsDir);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Dispose(){}

    }
}
