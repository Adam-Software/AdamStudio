﻿using AdamController.Core.Helpers;
using AdamController.ViewModels.HamburgerMenu;
using AdamController.WebApi.Client.v1;
using AdamController.WebApi.Client.v1.ResponseModel;
using MessageDialogManagerLib;
using Prism.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace AdamController.Modules.ContentRegion.ViewModels
{
    public class ScriptEditorControlView : HamburgerMenuItemView
    {
        public static Action<string> AppLogStatusBarAction { get; set; }

        private bool mIsWarningStackOwerflowAlreadyShow;
        private readonly IMessageDialogManager IDialogManager;

        public ScriptEditorControlView() 
        {
            IDialogManager = new MessageDialogManagerMahapps(Application.Current);

            InitAction();
            PythonExecuteEvent();
        }


        /*public ScriptEditorControlView(HamburgerMenuView hamburgerMenuView = null) : base(hamburgerMenuView)
        {
            IDialogManager = new MessageDialogManagerMahapps(Application.Current);

            InitAction();
            PythonExecuteEvent();
        }*/

        #region Python execute event

        private void PythonExecuteEvent()
        {
            PythonScriptExecuteHelper.OnExecuteStartEvent += (message) =>
            {
                //if (MainWindowViewModel.GetSelectedPageIndex != 1)
                //    return;

                ResultTextEditor = string.Empty;
                IsCodeExecuted = true;
                mIsWarningStackOwerflowAlreadyShow = false;
                ResultTextEditor += message;
            };

            PythonScriptExecuteHelper.OnStandartOutputEvent += (message) => 
            {
                //if (MainWindowViewModel.GetSelectedPageIndex != 1)
                //    return;

                if (ResultTextEditorLength > 10000)
                {
                    if (!mIsWarningStackOwerflowAlreadyShow)
                    {
                        ResultTextEditor += "\nДальнейший вывод результата, приведет к переполнению буфера, поэтому будет скрыт.";
                        ResultTextEditor += "\nПрограмма продолжает выполняться в неинтерактивном режиме.";
                        ResultTextEditor += "\nДля остановки нажмите \"Stop\". Или дождитесь завершения.";

                        mIsWarningStackOwerflowAlreadyShow = true;
                    }

                    return;
                }

                ResultTextEditor += message;
            };
            
            PythonScriptExecuteHelper.OnExecuteFinishEvent += (message) =>
            {
                //if (MainWindowViewModel.GetSelectedPageIndex != 1)
                //    return;

                IsCodeExecuted = false;
                ResultTextEditor += message;
            };
        }

        #endregion

        #region Source text editor

        private string sourceTextEditor;
        public string SourceTextEditor
        {
            get => sourceTextEditor;
            set
            {
                if (value == sourceTextEditor) return;

                sourceTextEditor = value;
                SetProperty(ref sourceTextEditor, value);
            }
        }

        private string selectedText;
        public string SelectedText
        {
            get => selectedText;
            set
            {
                if (value == selectedText) return;

                selectedText = value;
                SetProperty(ref selectedText, value);
            }
        }

        #endregion

        #region SendSourceToScriptEditor Action

        private void InitAction()
        {
            if(ScratchControlView.SendSourceToScriptEditor == null)
            {
                ScratchControlView.SendSourceToScriptEditor = new Action<string>(source => SourceTextEditor = source);
            }
        }

        #endregion

        #region Result text editor

        private string resultTextEditor;
        public string ResultTextEditor
        {
            get => resultTextEditor;
            set
            {
                if (value == resultTextEditor) return;

                resultTextEditor = value;
                ResultTextEditorLength = value.Length;

                SetProperty(ref resultTextEditor, value);
            }
        }

        #endregion

        #region ResultTextEditorLength

        private int resultTextEditorLength;
        public int ResultTextEditorLength
        {
            get => resultTextEditorLength;
            set
            {
                if (value == resultTextEditorLength) return;

                resultTextEditorLength = value;
                SetProperty (ref resultTextEditorLength, value);
            }
        }

        #endregion

        #region IsCodeExecuted field

        private bool isCodeExecuted = false;
        public bool IsCodeExecuted
        {
            get => isCodeExecuted;
            set
            {
                if (value == isCodeExecuted) return;
                isCodeExecuted = value;

                SetProperty(ref isCodeExecuted, value);
            }
        }

        #endregion

        #region ResultTextEditorError

        private string resultTextEditorError;
        public string ResultTextEditorError
        {
            get => resultTextEditorError;
            set
            {
                if (value == resultTextEditorError) return;

                if(value.Length > 0)
                    resultTextEditorError = $"Error: {value}";
                else
                    resultTextEditorError = value;

                SetProperty(ref resultTextEditorError, value);
            }
        }

        #endregion

        #region Command

        #region Run code button

        private DelegateCommand runCode;
        public DelegateCommand RunCode => runCode ??= new DelegateCommand(async () =>
        {
            ResultTextEditorError = string.Empty;
            ExtendedCommandExecuteResult executeResult = new();

            try
            {
                if(ComunicateHelper.TcpClientIsConnected)
                {
                    var command = new WebApi.Client.v1.RequestModel.PythonCommand
                    {
                        Command = SourceTextEditor
                    };
                    executeResult = await BaseApi.PythonExecuteAsync(command);
                }
                
            }
            catch (Exception ex)
            {
                ResultTextEditorError = ex.Message.ToString();
            }

            if (Core.Properties.Settings.Default.ChangeExtendedExecuteReportToggleSwitchState)
            {
                ResultTextEditor += "Отчет о инициализации процесса программы\n" +
                 "======================\n" +
                 $"Начало инициализации: {executeResult.StartTime}\n" +
                 $"Завершение инициализации: {executeResult.EndTime}\n" +
                 $"Общее время инициализации: {executeResult.RunTime}\n" +
                 $"Код выхода: {executeResult.ExitCode}\n" +
                 $"Статус успешности инициализации: {executeResult.Succeesed}" +
                 "\n======================\n";
            }

            if (!string.IsNullOrEmpty(executeResult?.StandardError))
                ResultTextEditor += $"Ошибка: {executeResult?.StandardError}" +
                    "\n======================";

        }, () => !string.IsNullOrEmpty(SourceTextEditor));

        #endregion

        private DelegateCommand copyToClipboard;
        public DelegateCommand CopyToClipboard => copyToClipboard ??= new DelegateCommand(() =>
        {
            if (SelectedText == null)
                return;

            Clipboard.SetText(SelectedText);
        });

        private DelegateCommand cutToClipboard;
        public DelegateCommand CutToClipboard => cutToClipboard ??= new DelegateCommand(() =>
        {
            if (SourceTextEditor == null)
                return;

            Clipboard.SetText(SourceTextEditor);
            SourceTextEditor = string.Empty;
        });

        private DelegateCommand pasteFromClipboard;
        public DelegateCommand PasteFromClipboard => pasteFromClipboard ??= new DelegateCommand(() =>
        {
            string text = Clipboard.GetText();
            SourceTextEditor += text;
        });

        private DelegateCommand stopExecute;
        public DelegateCommand StopExecute => stopExecute ??= new DelegateCommand(async () =>
        {
            try
            {
                await BaseApi.StopPythonExecute();
            }
            catch(Exception ex)
            {
                ResultTextEditorError = ex.Message; 
            }
           
        });

        private DelegateCommand cleanExecuteEditor;
        public DelegateCommand CleanExecuteEditor => cleanExecuteEditor ??= new DelegateCommand(async () =>
        {
            await Task.Run(() =>
            {
                ResultTextEditorError = string.Empty;
                ResultTextEditor = string.Empty; 
            });
        });

        #region OpenFileDialogCommand

        private DelegateCommand showOpenFileDialogCommand;
        public DelegateCommand ShowOpenFileDialogCommand => showOpenFileDialogCommand ??= new DelegateCommand(async () =>
        {
            if (IDialogManager.ShowFileBrowser("Выберите файл с исходным кодом программы", 
                    Core.Properties.Settings.Default.SavedUserScriptsFolderPath, "PythonScript file (.py)|*.py|Все файлы|*.*"))
            {
                string path = IDialogManager.FilePath;
                if (path == "") return;

                string pythonProgram = await FileHelper.ReadTextAsStringAsync(path);
                SourceTextEditor = pythonProgram;

                AppLogStatusBarAction($"Файл {path} загружен");
            }
            else
            {
                AppLogStatusBarAction("Файл c исходным кодом не выбран");
            }
        });

        #endregion

        #region ShowSaveFileDialogCommand

        private DelegateCommand showSaveFileDialogCommand;
        public DelegateCommand ShowSaveFileDialogCommand => showSaveFileDialogCommand ??= new DelegateCommand(async () =>
        {
            string pythonProgram = SourceTextEditor;

            if (IDialogManager.ShowSaveFileDialog("Сохранить файл программы", Core.Properties.Settings.Default.SavedUserScriptsFolderPath,
                "new_program", ".py", "PythonScript file (.py)|*.py|Все файлы|*.*"))
            {
                string path = IDialogManager.FilePathToSave;
                await FileHelper.WriteAsync(path, pythonProgram);

                AppLogStatusBarAction($"Файл {IDialogManager.FilePathToSave} сохранен");
            }
            else
            {
                AppLogStatusBarAction("Файл не сохранен");
            }
        }, () => SourceTextEditor?.Length > 0);

        #endregion

        #endregion
    }
}