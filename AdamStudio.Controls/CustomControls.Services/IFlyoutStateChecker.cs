namespace AdamStudio.Controls.CustomControls.Services
{

    public interface IFlyoutStateChecker
    {
        public event EventHandler IsFlyoutsOpenedStateChangeEvent;

        public bool IsFlyoutsOpened { get; set; }
    }
}
