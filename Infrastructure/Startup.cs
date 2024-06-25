using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.WholeSalePet.Customization.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var env = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();

            // Configure Amazone.S3 service
            var builder = new ConfigurationBuilder()
                .SetBasePath($"{env.ContentRootPath}/Plugins/WholeSalePet.Customization/PluginResources/")
                .AddJsonFile("plugin.aws.json", optional: false, reloadOnChange: true);

            IConfiguration awsConfiguration = builder.Build();

            var awsOptions = awsConfiguration.GetAWSOptions();
            awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(
                awsConfiguration["AWS:AccessKeyId"],
                awsConfiguration["AWS:SecretAccessKey"]
            );
            awsOptions.Region = Amazon.RegionEndpoint.GetBySystemName(awsConfiguration["AWS:Region"]);

            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();

            // Register custom service to send service information to controller
            services.AddSingleton<IPluginService>(provider => new PluginService(services, awsConfiguration));
        }

        public void Configure(IApplicationBuilder app)
        {

        }

        public int Order => 1000; // Ensure this runs after the core services are registered
    }
}
