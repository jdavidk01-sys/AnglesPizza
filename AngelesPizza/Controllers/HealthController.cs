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
                await _context.Database.OpenConnectionAsync();

                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT 1";

                var result = await command.ExecuteScalarAsync();

                await _context.Database.CloseConnectionAsync();

                return Ok(new
                {
                    ok = true,
                    result
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    ok = false
                });
            }
        }
    }
}