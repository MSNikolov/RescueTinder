using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using RescueTinder.Data;

namespace RescueTinder.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [MinLength(3), MaxLength(50)]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [MinLength(3), MaxLength(50)]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Your picture in jpg format")]
            public IFormFile Image { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date of birth")]
            public DateTime BirthDate { get; set; }

            [Display(Name = "Describe yourself")]
            [DataType(DataType.MultilineText)]
            public string PersonalNotes { get; set; }

            [Required]
            [Display(Name = "Which province are you situated in?")]
            public Province Province { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var validImage = Input.Image.FileName.EndsWith(".jpg");

            if (ModelState.IsValid && validImage)
            {
                var acc = new CloudinaryDotNet.Account("dmm9z8uow", "367813196612582", "I3kSZZCbEN-OHiyD35eh8mzyO8k");

                var cloud = new Cloudinary(acc);

                var file = new FileDescription(Input.Image.FileName, Input.Image.OpenReadStream());

                var upload = new ImageUploadParams()
                {
                    File = file
                };

                var uploaded = await cloud.UploadAsync(upload);

                var user = new User 
                {
                    UserName = Input.Email, 
                    Email = Input.Email,
                    FirstName=Input.FirstName,
                    LastName=Input.LastName,
                    BirthDate=Input.BirthDate,
                    PersonalNotes=Input.PersonalNotes,
                    Province=Input.Province,
                    ImageUrl=uploaded.Uri.AbsoluteUri
                };
                var result = await _userManager.CreateAsync(user, Input.Password);

                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    if (user.Email == "drmnikolov@gmail.com")
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }

                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
