using DocumentFormat.OpenXml.InkML;
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
    public class UpdateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdateModel> _logger;

        public UpdateModel(
            ApplicationDbContext context,
            ILogger<UpdateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Employee Employee { get; set; }

        public string SuccessMessage { get; set; }
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

        public JsonResult OnGetSearchUsers(string term)
        {
            var users = _context.Employees
                .Where(e => e.Name.StartsWith(term))
                .Select(e => new { e.Id, e.Name })
                .Take(10)
                .ToList();
            return new JsonResult(users);
        }

        public async Task<JsonResult> OnGetUserDetails(int id)
        {
            var user = await _context.Employees.FindAsync(id);
            return new JsonResult(user);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "���� ����� ������� �� �������.";
                ViewData["Departments"] = _context.Employees
                    .Where(e => !string.IsNullOrEmpty(e.Department))
                    .Select(e => e.Department)
                    .Distinct()
                    .ToList();
                return Page();
            }

            var existingEmployee = await _context.Employees.FindAsync(Employee.Id);

            if (existingEmployee == null)
            {
                ErrorMessage = "������ ��� �����.";
                ViewData["Departments"] = _context.Employees
                    .Where(e => !string.IsNullOrEmpty(e.Department))
                    .Select(e => e.Department)
                    .Distinct()
                    .ToList();
                return Page();
            }

            try
            {
                existingEmployee.Name = Employee.Name;
                existingEmployee.Email = Employee.Email;
                existingEmployee.LocalPhone = Employee.LocalPhone;
                existingEmployee.Department = Employee.Department;
                existingEmployee.Position = Employee.Position;
                existingEmployee.OfficeLocation = Employee.OfficeLocation;
                existingEmployee.Notes = Employee.Notes;
                existingEmployee.PhoneNumber = Employee.PhoneNumber;
                existingEmployee.OtherNumbers = Employee.OtherNumbers;

                await _context.SaveChangesAsync();

                SuccessMessage = "�� ����� ������ ������ �����!";
            }
            catch (Exception ex)
            {
                ErrorMessage = "��� ��� ����� ����� ������ ������.";
                _logger.LogError(ex, "��� ����� ����� ������ ������.");
            }

            ViewData["Departments"] = _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .Select(e => e.Department)
                .Distinct()
                .ToList();

            return Page();
        }
    }
}
