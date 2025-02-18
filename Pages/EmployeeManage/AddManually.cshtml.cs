using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;

namespace WebApplication8.Pages.EmployeeManage
{
    [Authorize(Policy = "ManageEmployee")]
    public class AddEmployeeManuallyModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddEmployeeManuallyModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; }


        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            var departments = _context.Employees
                                  .Where(e => !string.IsNullOrEmpty(e.Department))
                                  .Select(e => e.Department)
                                  .Distinct()
                                  .ToList();

            ViewData["Departments"] = departments;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "������ ����� ������� �� �������";
                return Page();
            }

            try
            {
                _context.Employees.Add(Employee);
                await _context.SaveChangesAsync();

                SuccessMessage = "��� ����� ������ �����";

                return RedirectToPage(); 
            }
            catch (DbUpdateException ex)
            {
                ErrorMessage = "��� ��� ����� �����. ������ �������� ��� ����";
                Console.WriteLine($"Database error: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = "��� ��� ��� �����";
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToPage();
        }
    }

}
