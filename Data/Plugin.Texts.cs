using System.Reflection;
using Newtonsoft.Json;

namespace Nop.Plugin.WholeSalePet.Customization.Data
{
    public class PluginTexts
    {
        public static PluginTextsObject GetTextsObject()
        {
            string baseDirectory = $"{Assembly.GetExecutingAssembly().Location.Split(Assembly.GetCallingAssembly().GetName().Name)[0]}";
            string filePath = Path.Combine(baseDirectory, "PluginResources/plugin.texts.json");

            // Check if the file exists
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<PluginTextsObject>(jsonContent);
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

        public class PluginTextsObject
        {
            public string RegisterTitle { get; set; }                        

            public string RegisterDescription { get; set; }
                     
            public string AddressTitle { get; set; }

            public string AddressInformation { get; set; }

            public string AccountInformation { get; set; }

            public string AdditionalInformation { get; set; }

            public string AdditionalInformationDescription { get; set; }

            public string Newsletter { get; set; }

            public string NewsletterText { get; set; }

            public PrivacyPolicy PrivacyInfo { get; set; }

            public AboutUs AboutUs { get; set; }

        }

        public class PrivacyPolicy
        {
            public string Title { get; set; }

            public string Body { get; set; }

            public string LinkText { get; set; }
        }

        public class AboutUs
        {
            public string Title { get; set; }

            public string Body { get; set; }
        }
    }
}