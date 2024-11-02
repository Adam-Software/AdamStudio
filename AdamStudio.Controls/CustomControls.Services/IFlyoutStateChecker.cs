namespace AdamStudio.Controls.CustomControls.Services
{
    public delegate void IsFlyoutsOpenedStateChangeEventHandler(object sender);

    public interface IFlyoutStateChecker
    {
        public event IsFlyoutsOpenedStateChangeEventHandler IsFlyoutsOpenedStateChangeEvent;

        public bool IsFlyoutsOpened { get; set; }
    }
}
