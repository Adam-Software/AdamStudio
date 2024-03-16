﻿using AdamController.Core.Dialog.Views;
using Prism.Services.Dialogs;

namespace AdamController.Core.Extensions
{
    public static class DialogServiceExtension
    {
        public static void ShowSettingsDialog(this IDialogService dialogService)
        {
            dialogService.ShowDialog(nameof(SettingsView));
        }

        public static void ShowNetworkTestDialog(this IDialogService dialogService)
        {
            dialogService.ShowDialog(nameof(NetworkTestView));
        }
    }
}