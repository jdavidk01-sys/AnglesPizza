using AngelesPizza.Data;
using AngelesPizza.Enums;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class OrderDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
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
                    o.Status != OrderStatus.Delivered &&
                    o.Status != OrderStatus.Cancelled)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.StatusClass = new Func<OrderStatus, string>(GetStatusClass);

            return View(orders);
        }

        private string GetStatusClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Created => "bg-secondary",
                OrderStatus.Queued => "bg-warning text-dark",
                OrderStatus.Preparing => "bg-primary",
                OrderStatus.Ready => "bg-success",
                _ => "bg-dark"
            };
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return Json(new { ok = false });

            order.Status = status;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = status,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.OrderDetailModifiers)
                        .ThenInclude(m => m.ProductModifier)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            ViewBag.OrderTypes = new SelectList(
                Enum.GetValues<OrderType>()
                    .Select(x => new
                    {
                        Id = (int)x,
                        Name = x.ToString()
                    }),
                "Id",
                "Name",
                (int)order.OrderType);

            ViewBag.Customers = new SelectList(
                await _context.Customers
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                order.CustomerId);

            ViewBag.Categories = await _context.Categories
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return Json(new { ok = false });

            order.Status = OrderStatus.Cancelled;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Cancelled,
                CreatedAt = DateTime.UtcNow,
                Notes = "Pedido cancelado."
            });

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }
    }
}