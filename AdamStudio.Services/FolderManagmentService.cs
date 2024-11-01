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
        #region Var

        private readonly string mAssemblyTitle = Assembly.GetEntryAssembly().GetName().Name;

        #endregion

        #region ~

        public FolderManagmentService(IServiceProvider serviceProvider) 
        {
            ILogger<FolderManagmentService> logger = serviceProvider.GetService<ILogger<FolderManagmentService>>();
            logger.LogTrace("Init FolderManagmentService");
        }

        #endregion

        #region Public fields

        public string MyDocumentsUserDir => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string SpecialProgramDocumentsDir => MyDocumentsUserDir + Path.DirectorySeparatorChar + "AdamStudio";

        public string SavedWorkspaceDocumentsDir => SpecialProgramDocumentsDir + Path.DirectorySeparatorChar + "MyWorkspaces";

        //public string SavedToolboxDocumentsDir => SpecialProgramDocumentsDir + Path.DirectorySeparatorChar + "MyToolboxes";

        //public string SavedUserCustomBlocksDocumentsDir => SpecialProgramDocumentsDir + Path.DirectorySeparatorChar + "MyBlocks";

        public string SavedUserScriptsDocumentsDir => SpecialProgramDocumentsDir + Path.DirectorySeparatorChar + "MyScripts";

        //public string SavedResultsNetworkTestsDir => SpecialProgramDocumentsDir + Path.DirectorySeparatorChar + "NetworkResultsTests";

        public string DirAppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + mAssemblyTitle;

        public string DirFileAppSessionData => Path.Combine(DirAppData, string.Format(CultureInfo.InvariantCulture, "{0}.App.session", mAssemblyTitle));

        public string CommonDirAppData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar + Assembly.GetEntryAssembly().GetName().Name;

        #endregion

        #region Public methods

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

                //if (!Directory.Exists(SavedToolboxDocumentsDir))
                //{
                //    _ = Directory.CreateDirectory(SavedToolboxDocumentsDir);
                //}
                //if (!Directory.Exists(SavedUserCustomBlocksDocumentsDir))
                //{
                //    _ = Directory.CreateDirectory(SavedUserCustomBlocksDocumentsDir);
                //}
                if (!Directory.Exists(SavedUserScriptsDocumentsDir))
                {
                    _ = Directory.CreateDirectory(SavedUserScriptsDocumentsDir);
                }
                //if (!Directory.Exists(SavedResultsNetworkTestsDir))
                //{
                //    _ = Directory.CreateDirectory(SavedResultsNetworkTestsDir);
                //}
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Dispose(){}

        #endregion
    }
}
