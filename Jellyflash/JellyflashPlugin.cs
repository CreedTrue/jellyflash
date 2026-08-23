using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Jellyflash.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Jellyflash
{
    public class JellyflashPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public override string Name => "Jellyflash";
        public override Guid Id => Guid.Parse("f609e9e1-6d9b-4b13-9a43-074fcbb321a4");
        
        public JellyflashPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public static JellyflashPlugin? Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = this.Name,
                    EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
                }
            };
        }
    }
}
