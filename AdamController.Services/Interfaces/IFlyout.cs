﻿using System.Windows.Input;
using System;
using AdamController.Services.FlayoutsRegionEventAwareServiceDependency;

namespace AdamController.Services.Interfaces
{
    
    public interface IFlyout
    {
        
        public event EventHandler<FlyoutEventArgs> OnClosed;

        public event EventHandler<FlyoutEventArgs> OnOpened;

        public event EventHandler<FlyoutEventArgs> OnOpenChanged;

        public string Position { get; set; }

        public string Header { get; set; }

        public string Theme { get; set; }

        public bool IsModal { get; set; }

        public bool AreAnimationsEnabled { get; set; }

        public bool AnimateOpacity { get; set; }

        public ICommand CloseCommand { get; set; }

        public MouseButton ExternalCloseButton { get; set; }

        public bool CloseButtonIsCancel { get; set; }

        public bool IsPinned { get; set; }

        public bool CanClose(FlyoutParameters flyoutParameters);

        public void Close(FlyoutParameters flyoutParameters);

        public void Close();

        public bool CanOpen(FlyoutParameters flyoutParameters);

        public void Open(FlyoutParameters flyoutParameters);

        public void Open();
    }
}
