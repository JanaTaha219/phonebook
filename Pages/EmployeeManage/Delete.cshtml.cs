using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;

namespace WebApplication8.Pages.EmployeeManage
{
    [Authorize(Policy = "ManageEmployee")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(
            ApplicationDbContext context,
            ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public int SelectedUserId { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public JsonResult OnGetSearchUsers(string term)
        {
            var users = _context.Employees
                .Where(e => e.Name.StartsWith(term))
                .Select(e => new { e.Id, e.Name })
                .Take(10)
                .ToList();
            return new JsonResult(users);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var user = await _context.Employees.FindAsync(SelectedUserId);
                if (user == null)
                {
                    ErrorMessage = "«·„ÊŸ› €Ì— „ÊÃÊœ!";
                    return RedirectToPage();
                }

                _context.Employees.Remove(user);
                await _context.SaveChangesAsync();

                SuccessMessage = " „ Õ–› «·„ÊŸ› »‰Ã«Õ!";
                SelectedUserId = 0; 
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Œÿ√ √À‰«¡ Õ–› «·„ÊŸ› »«·„⁄—› {EmployeeId}", SelectedUserId);
                ErrorMessage = "ÕœÀ Œÿ√ √À‰«¡ Õ–› «·„ÊŸ›. Ì—ÃÏ «·„Õ«Ê·… „—… √Œ—Ï.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Œÿ√ €Ì— „ Êﬁ⁄ √À‰«¡ Õ–› «·„ÊŸ›");
                ErrorMessage = "ÕœÀ Œÿ√ €Ì— „ Êﬁ⁄.";
            }

            return RedirectToPage();
        }
    }
}

