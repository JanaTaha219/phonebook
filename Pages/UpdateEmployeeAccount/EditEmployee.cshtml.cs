using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;
using WebApplication8.Services;

namespace WebApplication8.Pages
{
    public class EditEmployeeModel : PageModel
    {
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public EditEmployeeModel(ITokenService tokenService, ApplicationDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; }

        public IActionResult OnGet(string token)
        {
            if (!_tokenService.ValidateToken(token, out var userId))
            {
                return RedirectToPage("TokenExpired");
            }

            Employee = _context.Employees.FirstOrDefault(e => (e.Id).ToString() == userId);

            if (Employee == null)
            {
                return NotFound();
            }

            // Fetch distinct departments from Employee table
            var departments = _context.Employees
                                    .Select(e => e.Department)   // Assuming Department is a property of Employee
                                    .Distinct()
                                    .ToList();

            // Pass the departments to the view
            ViewData["Departments"] = departments;

            ViewData["EditToken"] = token; // Store token in view data for form
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string token)
        {
            if (!_tokenService.ValidateToken(token, out var userId))
            {
                return RedirectToPage("TokenExpired");

            }

            var employeeToUpdate = await _context.Employees.FindAsync(int.Parse(userId));

            if (employeeToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Employee>(
                employeeToUpdate,
                "Employee",
                e => e.Name,
                e => e.Email,
                e => e.LocalPhone,
                e => e.Department,
                e => e.Position,
                e => e.OfficeLocation,
                e => e.Notes,
                e => e.PhoneNumber,
                e => e.OtherNumbers))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    _tokenService.MarkTokenAsUsed(token);
                    return RedirectToPage("Successful");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again.");
                }
            }

            return Page();
        }
    }
}
