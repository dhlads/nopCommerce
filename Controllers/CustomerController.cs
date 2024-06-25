using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Plugin.WholeSalePet.Customization.Infrastructure;
using Nop.Plugin.WholeSalePet.Customization.Models;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Customer;
using Nop.Web.Validators.Customer;

namespace Nop.Plugin.WholeSalePet.Customization.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PluginCustomerController : BasePluginController
    {
        #region Fields

        private readonly string _registerViewPath = "~/Plugins/WholeSalePet.Customization/Views/Register.cshtml";
        private readonly string _infoViewPath = "~/Plugins/WholeSalePet.Customization/Views/Info.cshtml";

        private readonly IAmazonS3 _s3Client;
        private readonly IPluginService _pluginService;

        protected readonly AddressSettings _addressSettings;
        protected readonly CaptchaSettings _captchaSettings;
        protected readonly CustomerSettings _customerSettings;
        protected readonly DateTimeSettings _dateTimeSettings;
        protected readonly ForumSettings _forumSettings;
        protected readonly GdprSettings _gdprSettings;
        protected readonly HtmlEncoder _htmlEncoder;
        protected readonly IAddressModelFactory _addressModelFactory;
        protected readonly IAddressService _addressService;
        protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
        protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
        protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
        protected readonly IAuthenticationService _authenticationService;
        protected readonly ICountryService _countryService;
        protected readonly ICurrencyService _currencyService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ICustomerModelFactory _customerModelFactory;
        protected readonly ICustomerRegistrationService _customerRegistrationService;
        protected readonly ICustomerService _customerService;
        protected readonly IDownloadService _downloadService;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly IExportManager _exportManager;
        protected readonly IExternalAuthenticationService _externalAuthenticationService;
        protected readonly IGdprService _gdprService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IGiftCardService _giftCardService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ILogger _logger;
        protected readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        protected readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        protected readonly INotificationService _notificationService;
        protected readonly IOrderService _orderService;
        protected readonly IPermissionService _permissionService;
        protected readonly IPictureService _pictureService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IProductService _productService;
        protected readonly IStateProvinceService _stateProvinceService;
        protected readonly IStoreContext _storeContext;
        protected readonly ITaxService _taxService;
        protected readonly IWorkContext _workContext;
        protected readonly IWorkflowMessageService _workflowMessageService;
        protected readonly LocalizationSettings _localizationSettings;
        protected readonly MediaSettings _mediaSettings;
        protected readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;
        protected readonly StoreInformationSettings _storeInformationSettings;
        protected readonly TaxSettings _taxSettings;
        private static readonly char[] _separator = [','];

        #endregion

        #region Ctor

        public PluginCustomerController(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            HtmlEncoder htmlEncoder,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
            IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ILogger logger,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings,
            IAmazonS3 s3Client,
            IPluginService pluginService)
        {
            _s3Client = s3Client;
            _pluginService = pluginService;
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _htmlEncoder = htmlEncoder;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _addressAttributeParser = addressAttributeParser;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _downloadService = downloadService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;
        }
        #endregion

        #region My account / Info

        public virtual async Task<IActionResult> Info()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var model = await PrepareCustomerInfoModelAsync(customer, excludeProperties: false);
            return View(_infoViewPath, model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Info(PluginCustomerInfoModel model, IFormCollection form)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var oldCustomerModel = new CustomerInfoModel();

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            // Validate native fields
            var validator = new CustomerInfoValidator(_localizationService, _stateProvinceService, _customerSettings);
            NativeFieldsValidator(model, validator);

            var infoModel = await PrepareCustomerInfoModelAsync(customer, model, true, customerAttributesXml, form);

            try
            {
                if (ModelState.IsValid)
                {

                    //username 
                    if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                    {
                        var userName = model.Username;
                        if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            await _customerRegistrationService.SetUsernameAsync(customer, userName);

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }
                    //email
                    var email = model.Email;
                    if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                await _authenticationService.SignInAsync(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.VatNumber;
                        customer.VatNumber = model.VatNumber;

                        if (prevVatNumber != model.VatNumber)
                        {
                            var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                            customer.VatNumberStatusId = (int)vatNumberStatus;

                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                    model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.FirstNameEnabled)
                        customer.FirstName = model.FirstName;
                    if (_customerSettings.LastNameEnabled)
                        customer.LastName = model.LastName;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;

                    customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                            }
                            else
                            {
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = store.Id,
                                    LanguageId = customer.LanguageId ?? store.DefaultLanguageId,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute, model.Signature);

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                        await LogGdprAsync(customer, oldCustomerModel, model, form);

                    // Upload Tax ID Certificate file to Amazon account
                    _ = await UploadFileToAWSS3Account(model.TaxIDCertificateFile, infoModel.GenericAttributes.StateTaxResaleID);

                    // Save custom generic attributes
                    await SaveCustomGenericAttributes(model.GenericAttributes, customer);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.CustomerInfo.Updated"));

                    return RedirectToRoute("CustomerInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }

            //If we got this far, something failed, redisplay form
            infoModel = await PrepareCustomerInfoModelAsync(customer, model, true, customerAttributesXml);
            return View(_infoViewPath, infoModel);
        }

        #endregion

        #region Register Account

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = await PrepareCustomerRegisterModelAsync(excludeProperties: false, setDefaultValues: true);
            return View(_registerViewPath, model);
        }


        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot] // Try to detect bots
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        public virtual async Task<IActionResult> Register(PluginCustomerRegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsRegisteredAsync(customer))
            {
                //Already registered customer. 
                await _authenticationService.SignOutAsync();

                //raise logged out event       
                await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

                customer = await _customerService.InsertGuestCustomerAsync();

                //Save a new record
                await _workContext.SetCurrentCustomerAsync(customer);
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            customer.RegisteredInStoreId = store.Id;

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            // Fixing NopCommerce 4.70 bug
            if (model.CountryId > 0)
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.CountryId);
                model.AvailableStates = states
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name
                    }).ToList();
            }

            // Validate native fields
            var validator = new RegisterValidator(_localizationService, _stateProvinceService, _customerSettings);
            NativeFieldsValidator(model, validator);

            var registerModel = await PrepareCustomerRegisterModelAsync(model, true, customerAttributesXml, false, form);

            if (ModelState.IsValid)
            {

                var customerUserName = model.Username;
                var customerEmail = model.Email;

                var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    customerEmail,
                    _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    store.Id,
                    isApproved);
                var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                if (registrationResult.Success)
                {
                    // Upload Tax ID Certificate file to Amazon account
                    _ = await UploadFileToAWSS3Account(model.TaxIDCertificateFile, registerModel.GenericAttributes.StateTaxResaleID);

                    // Save custom generic attributes
                    await SaveCustomGenericAttributes(model.GenericAttributes, customer);

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;

                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        customer.VatNumber = model.VatNumber;

                        var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        customer.VatNumberStatusId = (int)vatNumberStatus;
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.FirstNameEnabled)
                        customer.FirstName = model.FirstName;
                    if (_customerSettings.LastNameEnabled)
                        customer.LastName = model.LastName;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;

                    //save customer attributes
                    customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        var isNewsletterActive = _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

                        //save newsletter value
                        var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customerEmail, store.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = isNewsletterActive;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                                }
                            }
                            //else
                            //{
                            //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                            //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            //}
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customerEmail,
                                    Active = isNewsletterActive,
                                    StoreId = store.Id,
                                    LanguageId = customer.LanguageId ?? store.DefaultLanguageId,
                                    CreatedOnUtc = DateTime.UtcNow
                                });

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                                }
                            }
                        }
                    }

                    if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    {
                        //privacy policy is required
                        //GDPR
                        if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        {
                            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                        }
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                    {
                        var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                        foreach (var consent in consents)
                        {
                            var controlId = $"consent{consent.Id}";
                            var cbConsent = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                            {
                                //agree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                            }
                            else
                            {
                                //disagree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                            }
                        }
                    }

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        Company = customer.Company,
                        CountryId = customer.CountryId > 0
                            ? (int?)customer.CountryId
                            : null,
                        StateProvinceId = customer.StateProvinceId > 0
                            ? (int?)customer.StateProvinceId
                            : null,
                        County = customer.County,
                        City = customer.City,
                        Address1 = customer.StreetAddress,
                        Address2 = customer.StreetAddress2,
                        ZipPostalCode = customer.ZipPostalCode,
                        PhoneNumber = customer.Phone,
                        FaxNumber = customer.Fax,
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (await _addressService.IsAddressValidAsync(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        //customer.Addresses.Add(defaultAddress);

                        await _addressService.InsertAddressAsync(defaultAddress);

                        await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                        customer.BillingAddressId = defaultAddress.Id;
                        customer.ShippingAddressId = defaultAddress.Id;

                        await _customerService.UpdateCustomerAsync(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    //raise event       
                    await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                    var currentLanguage = await _workContext.GetWorkingLanguageAsync();

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            //email validation message
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                            await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, currentLanguage.Id);

                            //result
                            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });

                        case UserRegistrationType.AdminApproval:
                            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });

                        case UserRegistrationType.Standard:
                            //send customer welcome message
                            await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                            returnUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                            return await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, true);

                        default:
                            return RedirectToRoute("Homepage");
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            registerModel = await PrepareCustomerRegisterModelAsync(model, true, customerAttributesXml);
            return View(_registerViewPath, registerModel);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        public virtual async Task<IActionResult> RegisterResult(int resultId, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            var model = await _customerModelFactory.PrepareRegisterResultModelAsync(resultId, returnUrl);
            return View(model);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare custom fields for info / my account page
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="customInfoModel"></param>
        /// <param name="excludeProperties"></param>
        /// <param name="overrideCustomCustomerAttributesXml"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        protected async Task<PluginCustomerInfoModel> PrepareCustomerInfoModelAsync(
            Customer customer,
            PluginCustomerInfoModel customInfoModel = null,
            bool excludeProperties = false,
            string overrideCustomCustomerAttributesXml = "",
            IFormCollection form = null)
        {

            if (customInfoModel == null)
            {
                // Get Customer info model
                var infoModel = new CustomerInfoModel();
                infoModel = await _customerModelFactory.PrepareCustomerInfoModelAsync(infoModel, customer, excludeProperties);

                var customerModel = new PluginCustomerInfoModel()
                {
                    GenericAttributes = new PluginCustomerBaseModel()
                };

                // Set customer attributes values to model
                var model = await SetAttributesValuesToModel(customer.Id, "Customer", customerModel);

                // Set PluginCustomerRegisterModel model with native CustomerInfoModel properties
                CopyProperties(infoModel, model);

                return model;
            }

            _ = await _customerModelFactory.PrepareCustomerInfoModelAsync(customInfoModel, customer, excludeProperties, overrideCustomCustomerAttributesXml);

            return customInfoModel;
        }


        /// <summary>
        /// Prepare custom fields for registration page
        /// </summary>
        /// <param name="customRegisterModel"></param>
        /// <param name="excludeProperties"></param>
        /// <param name="overrideCustomCustomerAttributesXml"></param>
        /// <param name="setDefaultValues"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        protected async Task<PluginCustomerRegisterModel> PrepareCustomerRegisterModelAsync(
            PluginCustomerRegisterModel customRegisterModel = null,
            bool excludeProperties = false,
            string overrideCustomCustomerAttributesXml = "",
            bool setDefaultValues = false,
            IFormCollection form = null)
        {
            if (customRegisterModel == null)
            {
                // Get CustomerModelFactory default values
                var registerModel = new RegisterModel();
                _ = await _customerModelFactory.PrepareRegisterModelAsync(registerModel, excludeProperties, overrideCustomCustomerAttributesXml, setDefaultValues);

                var model = new PluginCustomerRegisterModel()
                {
                    GenericAttributes = new PluginCustomerBaseModel()
                };

                // Set PluginCustomerRegisterModel model with default values
                CopyProperties(registerModel, model);

                return model;
            }

            _ = await _customerModelFactory.PrepareRegisterModelAsync(customRegisterModel, excludeProperties, overrideCustomCustomerAttributesXml, setDefaultValues);

            return customRegisterModel;
        }

        /// <summary>
        /// Save Custom Generic Attributes o 'GenericAttributes' table
        /// </summary>
        /// <param name="model"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        protected async Task SaveCustomGenericAttributes(PluginCustomerBaseModel model, Customer customer)
        {
            // Save custom properties into Generic Attributes table
            await _genericAttributeService.SaveAttributeAsync(customer, "WebsiteURL", model.WebsiteURL);
            await _genericAttributeService.SaveAttributeAsync(customer, "StateTaxResaleID", model.StateTaxResaleID);
            await _genericAttributeService.SaveAttributeAsync(customer, "BusinessCategory", model.BusinessCategory);
            await _genericAttributeService.SaveAttributeAsync(customer, "StoreType", model.StoreType);
            await _genericAttributeService.SaveAttributeAsync(customer, "OtherStoreType", model.OtherStoreType);
            await _genericAttributeService.SaveAttributeAsync(customer, "ECommerceBusiness", model.ECommerceBusiness);
            await _genericAttributeService.SaveAttributeAsync(customer, "OtherECommerceBusiness", model.OtherECommerceBusiness);
            await _genericAttributeService.SaveAttributeAsync(customer, "ServiceBusiness", model.ServiceBusiness);
            await _genericAttributeService.SaveAttributeAsync(customer, "OtherServiceBusiness", model.OtherServiceBusiness);
        }

        /// <summary>
        /// Set Generic Attributes Values to Model
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected async Task<PluginCustomerInfoModel> SetAttributesValuesToModel(
            int entityId, string entityType, PluginCustomerInfoModel model)
        {
            var attributes = await _genericAttributeService.GetAttributesForEntityAsync(entityId, entityType);
            foreach (var attribute in attributes)
            {
                var propertyName = attribute.Key;
                var propertyValue = attribute.Value;

                var propertyInfo = typeof(PluginCustomerBaseModel).GetProperty(propertyName);
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    var propertyType = propertyInfo.PropertyType;

                    object convertedValue;
                    if (propertyType.IsEnum)
                    {
                        convertedValue = Enum.Parse(propertyType, propertyValue);
                    }
                    else if (Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
                    {
                        var enumType = Nullable.GetUnderlyingType(propertyType);
                        convertedValue = string.IsNullOrEmpty(propertyValue) ? null : Enum.Parse(enumType, propertyValue);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(propertyValue, propertyType);
                    }
                    propertyInfo.SetValue(model.GenericAttributes, convertedValue);
                }
                else
                {
                    // Otherwise, try to find the property directly in the model
                    propertyInfo = typeof(PluginCustomerInfoModel).GetProperty(propertyName);
                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        var convertedValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                        propertyInfo.SetValue(model, convertedValue);
                    }
                }
            }

            return model;
        }


        /// <summary>
        /// Copy proprieties between models
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyProperties<TSource, TDestination>(TSource source, TDestination destination)
        {
            var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProp in sourceProperties)
            {
                var destinationProp = destinationProperties.FirstOrDefault(p => p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
                if (destinationProp != null && destinationProp.CanWrite)
                {
                    destinationProp.SetValue(destination, sourceProp.GetValue(source, null), null);
                }
            }
        }

        /// <summary>
        /// Native fields validations
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="validator"></param>
        private void NativeFieldsValidator<TModel>(TModel model, IValidator<TModel> validator)
        {
            var validationResult = validator.Validate(model);

            foreach (var error in validationResult.Errors)
            {
                string errorMsg = error.ErrorMessage.EndsWith(".")
                        ? error.ErrorMessage.Substring(0, error.ErrorMessage.Length - 1)
                        : error.ErrorMessage;

                if (error.PropertyName.ToLower().Contains("password") && !error.ErrorMessage.ToLower().Contains(" required"))
                {
                    if (!error.PropertyName.ToLower().Equals("password"))
                    {
                        errorMsg = "Password does not match";
                    }
                    else
                    {
                        errorMsg = "Invalid password";
                    }
                }

                ModelState.AddModelError(error.PropertyName, errorMsg);
            }
        }

        /// <summary>
        /// Upload file to Amazon AWS.S3 account
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected async Task<bool> UploadFileToAWSS3Account(IFormFile file, string stateTaxResaleID = null)
        {
            if (file == null)
                return false;

            IWebHostEnvironment env = _pluginService.GetEnvironment();
            IConfiguration awsConfiguration = _pluginService.GetConfiguration();

            var bucketName = awsConfiguration["AWS:DevBucketName"];
            if (env.EnvironmentName.ToLower().Equals("production"))
            {
                bucketName = awsConfiguration["AWS:ProdBucketName"];
            }

            var key = stateTaxResaleID ?? file.FileName;

            // Remove invalid chars to the new file name in Amazon
            string invalidChars = new string(Path.GetInvalidFileNameChars());
            key = string.Join("", key.Split(invalidChars.ToCharArray()));

            using (var newMemoryStream = new MemoryStream())
            {
                file.CopyTo(newMemoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = key,
                    BucketName = bucketName,
                    ContentType = file.ContentType
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);

                return true;
            }
        }

        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
                {
                    ModelState.AddModelError("", consent.RequiredMessage);
                }
            }
        }

        protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
        {
            ArgumentNullException.ThrowIfNull(form);

            var attributesXml = "";
            var attributes = await _customerAttributeService.GetAllAttributesAsync();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                         .Where(v => v.IsPreSelected)
                                         .Select(v => v.Id)
                                         .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel,
            CustomerInfoModel newCustomerInfoModel, IFormCollection form)
        {
            try
            {
                //consents
                var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, customer.Id);
                    var controlId = $"consent{consent.Id}";
                    var cbConsent = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value)
                        {
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                //newsletter subscriptions
                if (_gdprSettings.LogNewsletterConsent)
                {
                    if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                    if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }

                //user profile changes
                if (!_gdprSettings.LogUserProfileChanges)
                    return;

                if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

                if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

                if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

                if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

                if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

                if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

                if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

                if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

                if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

                if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

                if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

                if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
                {
                    var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
                }

                if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
                {
                    var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
                }
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync(exception.Message, exception, customer);
            }
        }

        #endregion
    }
}
