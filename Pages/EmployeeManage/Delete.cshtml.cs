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
                    ErrorMessage = "������ ��� �����!";
                    return RedirectToPage();
                }

                _context.Employees.Remove(user);
                await _context.SaveChangesAsync();

                SuccessMessage = "�� ��� ������ �����!";
                SelectedUserId = 0; 
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "��� ����� ��� ������ ������� {EmployeeId}", SelectedUserId);
                ErrorMessage = "��� ��� ����� ��� ������. ���� �������� ��� ����.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��� ��� ����� ����� ��� ������");
                ErrorMessage = "��� ��� ��� �����.";
            }

            return RedirectToPage();
        }
    }
}

