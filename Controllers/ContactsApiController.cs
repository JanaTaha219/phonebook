using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;

namespace WebApplication8.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/contacts?page=1
        [HttpGet]
        public async Task<IActionResult> GetContacts([FromQuery] int page = 1)
        {
            int pageSize = 10;
            var contacts = await _context.Employees
                                         .OrderBy(e => e.Name)
                                         .Skip((page - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToListAsync();

            return Ok(contacts);
        }
    }
}
