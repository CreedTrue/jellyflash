using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using System;
using System.IO;

namespace Jellyfin.Plugin.Jellyflash
{
    public class SwfItemResolver : IItemResolver
    {
        public ResolverPriority Priority => ResolverPriority.First;

        public BaseItem ResolvePath(ItemResolveArgs args)
        {
            if (args.IsDirectory)
            {
                return null;
            }

            var extension = Path.GetExtension(args.Path);
            if (string.Equals(extension, ".swf", StringComparison.OrdinalIgnoreCase))
            {
                return new Video
                {
                    Path = args.Path,
                    Name = Path.GetFileNameWithoutExtension(args.Path)
                };
            }

            return null;
        }
    }
}
