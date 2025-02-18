using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;
using WebApplication8.Services;

namespace WebApplication8.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IAuthorizationService _authorizationService;

        public List<string> Departments { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext applicationDbContext, IEmailSender emailSender, IConfiguration configuration, ITokenService tokenService, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
            _emailSender = emailSender;
            _configuration = configuration;
            _tokenService = tokenService;
            _authorizationService = authorizationService;
        }

        // Search handler
        public async Task<IActionResult> OnGetSearchAsync([FromQuery] string query, [FromQuery] string department)
        {
            if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(department))
            {
                return BadRequest("Query and department cannot be both empty.");
            }

            var queryable = _applicationDbContext.Employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                queryable = queryable.Where(n => EF.Functions.Like(n.Name, $"{query}%"));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                queryable = queryable.Where(d => d.Department == department);
            }

            var results = await queryable
                .Select(n => new { n.Name, n.Id })  // Return ID for further fetching
                .Take(10)
                .ToListAsync();

            return new JsonResult(results);
        }


        // Employee details handler
        public async Task<IActionResult> OnGetEmployeeDetailsAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid employee ID.");
            }

            try
            {
                var employee = await _applicationDbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                // Increment search counter
                employee.counter++;

                // Save search history
                var searchHistory = new SearchHistory
                {
                    EmployeeId = employee.Id,
                    SearchDate = DateTime.Now
                };

                _applicationDbContext.SearchHistories.Add(searchHistory);
                await _applicationDbContext.SaveChangesAsync();

                var employeeDetails = new
                {
                    employee.Name,
                    employee.Email,
                    employee.LocalPhone,
                    employee.Department,
                    employee.Position,
                    employee.OfficeLocation,
                    employee.Notes,
                    employee.PhoneNumber,
                    employee.OtherNumbers
                };

                return new JsonResult(employeeDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee details.");
                return new JsonResult(new Employee());
            }
        }

        public async Task<JsonResult> OnPostSendUpdateRequestAsync([FromBody] EmailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Email))
                {
                    return new JsonResult(new { success = false, error = "«·»—Ìœ «·≈·ﬂ —Ê‰Ì „ÿ·Ê»" });
                }

                var employee = await _applicationDbContext.Employees.FirstOrDefaultAsync(e => e.Email == request.Email);
                if (employee == null)
                {
                    return new JsonResult(new { success = false, error = "«·„ÊŸ› €Ì— „ÊÃÊœ" });
                }

                // Generate a unique token (GUID + EmployeeID)
                var token = _tokenService.GenerateToken((employee.Id).ToString());
                var editLink = $"https://localhost:7181/UpdateEmployeeAccount/EditEmployee?token={token}";

                var subject = "ÿ·»  ÕœÌÀ «·„⁄·Ê„« ";
                var message = $@"„—Õ»«°<br/>
                                 „ «” ·«„ ÿ·»ﬂ · ÕœÌÀ «·„⁄·Ê„« . «·—Ã«¡ „ «»⁄… «·—«»ÿ «· «·Ì:<br/>
                                <a href='{editLink}'> ÕœÌÀ «·„⁄·Ê„« </a><br/>
                                «·—«»ÿ ’«·Õ ·„œ… 24 ”«⁄… ÊÌ„ﬂ‰ «” Œœ«„Â „—… Ê«Õœ… ›ﬁÿ.";

                await _emailSender.SendEmailAsync(
                    request.Email,
                    subject,
                    message);

                _logger.LogInformation($"Update request email sent to {request.Email}");

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending update request email");
                return new JsonResult(new
                {
                    success = false,
                    error = "›‘· ≈—”«· «·»—Ìœ «·≈·ﬂ —Ê‰Ì. «·—Ã«¡ «·„Õ«Ê·… ·«Õﬁ«."
                });
            }
        }

        //add claim can edit to main page.
        public async Task<IActionResult> OnPostAddToImportantNumbersAsync([FromBody] int employeeId)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, "EditMainPage");

            if (!authorizationResult.Succeeded)
            {
                return new ForbidResult(); // Returns 403 Forbidden if the user is not authorized
            }
            Console.WriteLine($"************************************{employeeId}");
            var exists = await _applicationDbContext.ImportantNumbers.AnyAsync(n => n.EmployeeId == employeeId);
            if (exists)
            {
                Console.WriteLine("EXISTTTTTTTTTTTTTTTTTTTTTTTt");
                return new JsonResult(new { success = false, message = "«·„ÊŸ› „÷«› »«·›⁄· ≈·Ï «·ﬁ«∆„…." });
            }

            ImportantNumber newImportantNumber = new ImportantNumber(employeeId);
            _applicationDbContext.ImportantNumbers.Add(newImportantNumber);
            await _applicationDbContext.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }



        //add claim can edit main page
        public async Task<IActionResult> OnPostRemoveFromImportantNumbersAsync([FromBody] int employeeId)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, "EditMainPage");

            if (!authorizationResult.Succeeded)
            {
                return new ForbidResult(); 
            }
            var record = await _applicationDbContext.ImportantNumbers
                .FirstOrDefaultAsync(n => n.EmployeeId == employeeId);

            if (record == null)
            {
                return new JsonResult(new { success = false, message = "«·„ÊŸ› €Ì— „ÊÃÊœ ›Ì «·ﬁ«∆„…." });
            }

            _applicationDbContext.ImportantNumbers.Remove(record);
            await _applicationDbContext.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnGetImportantNumbersAsync()
        {
            var numbers = await _applicationDbContext.ImportantNumbers
                .Include(n => n.Employee) // Include employee data
                .Select(n => new
                {
                    Id = n.EmployeeId,
                    Name = n.Employee.Name
                })
                .ToListAsync();

            return new JsonResult(numbers);
        }


        public class EmailRequest
    {
        public string Email { get; set; }
    }

        public async Task OnGetAsync()
        {
            Departments = await _applicationDbContext.Employees
                                        .Where(e => !string.IsNullOrEmpty(e.Department))
                                        .Select(e => e.Department)
                                        .Distinct()
                                        .ToListAsync();
        }
    }
}
