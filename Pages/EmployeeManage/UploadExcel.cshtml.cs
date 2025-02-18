using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication8.Data;
using WebApplication8.Models;
//initial version of the project

namespace WebApplication8.Pages.EmployeeManage
{
    [Authorize(Policy = "ManageEmployee")]
    public class UploadExcelModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UploadExcelModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please select an Excel file")]
        public IFormFile ExcelFile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (ExcelFile == null || ExcelFile.Length == 0)
            {
                TempData["Error"] = "Ì—ÃÏ «Œ Ì«— „·› Excel ’«·Õ";
                return RedirectToPage();
            }

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(ExcelFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["Error"] = "‰Ê⁄ «·„·› €Ì— ’«·Õ. Ìı”„Õ ›ﬁÿ »„·›«  Excel.";
                return RedirectToPage();
            }

            var employees = new List<Employee>();
            var errors = new List<string>();

            using (var stream = new MemoryStream())
            {
                await ExcelFile.CopyToAsync(stream);
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    // Validate column headers
                    var requiredHeaders = new[] { "Name", "Email", "LocalPhone", "Department",
                        "Position", "OfficeLocation", "Notes", "PhoneNumber", "OtherNumbers" };

                    for (int col = 1; col <= requiredHeaders.Length; col++)
                    {
                        if (worksheet.Cells[1, col].Value?.ToString() != requiredHeaders[col - 1])
                        {
                            TempData["Error"] = $"Invalid Excel format. Column {col} should be '{requiredHeaders[col - 1]}'";
                            return RedirectToPage();
                        }
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var employee = new Employee
                            {
                                Name = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                                Email = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                                LocalPhone = long.Parse(worksheet.Cells[row, 3].Value?.ToString().Trim()),
                                Department = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                                Position = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                                OfficeLocation = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                                Notes = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                                PhoneNumber = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                                OtherNumbers = worksheet.Cells[row, 9].Value?.ToString().Trim()
                            };

                            // name and local phone are required and can not be null
                            if (string.IsNullOrWhiteSpace(employee.Name))
                            {
                                errors.Add($"Row {row}: Name is required");
                                continue;
                            }

                            if (employee.LocalPhone == null || employee.LocalPhone == 0)
                            {
                                errors.Add($"Row {row}: LocalPhone is required");
                                continue;
                            }

                            employees.Add(employee);
                        }
                        catch
                        {
                            errors.Add($"Error processing row {row}");
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                TempData["Error"] = $"Process completed with {errors.Count} errors. First error: {errors.First()}";
                return RedirectToPage();
            }

            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();

            TempData["Message"] = $" „  Õ„Ì· {employees.Count} „ÊŸ›« »‰Ã«Õ!";
            return RedirectToPage();
        }
    }
}
