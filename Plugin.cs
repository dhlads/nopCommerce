using Nop.Services.Logging;
using Nop.Services.Plugins;

namespace Nop.Plugin.WholeSalePet.Customization
{
    public class Plugin : BasePlugin
    {

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public static string PluginAssemblyName => "Nop.Plugin.WholeSalePet.Customization";
        public static string PluginLocalResourcesPrefix => "plugins.wholesalepet.customization";

        public bool HideInWidgetList => false;

        private readonly ILogger _logger;

        public Plugin(
            ILogger logger
        )
        {
            _logger = logger;
        }

        public override async Task InstallAsync()
        {
            await base.InstallAsync();
            _logger.Information("Custom Fields Plugin successfully installed.");
        }

        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
            _logger.Information("Custom Fields Plugin successfully uninstalled.");
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            await base.UpdateAsync(currentVersion, targetVersion);
            _logger.Information("Custom Fields Plugin successfully updated.");
        }
    }

}
