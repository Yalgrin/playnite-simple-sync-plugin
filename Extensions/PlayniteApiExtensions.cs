using Playnite.SDK;

namespace SimpleSyncPlugin.Extensions
{
    public static class PlayniteApiExtensions
    {
        public static string GetLocalizedString(this IPlayniteAPI api, string key)
        {
            return api.Resources.GetString(key);
        }
    }
}