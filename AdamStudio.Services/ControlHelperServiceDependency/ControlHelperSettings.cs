namespace AdamStudio.Services.ControlHelperServiceDependency
{
    public class ControlHelperSettings
    {
        public ControlHelperSettings(bool isVideoShownLastValue) 
        {
            IsVideoShownLastValue = isVideoShownLastValue;
        }

        public bool IsVideoShownLastValue { get; }
    }
}
