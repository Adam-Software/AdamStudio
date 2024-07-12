using AdamStudio.Services.Interfaces;
using Prism.Mvvm;


namespace AdamStudio.Services
{
    public class StatusBarNotificationDeliveryService : BindableBase, IStatusBarNotificationDeliveryService
    {

        #region Events

        public event ChangeProgressRingStateEventHandler RaiseChangeProgressRingStateEvent;
        public event NewCompileLogMessageEventHandler RaiseNewCompileLogMessageEvent;
        //public event NewAppLogMessageEventHandler RaiseNewAppLogMessageEvent;
        public event UpdateNotificationCounterEventHandler RaiseUpdateNotificationCounterEvent;

        #endregion

        #region ~

        public StatusBarNotificationDeliveryService() { }

        #endregion

        #region Public fields

        private bool progressRingStart;
        public bool ProgressRingStart 
        { 
            get =>   progressRingStart; 
            set 
            { 
                bool isNewValue = SetProperty(ref progressRingStart, value);

                if (isNewValue)
                    OnRaiseChangeProgressRingStateEvent(ProgressRingStart);
                
            }
        }

        private string compileLogMessage = string.Empty;
        public string CompileLogMessage 
        {
            get => compileLogMessage; 
            set 
            { 
                bool isNewValue = SetProperty(ref compileLogMessage, value);
                
                if (isNewValue)
                    OnRaiseNewCompileLogMessageEvent(CompileLogMessage);
            }
        }
       
        /*private string appLogMessage = string.Empty;
        public string AppLogMessage 
        { 
            get => appLogMessage; 
            set 
            {
                bool isNewValue = SetProperty(ref appLogMessage, value);

                if (isNewValue)
                    OnRaiseNewAppLogMessageEvent(AppLogMessage);
            } 
        }*/

        private int notificationCounter;
        public int NotificationCounter 
        { 
            get => notificationCounter;
            set 
            {
                bool isNewValue = SetProperty(ref notificationCounter, value);
            
                if (isNewValue)
                    OnRaiseUpdateNotificationCounterEvent(NotificationCounter);
            }
        }

        #endregion

        #region Public methode

        public void ResetNotificationCounter()
        {
            NotificationCounter = 0;
        }

        public void Dispose()
        {

        }

        #endregion

        #region OnRaise methods

        protected virtual void OnRaiseChangeProgressRingStateEvent(bool newState)
        {
            ChangeProgressRingStateEventHandler raiseEvent = RaiseChangeProgressRingStateEvent;
            raiseEvent?.Invoke(this, newState);
        }

        protected virtual void OnRaiseNewCompileLogMessageEvent(string message) 
        {
            NewCompileLogMessageEventHandler raiseEvent = RaiseNewCompileLogMessageEvent;
            raiseEvent?.Invoke(this, message);
        }

        /*protected virtual void OnRaiseNewAppLogMessageEvent(string message)
        {
            NewAppLogMessageEventHandler raiseEvent = RaiseNewAppLogMessageEvent;
            raiseEvent?.Invoke(this, message);
        }*/

        protected virtual void OnRaiseUpdateNotificationCounterEvent(int counter)
        {
            UpdateNotificationCounterEventHandler raiseEvent = RaiseUpdateNotificationCounterEvent;
            raiseEvent?.Invoke(this, counter);  
        }

        #endregion
    }
}
