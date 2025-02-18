using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;

namespace WebApplication8.Pages.Privilege
{
    public class UpdateAdminPrivilegeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UpdateAdminPrivilegeModel(ApplicationDbContext context)
        {
            _context = context;
            SelectedPrivileges = new List<string>(); // Ensure it's initialized
        }

        [BindProperty]
        public string SelectedUserId { get; set; }

        [BindProperty]
        public List<string> SelectedPrivileges { get; set; }

        public List<string> UserPrivileges { get; set; }
        public List<string> AllPrivileges { get; set; }

        public JsonResult OnGetSearchUsers(string term)
        {
            var users = _context.Users
                .Where(u => u.UserName.Contains(term))
                .Select(u => new { u.Id, u.UserName })
                .ToList();

            return new JsonResult(users);
        }

        public IActionResult OnGetUserPrivileges(string userId)
        {
            var userPrivileges = _context.UserClaims
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ClaimType)
                .ToList();

            var allPrivileges = new List<string>
            {
                "ViewAnalytics",
                "DeleteAdmin",
                "CreateAdmin",
                "UpdateUserRole",
                "EditMainPage",
                "ManageEmployee"
            };

            var missingPrivileges = allPrivileges.Except(userPrivileges).ToList();

            return new JsonResult(new { UserPrivileges = userPrivileges, MissingPrivileges = missingPrivileges });
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(SelectedUserId))
            {
                ModelState.AddModelError("", "User ID is required.");
                return Page();
            }

            if (SelectedPrivileges == null)
            {
                SelectedPrivileges = new List<string>();
            }

            var existingPrivileges = _context.UserClaims
                .Where(uc => uc.UserId == SelectedUserId)
                .ToList();

            _context.UserClaims.RemoveRange(existingPrivileges);

            foreach (var privilege in SelectedPrivileges)
            {
                _context.UserClaims.Add(new IdentityUserClaim<string>
                {
                    UserId = SelectedUserId,
                    ClaimType = privilege,
                    ClaimValue = "true"
                });
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Privileges updated successfully!";
            return Page();
        }
    }
}
