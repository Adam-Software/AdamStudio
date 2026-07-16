
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;

namespace AdamStudio.Core.Mvvm
{
    public class DialogViewModelBase : BindableBase, IDialogAware
    {
        public string Title { get; protected set; } = "DefaultTitle";

        #region Command

        private DelegateCommand<string> mCloseDialogCommand;
        public DelegateCommand<string> CloseDialogCommand => mCloseDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        DialogCloseListener IDialogAware.RequestClose { get; }

        #endregion

        #region Navigation


        //public event Action<IDialogResult> RequestClose;
        //public DialogCloseListener RequestClose { get; }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose.Invoke(dialogResult);
        }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {

        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("Title");
        }

        public virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new DialogResult(result));
        }

        #endregion

    }
}
