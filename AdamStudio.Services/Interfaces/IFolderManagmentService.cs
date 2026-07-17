using System;

namespace AdamStudio.Services.Interfaces
{
    public interface IFolderManagmentService : IDisposable
    {
        public string AssemblyTitle { get; }
        public string MyDocumentsUserDir { get; }

        public string SpecialProgramDocumentsDir { get; }

        public  string SavedWorkspaceDocumentsDir { get; }

        public  string SavedUserScriptsDocumentsDir { get; }

        public string DirAppData { get; }

        public string DirFileAppSessionData { get; }

        public string CommonDirAppData { get; }

        public bool CreateAppDataFolder();
    }
}
