using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.WholeSalePet.Customization.Models
{

    #region Enumerators 

    public enum BusinessCategoryOptions
    {
        [Description("Brick and Mortar Retail")]
        BrickAndMortarRetail,

        [Description("E-Commerce")]
        ECommerce,

        [Description("Services")]
        Services
    }

    public enum StoreTypeOptions
    {
        [Description("Pet Speciality")]
        PetSpeciality,

        [Description("Gift Store")]
        GiftStore,

        [Description("Other")]
        Other
    }

    public enum ECommerceBusinessOptions
    {
        [Description("Stocking Online Retailer")]
        StockingOnlineRetailer,

        [Description("Drop-Shipper")]
        DropShipper,

        [Description("3rd Party Marketplaces")]
        ThirdPartyMarketplaces,

        [Description("Social Media")]
        SocialMedia,

        [Description("Other")]
        Other
    }

    public enum ServiceBusinessOptions
    {
        [Description("Grooming")]
        Grooming,

        [Description("Boarding/ Daycare")]
        BoardingDaycare,

        [Description("Rescue/ Shelter")]
        RescueShelter,

        [Description("Veterinary")]
        Veterinary,

        [Description("Trainer")]
        Trainer,

        [Description("Other")]
        Other
    }

    #endregion

    #region Classes

    public partial record PluginCustomerBaseModel
    {
        [DisplayName("Website URL")]
        public string WebsiteURL { get; set; }

        [DisplayName("State tax resale ID")]
        [Required]  
        public string StateTaxResaleID { get; set; }

        [DisplayName("Business category")]
        [Required(ErrorMessage = "Please select a business category")]
        public BusinessCategoryOptions? BusinessCategory { get; set; } = null;

        [DisplayName("Store type")]
        [RequiredIf("BusinessCategory", nameof(BusinessCategoryOptions.BrickAndMortarRetail), ErrorMessage = "Please select a store type")]
        public StoreTypeOptions? StoreType { get; set; }

        [DisplayName("Other store type")]
        [RequiredIf("StoreType", nameof(StoreTypeOptions.Other), ErrorMessage = "Please insert a store type")]
        public string OtherStoreType { get; set; }

        [DisplayName("E-commerce business")]
        [RequiredIf("BusinessCategory", nameof(BusinessCategoryOptions.ECommerce), ErrorMessage = "Please select a e-Commerce business")]
        public ECommerceBusinessOptions? ECommerceBusiness { get; set; } = null;

        [DisplayName("Other e-commerce business")]
        [RequiredIf("ECommerceBusiness", nameof(ECommerceBusinessOptions.Other), ErrorMessage = "Please insert a e-Commerce business")]
        public string OtherECommerceBusiness { get; set; }

        [DisplayName("Service business")]
        [RequiredIf("BusinessCategory", nameof(BusinessCategoryOptions.Services), ErrorMessage = "Please select a service business")]
        public ServiceBusinessOptions? ServiceBusiness { get; set; } = null;

        [DisplayName("Other service business")]
        [RequiredIf("ServiceBusiness", nameof(ServiceBusinessOptions.Other), ErrorMessage = "Please insert a service business")]
        public string OtherServiceBusiness { get; set; }
    }

    public partial record PluginCustomerInfoModel : CustomerInfoModel
    {
        public PluginCustomerBaseModel GenericAttributes { get; set; }

        // [Required(ErrorMessage = "Please upload a Tax ID Certificate file")]
        [AllowedFiles(new string[] { ".jpg", ".pdf" }, 2097152)] // Allowed extensions and file size in bytes
        public IFormFile TaxIDCertificateFile { get; set; }
    }

    public partial record PluginCustomerRegisterModel : RegisterModel
    {
        public PluginCustomerBaseModel GenericAttributes { get; set; }

        // [Required(ErrorMessage = "Please upload a Tax ID Certificate file")]
        [AllowedFiles(new string[] { ".jpg", ".pdf" }, 2097152)] // Allowed extensions and file size in bytes
        public IFormFile TaxIDCertificateFile { get; set; }
    }

    #endregion

    #region Custom validation attribute

    /// <summary>
    /// Custom upload file attribute validation to allow certain types of file extensions
    /// </summary>
    public class AllowedFilesAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        private readonly int _fileSizeLimitInBytes;

        public AllowedFilesAttribute(string[] extensions, int fileSizeLimitInBytes)
        {
            _extensions = extensions;
            _fileSizeLimitInBytes = fileSizeLimitInBytes;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult($"Allowed extensions: {string.Join(", ", _extensions)}");
                }

                if (file.Length > _fileSizeLimitInBytes)
                {
                    return new ValidationResult($"The file size must be less than {_fileSizeLimitInBytes / 1024 / 1024} MB");
                }
            }
            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Custom validation for custom fields
    /// </summary>
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly string _dependentValue;

        public RequiredIfAttribute(string dependentProperty, string dependentValue)
        {
            _dependentProperty = dependentProperty;
            _dependentValue = dependentValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dependentPropertyValue = validationContext.ObjectInstance.GetType().GetProperty(_dependentProperty)?.GetValue(validationContext.ObjectInstance, null)?.ToString();

            if (_dependentValue.Equals(dependentPropertyValue, StringComparison.OrdinalIgnoreCase))
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }

    #endregion
}

