using System;

namespace AdamStudio.Services.Interfaces
{

    public delegate void ChangeProgressRingStateEventHandler(object sender, bool newState);
    public delegate void UpdateNotificationCounterEventHandler(object sender, int counter);

    public interface IStatusBarNotificationDeliveryService : IDisposable
    {

        public event ChangeProgressRingStateEventHandler RaiseChangeProgressRingStateEvent;
        public event UpdateNotificationCounterEventHandler RaiseUpdateNotificationCounterEvent;

        public bool ProgressRingStart { get; set; }
        public int NotificationCounter { get; set; }

        public void ResetNotificationCounter();

    }
}
