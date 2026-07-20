using AngelesPizza.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class HealthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Database()
        {
            try
            {
                await _context.Products
                    .AsNoTracking()
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    ok = true,
                    time = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}