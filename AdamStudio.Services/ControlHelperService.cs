using AdamStudio.Services.ControlHelperServiceDependency;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Prism.Mvvm;
using System;

namespace AdamStudio.Services
{
    public class ControlHelperService : BindableBase, IControlHelperService
    {
        public event EventHandler RaiseBlocklyColumnWidthChangeEvent;
        public event EventHandler IsVideoShowChangeEvent;

        public ControlHelperService(IServiceProvider serviceProvider)
        {
            IsShowVideo = serviceProvider.GetService<IServiceSettings>().ControlHelperSettings.IsVideoShownLastValue;
        }

        private double mainGridActualWidth = double.NaN;
        public double MainGridActualWidth
        {
            get => mainGridActualWidth;  
            set => SetProperty(ref mainGridActualWidth, value);
        }

        private double blocklyColumnActualWidth = double.NaN;
        public double BlocklyColumnActualWidth
        {
            get { return blocklyColumnActualWidth;}
            set 
            {
                bool isNewValue = SetProperty(ref blocklyColumnActualWidth, value);

                if (isNewValue)
                    UpdateCurrentBlocklyViewMode();
                
            }
        }

        private double blocklyColumnWidth = double.NaN;
        public double BlocklyColumnWidth
        {
            get { return blocklyColumnWidth; }
            set
            {
                bool isNewValue = SetProperty(ref blocklyColumnWidth, value);

                if (isNewValue)
                    OnRaiseBlocklyColumnWidthChangeEvent();
                
            }
        }

        private BlocklyViewMode currentBlocklyViewMode;

        public BlocklyViewMode CurrentBlocklyViewMode
        {
            get => currentBlocklyViewMode;
            set
            {
                bool isNewValue = SetProperty(ref currentBlocklyViewMode, value);

                if (isNewValue)
                    UpdateBlocklyColumnWidth();
            }
        }

        private bool isShowVideo;

       

        public bool IsShowVideo
        {
            get =>  isShowVideo;
            set
            {
                bool isNewValue = SetProperty(ref isShowVideo, value);

                if (isNewValue)
                    OnRaiseIsVideoShowChangeEvent();
            }
        }

        private void UpdateBlocklyColumnWidth()
        {
            var dividedScreen = MainGridActualWidth/2;

            switch (CurrentBlocklyViewMode)
            {
                case BlocklyViewMode.Hidden:
                    BlocklyColumnWidth = 0;
                    break;
                case BlocklyViewMode.MiddleScreen:
                    BlocklyColumnWidth = dividedScreen;
                    break;
                case BlocklyViewMode.FullScreen:
                    BlocklyColumnWidth = MainGridActualWidth;
                    break;
            }
        }

        private void UpdateCurrentBlocklyViewMode()
        {
            if (BlocklyColumnActualWidth >= MainGridActualWidth - 10)
            {
                CurrentBlocklyViewMode = BlocklyViewMode.FullScreen;
                return;
            }

            if (BlocklyColumnActualWidth == 0)
            {
                CurrentBlocklyViewMode = BlocklyViewMode.Hidden;
                return;
            }

            CurrentBlocklyViewMode = BlocklyViewMode.MiddleScreen;
        }

        public void Dispose()
        {
        }

        protected virtual void OnRaiseBlocklyColumnWidthChangeEvent()
        {
            
            RaiseBlocklyColumnWidthChangeEvent?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseIsVideoShowChangeEvent()
        {
            
            IsVideoShowChangeEvent?.Invoke(this, EventArgs.Empty);

        }
    }
}
