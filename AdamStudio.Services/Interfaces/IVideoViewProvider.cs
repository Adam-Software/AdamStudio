using System;

namespace AdamStudio.Services.Interfaces
{


    public interface IVideoViewProvider : IDisposable
    {

        public event EventHandler RaiseFrameRateUpdateEvent;

        public double FrameRate { get; set; }

        public void ClearFrameRate();

    }
}
