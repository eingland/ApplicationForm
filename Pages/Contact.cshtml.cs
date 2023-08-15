using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ApplicationForm.Pages
{
    public class ContactModel : PageModel
    {
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContactModel> _logger;
        public ContactModel(IConfiguration configuration, ILogger<ContactModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public ContactFormModel Contact { get; set; }
        public string error { get; set; }
        public void OnGet()
        {
            ViewData["ReCaptchaKey"] = _configuration["recaptchasitekey"];
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ReCaptchaKey"] = _configuration["recaptchasitekey"];

            if (ModelState.IsValid)
            {
                if (!ReCaptchaPassed(
                    Request.Form["g-recaptcha-response"], // that's how you get it from the Request object
                    _configuration["recaptchasecretkey"],
                    _logger
                    ))
                {
                    error = "You failed the CAPTCHA. Please try again.";
                    return Page();
                }

                // create and send email
                var mailbody = $@"Hello website owner,

                This is a new application from your website:

                First Name: {Contact.Name}
                Last Name: {Contact.LastName}
                Email: {Contact.Email}
                Telephone Number: {Contact.Telephone}
                Address: {Contact.Address}
                City: {Contact.City}
                Zip Code: {Contact.Zip}

                Felon: {Contact.Felon}
                Felon Comment: ""{Contact.FelonComment}""
                Desired Position: {Contact.DesiredPosition}
                Desired Wage: {Contact.DesiredWage}

                Previous Employer: {Contact.PreviousEmployer1}
                Previous Title: {Contact.PreviousTitle1}
                Previous Wage: {Contact.PreviousWage1}

                Previous Employer: {Contact.PreviousEmployer2}
                Previous Title: {Contact.PreviousTitle2}
                Previous Wage: {Contact.PreviousWage2}

                Previous Employer: {Contact.PreviousEmployer3}
                Previous Title: {Contact.PreviousTitle3}
                Previous Wage: {Contact.PreviousWage3}

                Additional Comments: ""{Contact.Message}""


                Cheers,
                The websites contact form";

                await SendMail(mailbody);

                return RedirectToPage("Index");
            }

            return Page();
        }

        private Task SendMail(string mailbody)
        {
            var apiKey = _configuration["SENDGRID-API-KEY"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("Form@example.com", "Form");
            List<EmailAddress> tos = new List<EmailAddress>
          {
              new EmailAddress("ingland.eric@gmail.com", "Eric Ingland")
          };

            var subject = "New E-Mail from my website";
            var htmlContent = mailbody;
            var displayRecipients = false; // set this to true if you want recipients to see each others mail id 
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, displayRecipients);
            var response = client.SendEmailAsync(msg);
            return response;
        }

        // A function that checks reCAPTCHA results
        // You might want to move it to some common class
        public static bool ReCaptchaPassed(string gRecaptchaResponse, string secret, ILogger logger)
        {
            HttpClient httpClient = new HttpClient();
            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={gRecaptchaResponse}").Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Error while sending request to ReCaptcha");
                return false;
            }

            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);
            if (JSONdata.success != "true")
            {
                return false;
            }

            return true;
        }
    }

    public class ContactFormModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [EmailAddress]
        [Compare("Email", ErrorMessage = "The email addresses do not match.")]
        [Display(Name = "Confirm Email")]
        public string EmailConfirm { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        public string Telephone { get; set; }
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Required]
        [Display(Name = "City")]
        public string City { get; set; }
        [Required]
        [Display(Name = "Zip Code")]
        public string Zip { get; set; }
        [Required]
        [Display(Name = "Felon Status")]
        public bool Felon { get; set; }
        [Display(Name = "Felon Comment")]
        public string FelonComment { get; set; }
        [Required]
        [Display(Name = "Desired Position")]
        public string DesiredPosition { get; set; }
        [Required]
        [Display(Name = "Desired Wage")]
        public string DesiredWage { get; set; }
        [Display(Name = "Previous Employer Name")]
        public string PreviousEmployer1 { get; set; }
        [Display(Name = "Previous Title")]
        public string PreviousTitle1 { get; set; }
        [Display(Name = "Previous Wage")]
        public string PreviousWage1 { get; set; }
        [Display(Name = "Permission to Contact")]
        public bool PermissionContact1 { get; set; }
        [Display(Name = "Previous Employer Name")]
        public string PreviousEmployer2 { get; set; }
        [Display(Name = "Previous Title")]
        public string PreviousTitle2 { get; set; }
        [Display(Name = "Previous Wage")]
        public string PreviousWage2 { get; set; }
        [Display(Name = "Permission to Contact")]
        public bool PermissionContact2 { get; set; }
        [Display(Name = "Previous Employer Name")]
        public string PreviousEmployer3 { get; set; }
        [Display(Name = "Previous Title")]
        public string PreviousTitle3 { get; set; }
        [Display(Name = "Previous Wage")]
        public string PreviousWage3 { get; set; }
        [Display(Name = "Permission to Contact")]
        public bool PermissionContact3 { get; set; }
        [Required]
        [Display(Name = "Additional Comments")]
        public string Message { get; set; }
    }
}
