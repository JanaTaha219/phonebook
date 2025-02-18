// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using WebApplication8.Models;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace WebApplication8.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;


        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string UsernameOrEmail { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Check if input is an email or username
                var user = await _userManager.FindByNameAsync(Input.UsernameOrEmail)
                            ?? await _userManager.FindByEmailAsync(Input.UsernameOrEmail);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                if (!user.EmailConfirmed)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = token },
                        protocol: Request.Scheme);

                    var emailBody = $@"
<div dir='rtl' style='font-size: 18px; font-weight: bold; margin-bottom: 16px; text-align: right;'>
    مرحبًا بكم في خدمة دليل الهاتف الجامعي!
</div>
<p dir='rtl' style='text-align: right;'>
    يرجى تأكيد عنوان بريدك الإلكتروني عن طريق النقر على الرابط أدناه:
</p>
<p dir='rtl' style='text-align: right;'>
    <a href='{confirmationLink}' style='color: #0066cc; text-decoration: underline;'>تأكيد البريد الإلكتروني</a>
</p>
<p dir='rtl' style='text-align: right;'>
    إذا لم تطلب هذا، يرجى تجاهل هذه الرسالة.
</p>";

                    await _emailSender.SendEmailAsync(
                        user.Email,
                        "Confirm your email address",
                        emailBody);

                    return RedirectToPage("./ConfirmEmail", new { userId = user.Id });
                }

                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }
    }

}
