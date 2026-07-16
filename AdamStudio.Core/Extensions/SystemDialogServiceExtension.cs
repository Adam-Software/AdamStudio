using AdamStudio.Services.Interfaces;
using AdamStudio.Services.SystemDialogServiceDependency;
using Prism.Dialogs;
using System;
using System.Threading.Tasks;

namespace AdamStudio.Core.Extensions
{
    public static class SystemDialogServiceExtension
    {
        public static Task<OpenFileDialogResult> ShowOpenFileDialog(this ISystemDialogService dialogService, IDialogParameters parameters)
        {
            var task = new TaskCompletionSource<OpenFileDialogResult>();

            try
            {
                dialogService.ShowOpenFileDialog(parameters);
            }
            catch (Exception ex)
            {
                task.SetException(ex);
            }

            return task.Task;

        }
    }
}
