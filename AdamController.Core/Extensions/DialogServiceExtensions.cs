﻿using AdamController.Core.Dialog.Views;
using Prism.Services.Dialogs;

namespace AdamController.Core.Extensions
{
    public static class DialogServiceExtensions
    {
        public static void ShowSettingsDialog(this IDialogService dialogService)
        {
            dialogService.ShowDialog(nameof(SettingsView));
        }
    }
}