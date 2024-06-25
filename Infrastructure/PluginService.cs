using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Plugin.WholeSalePet.Customization.Data;
using static Nop.Plugin.WholeSalePet.Customization.Data.PluginTexts;
using static Nop.Plugin.WholeSalePet.Customization.Data.PluginDefaultSettings;

namespace Nop.Plugin.WholeSalePet.Customization.Infrastructure
{
    #region Interfaces
    public interface IPluginService
    {
        IServiceCollection GetServices();

        IConfiguration GetConfiguration();

        IWebHostEnvironment GetEnvironment();

        PluginTextsObject GetPluginTexts();

        PluginDefaultsObject GetPluginDefaults();
    }

    #endregion

    #region Classes

    internal class PluginService : IPluginService
    {
        #region Fields

        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;

        #endregion

        #region Ctor

        internal PluginService(IServiceCollection services, IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;
        }
        #endregion

        #region Methods

        public IServiceCollection GetServices()
        {
            return _services;
        }

        public IConfiguration GetConfiguration()
        {
            return _configuration;
        }

        public IWebHostEnvironment GetEnvironment()
        {
            return _services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();
        }

        public PluginTextsObject GetPluginTexts()
        {
            return PluginTexts.GetTextsObject();
        }

        public PluginDefaultsObject GetPluginDefaults()
        {
            return PluginDefaultSettings.GetPluginDefaults();
        }

        #endregion

    }

    #endregion
}