using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication8.Pages.Account
{
    [Authorize(Policy = "DeleteAdmin")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public DeleteModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string Username { get; set; }

        public IdentityUser UserToDelete { get; set; }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                UserToDelete = await _userManager.FindByNameAsync(username);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(Username))
            {
                var user = await _userManager.FindByNameAsync(Username);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToPage("");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return Page();
        }

        public async Task<JsonResult> OnGetSearchUsers(string term)
        {
            var users = _userManager.Users
                .Where(u => u.UserName.Contains(term))
                .Select(u => new { u.UserName })
                .ToList();

            return new JsonResult(users);
        }
    }
}
