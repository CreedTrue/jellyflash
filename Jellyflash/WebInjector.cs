using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.Jellyflash
{
    public class WebInjector : IHostedService
    {
        private readonly ILogger<WebInjector> _logger;
        private readonly IApplicationPaths _appPaths;

        public WebInjector(ILogger<WebInjector> logger, IApplicationPaths appPaths)
        {
            _logger = logger;
            _appPaths = appPaths;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (JellyflashPlugin.Instance?.Configuration.AutomaticallyInjectJavascript == true)
                {
                    InjectJavascript();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error injecting Jellyflash javascript");
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void InjectJavascript()
        {
            var webPath = _appPaths.WebPath;
            if (string.IsNullOrEmpty(webPath) || !Directory.Exists(webPath))
            {
                _logger.LogWarning("Jellyflash could not locate the web directory.");
                return;
            }

            var indexHtmlPath = Path.Combine(webPath, "index.html");
            if (!File.Exists(indexHtmlPath))
            {
                _logger.LogWarning("Jellyflash could not find index.html to inject Ruffle.");
                return;
            }

            var html = File.ReadAllText(indexHtmlPath);

            // Copy our JS file to the web path so it can be served
            var pluginJsPath = Path.Combine(webPath, "ruffle-injector.js");
            ExtractEmbeddedResource("Jellyfin.Plugin.Jellyflash.Web.ruffle-injector.js", pluginJsPath);

            var scriptTag = "<script src=\"ruffle-injector.js\"></script>";
            if (!html.Contains(scriptTag))
            {
                // Inject right before the closing body tag
                html = html.Replace("</body>", scriptTag + "\n</body>");
                File.WriteAllText(indexHtmlPath, html);
                _logger.LogInformation("Successfully injected Jellyflash (Ruffle) into index.html");
            }
            else
            {
                _logger.LogInformation("Jellyflash (Ruffle) is already injected into index.html");
            }
        }

        private void ExtractEmbeddedResource(string resourceName, string outputPath)
        {
            using var stream = typeof(WebInjector).Assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return;
            
            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }
    }
}
