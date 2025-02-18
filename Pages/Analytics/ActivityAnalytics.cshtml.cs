using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;

namespace WebApplication8.Pages.Analytics
{
    [Authorize(Policy = "CanViewAnalytics")]
    public class AnalyticsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Employee> TopSearchedUsers { get; set; } = new();
        public List<string> Departments { get; set; } = new(); // Store all unique departments

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Department { get; set; }

        public Dictionary<string, int> DepartmentSearchCounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Fetch all departments from the database
            Departments = await _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .Select(e => e.Department!)
                .Distinct()
                .ToListAsync();

            var query = _context.Employees.AsQueryable();

            // Apply department filter
            if (!string.IsNullOrEmpty(Department))
            {
                query = query.Where(e => e.Department == Department);
            }

            // Apply date range filter
            if (StartDate.HasValue && EndDate.HasValue)
            {
                query = query.Where(e => e.SearchHistories.Any(sh => sh.SearchDate >= StartDate.Value && sh.SearchDate <= EndDate.Value.AddDays(1)));
            }

            TopSearchedUsers = await query
                .OrderByDescending(e => e.counter)
                .Take(10)
                .ToListAsync();

            // Get department search counts
            var departmentQuery = _context.SearchHistories.AsQueryable();

            if (StartDate.HasValue && EndDate.HasValue)
            {
                departmentQuery = departmentQuery.Where(sh => sh.SearchDate >= StartDate.Value && sh.SearchDate <= EndDate.Value.AddDays(1));
            }

            DepartmentSearchCounts = await departmentQuery
             .GroupBy(sh => sh.Employee.Department ?? "Unknown") // Group by department, defaulting to "Unknown" for null departments
             .Select(g => new { Department = g.Key, Count = g.Count() })
             .Where(g => g.Department != "Unknown") // Filter out the "Unknown" department
             .OrderByDescending(g => g.Count)
             .ToDictionaryAsync(g => g.Department, g => g.Count);
        }
    }
}
