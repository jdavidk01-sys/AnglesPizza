using AngelesPizza.Data;
using AngelesPizza.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class KitchenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KitchenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // INDEX
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.OrderDetailModifiers)
                        .ThenInclude(m => m.ProductModifier)
                .Where(o =>
                    o.Status == OrderStatus.Queued ||
                    o.Status == OrderStatus.Preparing)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }
    }
}