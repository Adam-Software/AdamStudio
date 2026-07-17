using System;

namespace AdamStudio.Services.Interfaces
{

    public delegate void FrameRateUpdateEventHandler(object sender);

    public interface IVideoViewProvider : IDisposable
    {

        public event FrameRateUpdateEventHandler RaiseFrameRateUpdateEvent;

        public double FrameRate { get; set; }

        public void ClearFrameRate();

    }
}
