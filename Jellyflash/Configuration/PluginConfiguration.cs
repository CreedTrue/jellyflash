using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Jellyflash.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public bool AutomaticallyInjectJavascript { get; set; } = true;
    }
}
