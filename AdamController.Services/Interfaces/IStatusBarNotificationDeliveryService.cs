﻿using System;

namespace AdamController.Services.Interfaces
{
    #region Delegates

    public delegate void ChangeProgressRingStateEventHandler(object sender, bool newState);
    public delegate void NewCompileLogMessageEventHandler(object sender, string message);
    public delegate void NewAppLogMessageEventHandler(object sender, string message);
    public delegate void NewNotificationBadgeMessageEventHandler(object sender, string message);

    #endregion

    public interface IStatusBarNotificationDeliveryService : IDisposable
    {
        #region Event

        public event ChangeProgressRingStateEventHandler RaiseChangeProgressRingStateEvent;
        public event NewCompileLogMessageEventHandler RaiseNewCompileLogMessageEvent;
        public event NewAppLogMessageEventHandler RaiseNewAppLogMessageEvent;
        public event NewNotificationBadgeMessageEventHandler RaiseNewNotificationBadgeMessageEvent;

        #endregion

        #region Public methods

        public bool ProgressRingStart { get; set; }
        public string CompileLogMessage { get; set; }
        public string AppLogMessage {  get; set; }
        public string NotificationBadgeMessage { get; set; }

        #endregion
    }
}