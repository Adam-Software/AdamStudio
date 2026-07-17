using AdamStudio.Services.Interfaces;
using AdamStudio.Services.SystemDialogServiceDependency;
using Microsoft.Win32;
using Prism.Dialogs;
using System.IO;

namespace AdamStudio.Services
{
    public class SystemDialogService : ISystemDialogService
    {
        public OpenFileDialogResult ShowOpenFileDialog(IDialogParameters parameters)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                AddToRecent = true,
                Filter = "SupportedFiles (*.xml,*.py)|*.xml;*.py|All files (*.*)|*.*",
                Title = parameters.GetValue<string>(DialogParametrsKeysName.TitleParametr),
                InitialDirectory = parameters.GetValue<string>(DialogParametrsKeysName.InitialDirectoryParametr)
            };

            bool? isOpen = openFileDialog.ShowDialog();
            OpenFileDialogResult result = new();
            
            if (isOpen == true)
            {
                string filePathName = openFileDialog.FileName;
                SupportFileType fileType = DetermineOpenFileType(filePathName);

                result.IsOpenFileCanceled = false;
                result.OpenFilePath = filePathName;
                result.OpenFileType = fileType;
                result.IsSupportedFileTypeOpened = fileType != SupportFileType.Undefined;
            }

            return result;
        }

        public SaveFileDialogResult ShowSaveFileDialog(IDialogParameters parameters)
        {
            SupportFileType savedFileType = parameters.GetValue<SupportFileType>(DialogParametrsKeysName.SavedFileTypeParametr);
            string title = parameters.GetValue<string>(DialogParametrsKeysName.TitleParametr);
            string initialDirectory = parameters.GetValue<string>(DialogParametrsKeysName.InitialDirectoryParametr);

            SaveFileDialogParam fileDialogParam = DetermineSaveFileDialogParam(savedFileType);

            SaveFileDialog saveFileDialog = new()
            {
                OverwritePrompt = true,
                AddToRecent = true,
                Title = title,
                InitialDirectory = initialDirectory,

                FileName = fileDialogParam.FileName,
                DefaultExt = fileDialogParam.FileExtension,
                Filter = fileDialogParam.DialogFilter
            };

            bool? isOpen = saveFileDialog.ShowDialog();
            SaveFileDialogResult result = new();

            if (isOpen == true)
            {
                result.IsSaveFileCanceled = false;
                result.SavedFilePath = saveFileDialog.FileName;
            }

            return result;
        }

        public OpenFolderDialogResult ShowOpenFolderDialog(IDialogParameters parameters)
        {
            OpenFolderDialog openFolderDialog = new()
            {
                AddToRecent = false,
                Multiselect = false,
                Title = parameters.GetValue<string>(DialogParametrsKeysName.TitleParametr),
                InitialDirectory = parameters.GetValue<string>(DialogParametrsKeysName.InitialDirectoryParametr)
            };

            bool? isOpen = openFolderDialog.ShowDialog();
            OpenFolderDialogResult result = new();

            if (isOpen == true)
            {
                result.IsSelectFolderCanceled = false;
                result.SelectedFolderPath = openFolderDialog.FolderName;
            }

            return result;
        }

        private static SaveFileDialogParam DetermineSaveFileDialogParam(SupportFileType savedFileType)
        {
            SaveFileDialogParam saveFileDialogParam = new();

            switch (savedFileType)
            {
                case SupportFileType.Script:
                    saveFileDialogParam.DialogFilter = "Python file (.py)|*.py|All files|*.*";
                    saveFileDialogParam.FileExtension = ".py";
                    saveFileDialogParam.FileName = "new_script";
                    break;
                case SupportFileType.Workspace:
                    saveFileDialogParam.DialogFilter = "Workspace file (.xml)|*.xml|All files|*.*";
                    saveFileDialogParam.FileExtension = ".xml";
                    saveFileDialogParam.FileName = "new_workspace";
                    break;
            }

            return saveFileDialogParam;
        }

        private static SupportFileType DetermineOpenFileType(string fileName)
        {
            string openExt = Path.GetExtension(fileName);

            SupportFileType result = openExt switch
            {
                ".xml" => SupportFileType.Workspace,
                ".py" => SupportFileType.Script,
                _ => SupportFileType.Undefined,
            };

            return result;
        }

        public void Dispose()
        {
            
        }
    }
}
