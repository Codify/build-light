namespace Codify.VisualStudioOnline.BuildLight.Extensions
{
    public static class SettingsExtensions
    {
        public static bool IsLoaded(this Settings settings)
        {
            return (settings != null) && settings.IsLoaded;
        }

        public static bool CanConnect(this Settings settings)
        {
            return (settings != null) && settings.CanConnectTo;
        }
    }
}
