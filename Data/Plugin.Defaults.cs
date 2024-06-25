using System.Reflection;
using Newtonsoft.Json;

namespace Nop.Plugin.WholeSalePet.Customization.Data
{
    public class PluginDefaultSettings
    {
        public static PluginDefaultsObject GetPluginDefaults()
        {
            string baseDirectory = $"{Assembly.GetExecutingAssembly().Location.Split(Assembly.GetCallingAssembly().GetName().Name)[0]}";
            string filePath = Path.Combine(baseDirectory, "PluginResources/plugin.defaults.json");

            // Check if the file exists
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<PluginDefaultsObject>(jsonContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("The file does not exist.");
                return null;
            }
        }

        public class PluginDefaultsObject
        {

            public string ContactUsEmail { get; set; }

            public string CaptchaPublicKey { get; set; }

            public string CaptchaPrivateKey { get; set; }

            public string FacebookURL { get; set; }

            public string InstagramURL { get; set; }

            public string YoutubeURL { get; set; }

            public string TwitterX { get; set; }
        }
    }
}