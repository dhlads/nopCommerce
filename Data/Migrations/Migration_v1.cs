using System.Text;
using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;
using static Nop.Plugin.WholeSalePet.Customization.Data.PluginTexts;
using static Nop.Plugin.WholeSalePet.Customization.Data.PluginDefaultSettings;
using Nop.Core.Domain.News;
using System.Net.NetworkInformation;

namespace Nop.Plugin.WholeSalePet.Customization.Data.Migrations
{
    [NopMigration("2024/06/03 00:00:00", "WholeSalePet.Customization: new LocalResources and texts updates")]
    public class PluginMigration_v1 : Migration
    {

        #region Fields

        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public PluginMigration_v1(ILanguageService languageService,
            ILocalizationService localizationService)
        {
            _languageService = languageService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            PluginTextsObject pluginTexts = PluginTexts.GetTextsObject();
            PluginDefaultsObject pluginDefaults = PluginDefaultSettings.GetPluginDefaults();

            var (languageId, _) = this.GetLanguageData(_languageService);

            // Add/Update local resources to new values
            _localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                [$"{Plugin.PluginLocalResourcesPrefix}.ContactUsEmail"] = pluginDefaults.ContactUsEmail,
                [$"{Plugin.PluginLocalResourcesPrefix}.RegisterTitle"] = pluginTexts.RegisterTitle,
                [$"{Plugin.PluginLocalResourcesPrefix}.RegisterDescription"] = pluginTexts.RegisterDescription,
                [$"{Plugin.PluginLocalResourcesPrefix}.AddressInformation"] = pluginTexts.AddressInformation,
                [$"{Plugin.PluginLocalResourcesPrefix}.AddressTitle"] = pluginTexts.AddressTitle,
                [$"{Plugin.PluginLocalResourcesPrefix}.AcceptPrivacyPolicyLink"] = pluginTexts.PrivacyInfo.LinkText,
                [$"{Plugin.PluginLocalResourcesPrefix}.Newsletter"] = pluginTexts.Newsletter, // Default text in nopCommerce is "Options"
                [$"{Plugin.PluginLocalResourcesPrefix}.NewsletterText"] = pluginTexts.NewsletterText,
                [$"{Plugin.PluginLocalResourcesPrefix}.AccountInformation"] = pluginTexts.AccountInformation,
                [$"{Plugin.PluginLocalResourcesPrefix}.AdditionalInformation"] = pluginTexts.AdditionalInformation,
                [$"{Plugin.PluginLocalResourcesPrefix}.AdditionalInformationDescription"] = pluginTexts.AdditionalInformationDescription
            }, languageId);

            // Customer Settings
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE public.\"Setting\"");
            sql.AppendLine($" SET \"Value\" = 'True'");
            sql.AppendLine(" WHERE \"Name\" = 'customersettings.streetaddressenabled'"); // Set Street Address enabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.streetaddressrequired'"); // Set Street Address required
            sql.AppendLine(" OR \"Name\" = 'customersettings.streetaddress2enabled'"); // Set Street Address 2 enabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.zippostalcodeenabled'"); // Set Zip Postal enabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.zippostalcoderequired'"); // Set Zip Postal required
            sql.AppendLine(" OR \"Name\" = 'customersettings.cityenabled'"); // Set City enabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.cityrequired'"); // Set City required
            sql.AppendLine(" OR \"Name\" = 'customersettings.stateprovinceenabled'"); // Set State Province enabled
            //sql.AppendLine(" OR \"Name\" = 'customersettings.stateprovincerequired'"); // Set State Province required
            sql.AppendLine(" OR \"Name\" = 'customersettings.countryenabled'"); // Set Country enabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.countryrequired'"); // Set Country required
            sql.AppendLine(" OR \"Name\" = 'customersettings.phoneenabled'"); // Set Phone enabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.phonerequired'"); // Set Phone required
            sql.AppendLine(" OR \"Name\" = 'customersettings.companyenabled'"); // Set Company enabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.companyrequired'"); // Set Company required
            sql.AppendLine(" OR \"Name\" = 'customersettings.acceptprivacypolicyenabled'"); // Set Accept Privacy Policy enabled
            Execute.Sql(sql.ToString());

            // Gender
            Update.Table("Setting")
                .Set(new { Value = "False" })
                .Where(new { Name = "customersettings.genderenabled" });

            // Password Settings
            Update.Table("Setting")
                .Set(new { Value = "6" })
                .Where(new { Name = "customersettings.passwordminlength" }); // Set Password minimum length
            Update.Table("Setting")
                .Set(new { Value = "True" })
                .Where(new { Name = "customersettings.passwordrequiredigit" }); // Digit required
            Update.Table("Setting")
                .Set(new { Value = "True" })
                .Where(new { Name = "customersettings.passwordrequirelowercase" }); // Lower case char required
            Update.Table("Setting")
                .Set(new { Value = "False" })
                .Where(new { Name = "customersettings.passwordrequireuppercase" }); // Upper case char NOT required
            Update.Table("Setting")
                .Set(new { Value = "False" })
                .Where(new { Name = "customersettings.passwordrequirenonalphanumeric" }); // Special characters NOT required

            // Update 'Privacy Policy' texts
            Update.Table("Topic")
               .Set(new
               {
                   Title = $"{pluginTexts.PrivacyInfo.Title ?? "Privacy Policy"}",
                   Body = $"{pluginTexts.PrivacyInfo.Body ?? "Set here your privacy policy text"}"
               })
               .Where(new { SystemName = "PrivacyInfo" });

            // Update 'About us' texts
            Update.Table("Topic")
                .Set(new
                {
                    Title = $"{pluginTexts.AboutUs.Title ?? "About us"}",
                    Body = $"{pluginTexts.AboutUs.Body ?? ""}"
                })
                .Where(new { SystemName = "AboutUs" });

            // Captcha general settings
            sql = new StringBuilder();
            sql.AppendLine("UPDATE public.\"Setting\"");
            sql.AppendLine($" SET \"Value\" = 'True'");
            sql.AppendLine(" WHERE \"Name\" = 'captchasettings.enabled'"); // Set Captcha enabled
            sql.AppendLine(" OR \"Name\" = 'captchasettings.showonloginpage'"); // Show Captcha on Login Page
            sql.AppendLine(" OR \"Name\" = 'captchasettings.showonregistrationpage'"); // Show Captcha on Registration Page
            sql.AppendLine(" OR \"Name\" = 'captchasettings.showonforgotpasswordpage'"); // Show Captcha on Forgot Password Page
            Execute.Sql(sql.ToString());

            // Captcha Plubic Key
            Update.Table("Setting")
                .Set(new { Value = $"{pluginDefaults.CaptchaPublicKey ?? ""}" })
                .Where(new { Name = "captchasettings.recaptchapublickey" });

            // Captcha Private Key
            Update.Table("Setting")
               .Set(new { Value = $"{pluginDefaults.CaptchaPrivateKey ?? ""}" })
               .Where(new { Name = "captchasettings.recaptchaprivatekey" });

            // Facebook URL
            Update.Table("Setting")
               .Set(new { Value = $"{pluginDefaults.FacebookURL ?? ""}" })
               .Where(new { Name = "storeinformationsettings.facebooklink" });

            // Twitter / X URL
            Update.Table("Setting")
               .Set(new { Value = $"{pluginDefaults.TwitterX ?? ""}" })
               .Where(new { Name = "storeinformationsettings.twitterlink" });

            // Instagram URL
            Update.Table("Setting")
               .Set(new { Value = $"{pluginDefaults.InstagramURL ?? ""}" })
               .Where(new { Name = "storeinformationsettings.instagramlink" });

            // Youtube URL
            Update.Table("Setting")
               .Set(new { Value = $"{pluginDefaults.YoutubeURL ?? ""}" })
               .Where(new { Name = "storeinformationsettings.youtubelink" });

            // Hide RSS Feed footer icon when Youtube URL is empty
            Update.Table("Setting")
               .Set(new { Value = "False" })
               .Where(new { Name = "newssettings.enabled" });
        }

        public override void Down()
        {
            var (languageId, _) = this.GetLanguageData(_languageService);

            PluginTextsObject pluginTexts = PluginTexts.GetTextsObject();

            // Delete plugin local resources
            _localizationService.DeleteLocaleResources(new List<string>
            {
                $"{Plugin.PluginLocalResourcesPrefix}.ContactUsEmail",
                $"{Plugin.PluginLocalResourcesPrefix}.RegisterTitle",
                $"{Plugin.PluginLocalResourcesPrefix}.RegisterDescription",
                $"{Plugin.PluginLocalResourcesPrefix}.AddressInformation",
                $"{Plugin.PluginLocalResourcesPrefix}.AddressTitle",
                $"{Plugin.PluginLocalResourcesPrefix}.AcceptPrivacyPolicyLink",
                $"{Plugin.PluginLocalResourcesPrefix}.Newsletter",
                $"{Plugin.PluginLocalResourcesPrefix}.NewsletterText",
                $"{Plugin.PluginLocalResourcesPrefix}.AccountInformation",
                $"{Plugin.PluginLocalResourcesPrefix}.AdditionalInformation",
                $"{Plugin.PluginLocalResourcesPrefix}.AdditionalInformationDescription"
            }, languageId);

            // Restore Customer Settings
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE public.\"Setting\"");
            sql.AppendLine($" SET \"Value\" = 'False'");
            sql.AppendLine(" WHERE \"Name\" = 'customersettings.streetaddressenabled'"); // Set Street Address disabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.streetaddressrequired'"); // Set Street Address not required
            sql.AppendLine(" OR \"Name\" = 'customersettings.streetaddress2enabled'"); // Set Street Address 2 disabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.zippostalcodeenabled'"); // Set Zip Postal disabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.zippostalcoderequired'"); // Set Zip Postal not required
            sql.AppendLine(" OR \"Name\" = 'customersettings.cityenabled'"); // Set City disabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.cityrequired'"); // Set City not required
            sql.AppendLine(" OR \"Name\" = 'customersettings.stateprovinceenabled'"); // Set State Province disabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.stateprovincerequired'"); // Set State Province not required
            sql.AppendLine(" OR \"Name\" = 'customersettings.countryenabled'"); // Set Country disabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.countryrequired'"); // Set Country not required
            sql.AppendLine(" OR \"Name\" = 'customersettings.phoneenabled'"); // Set Phone disabled
            sql.AppendLine(" OR \"Name\" = 'customersettings.phonerequired'"); // Set Phone not required
            sql.AppendLine(" OR \"Name\" = 'customersettings.companyrequired'"); // Set Company not required
            sql.AppendLine(" OR \"Name\" = 'customersettings.acceptprivacypolicyenabled'"); // Set Accept Privacy Policy disabled
            Execute.Sql(sql.ToString());

            // Gender
            Update.Table("Setting")
                .Set(new { Value = "True" })
                .Where(new { Name = "customersettings.genderenabled" });

            // Restore Password Settings
            Update.Table("Setting")
                .Set(new { Value = "6" })
                .Where(new { Name = "customersettings.passwordminlength" }); // Set Password minimum length
            Update.Table("Setting")
                .Set(new { Value = "False" })
                .Where(new { Name = "customersettings.passwordrequiredigit" }); // Digit NOT required
            Update.Table("Setting")
                .Set(new { Value = "False" })
                .Where(new { Name = "customersettings.passwordrequirelowercase" }); // Lower case char NOT required
            Update.Table("Setting")
                .Set(new { Value = "False" })
                .Where(new { Name = "customersettings.passwordrequireuppercase" }); // Upper case char NOT required
            Update.Table("Setting")
                .Set(new { Value = "False" })
                .Where(new { Name = "customersettings.passwordrequirenonalphanumeric" }); // Special characters NOT required

            // Restore 'Privacy Policy' to default texts
            Update.Table("Topic")
                .Set(new { Title = "Privacy Policy", Body = "Set here your privacy policy text" })
                .Where(new { SystemName = "PrivacyInfo" });

            // Restore 'About us' to default texts
            Update.Table("Topic")
                .Set(new { Title = "About us", Body = "" })
                .Where(new { SystemName = "AboutUs" });


            // Restore Captcha general settings
            sql = new StringBuilder();
            sql.AppendLine("UPDATE public.\"Setting\"");
            sql.AppendLine($" SET \"Value\" = 'False'");
            sql.AppendLine(" WHERE \"Name\" = 'captchasettings.enabled'"); // Set Captcha disabled
            sql.AppendLine(" OR \"Name\" = 'captchasettings.showonloginpage'"); // Hide Captcha on Login Page
            sql.AppendLine(" OR \"Name\" = 'captchasettings.showonregistrationpage'"); // Hide Captcha on Registration Page
            sql.AppendLine(" OR \"Name\" = 'captchasettings.showonforgotpasswordpage'"); // Hide Captcha on Forgot Password Page
            Execute.Sql(sql.ToString());

            // Restore Captcha Plubic and Private Keys
            sql = new StringBuilder();
            sql.AppendLine("UPDATE public.\"Setting\"");
            sql.AppendLine($" SET \"Value\" = ''");
            sql.AppendLine(" WHERE \"Name\" = 'captchasettings.recaptchapublickey'");
            sql.AppendLine(" OR \"Name\" = 'captchasettings.recaptchaprivatekey'");
            Execute.Sql(sql.ToString());

            // Restore Social Media URLs
            sql = new StringBuilder();
            sql.AppendLine("UPDATE public.\"Setting\"");
            sql.AppendLine($" SET \"Value\" = ''");
            sql.AppendLine(" WHERE \"Name\" = 'storeinformationsettings.facebooklink'");
            sql.AppendLine(" OR \"Name\" = 'storeinformationsettings.instagramlink'");
            sql.AppendLine(" OR \"Name\" = 'storeinformationsettings.youtubelink'");
            sql.AppendLine(" OR \"Name\" = 'storeinformationsettings.twitterlink'");
            Execute.Sql(sql.ToString());

            // Show by default RSS Feed footer icon
            Update.Table("Setting")
               .Set(new { Value = "True" })
               .Where(new { Name = "newssettings.enabled" });

        }

        #endregion
    }
}