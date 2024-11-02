
namespace AdamStudio.Controls.CustomControls.Services
{
    public class FlyoutStateChecker : IFlyoutStateChecker
    {
        public event IsFlyoutsOpenedStateChangeEventHandler IsFlyoutsOpenedStateChangeEvent;

        private bool isNotificationFlyoutOpened;
        public bool IsFlyoutsOpened 
        { 
            get {  return isNotificationFlyoutOpened; }
            set 
            {  
                if (isNotificationFlyoutOpened == value) 
                    return;

                isNotificationFlyoutOpened = value;
                OnNotificationFlyoutOpenedStateChangeEvent();
            } 
        }

        protected void OnNotificationFlyoutOpenedStateChangeEvent()
        {
            IsFlyoutsOpenedStateChangeEventHandler raiseEvent = IsFlyoutsOpenedStateChangeEvent;
            raiseEvent?.Invoke(this);
        }
    }
}
