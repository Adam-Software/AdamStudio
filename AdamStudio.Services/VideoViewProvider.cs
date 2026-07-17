using AdamStudio.Services.Interfaces;
using Prism.Mvvm;
using System;

namespace AdamStudio.Services
{
    public class VideoViewProvider : BindableBase, IVideoViewProvider
    {
        public event EventHandler RaiseFrameRateUpdateEvent;
        private double frameRate = double.NaN;

        // set frame rate in view, get frame rate in view model
        public double FrameRate
        {
            get { return frameRate; }
            set 
            { 
                var isNewValue = SetProperty(ref frameRate, value); 

                if(isNewValue)
                    OnRaiseFrameRateUpdateEvent();
            }
        }

        protected virtual void OnRaiseFrameRateUpdateEvent()
        {
            
            RaiseFrameRateUpdateEvent?.Invoke(this, EventArgs.Empty);
        }

        public void ClearFrameRate()
        {
            FrameRate = double.NaN;
        }

        public void Dispose()
        {
            
        }

    }
}
