using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApplication8.Pages.Account
{
    [Authorize(Policy = "CreateAdmin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender; 

        public CreateModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty] public string Username { get; set; } // New property
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public bool CanUpdateUserRole { get; set; }
        [BindProperty] public bool CanCreateAdmin { get; set; }
        [BindProperty] public bool CanDeleteAdmin { get; set; }
        [BindProperty] public bool CanManageEmployee { get; set; }
        [BindProperty] public bool ViewAnalytics { get; set; }
        [BindProperty] public bool EditMainPage { get; set; }
        

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new IdentityUser
            {
                UserName = Username,
                Email = Email,
                EmailConfirmed = false
            };

            var existingUser = await _userManager.FindByNameAsync(Username);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "«”„ «·„” Œœ„ „ÊÃÊœ »«·›⁄·° Ì—ÃÏ «Œ Ì«— «”„ „” Œœ„ ¬Œ—.");
                return Page();
            }

            var result = await _userManager.CreateAsync(user, Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            var claims = new List<Claim>();
            if (CanUpdateUserRole) claims.Add(new Claim("UpdateUserRole", "true"));
            if (CanCreateAdmin) claims.Add(new Claim("CreateAdmin", "true"));
            if (CanDeleteAdmin) claims.Add(new Claim("DeleteAdmin", "true"));
            if (CanManageEmployee) claims.Add(new Claim("ManageEmployee", "true"));
            if (ViewAnalytics) claims.Add(new Claim("ViewAnalytics", "true"));
            if (EditMainPage) claims.Add(new Claim("EditMainPage", "true"));

            foreach (var claim in claims)
            {
                await _userManager.AddClaimAsync(user, claim);
            }

            TempData["CreatSuccessMessage"] = "  „ ≈‰‘«¡ «·„” Œœ„ »‰Ã«Õ, ⁄‰œ „Õ«Ê·… «·„” Œœ„  ”ÃÌ· «·œŒÊ· «·Ï Õ”«»Â ”Ì’·Â »—Ìœ ≈·ﬂ —Ê‰Ì · ›⁄Ì· «·Õ”«».";
            return RedirectToPage("/Account/Create");
        }

    }


}