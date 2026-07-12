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
                .Include(x => x.RestaurantTable)
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

        // Edicion desde Dashboard
        [HttpGet]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.OrderDetailModifiers)
                        .ThenInclude(m => m.ProductModifier)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return Json(new List<object>());

            var items = order.OrderDetails
                .Select(d => new
                {
                    detailId = d.Id,
                    productId = d.ProductId,
                    product = d.Product.Name,
                    price = d.Price,
                    notes = d.Notes,
                    modifiers = d.OrderDetailModifiers.Select(m => new
                    {
                        id = m.ProductModifierId,
                        name = m.ProductModifier.Name,
                        extraCost = m.ExtraCost
                    }).ToList(),
                    total = d.Price + d.OrderDetailModifiers.Sum(x => x.ExtraCost)
                })
                .ToList();

            return Json(items);
        }

        //Agregas producto editado
        [HttpPost]
        public async Task<IActionResult> AddProduct(
            int orderId,
            int productId,
            string? notes,
            List<int> modifierIds)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                return Json(new { ok = false });

            var product = await _context.Products
                .FirstAsync(x => x.Id == productId);

            var detail = new OrderDetail
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Price = product.Price,
                Status = OrderStatus.Queued,
                Notes = notes
            };

            _context.OrderDetails.Add(detail);

            await _context.SaveChangesAsync();

            if (modifierIds.Any())
            {
                var modifiers = await _context.ProductModifiers
                    .Where(x => modifierIds.Contains(x.Id))
                    .ToListAsync();

                foreach (var modifier in modifiers)
                {
                    _context.OrderDetailModifiers.Add(new OrderDetailModifier
                    {
                        OrderDetailId = detail.Id,
                        ProductModifierId = modifier.Id,
                        ExtraCost = modifier.ExtraCost
                    });
                }

                await _context.SaveChangesAsync();
            }

            order.SubTotal = await _context.OrderDetails
                .Where(x => x.OrderId == order.Id)
                .Select(x => x.Price + x.OrderDetailModifiers.Sum(m => m.ExtraCost))
                .SumAsync();

            order.Total = order.SubTotal + order.Tax;

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }


        //Retirar producto de orden
        [HttpPost]
        public async Task<IActionResult> RemoveProduct(int detailId)
        {
            var detail = await _context.OrderDetails
                .Include(x => x.OrderDetailModifiers)
                .FirstOrDefaultAsync(x => x.Id == detailId);

            if (detail == null)
                return Json(new { ok = false });

            var order = await _context.Orders
                .FirstAsync(x => x.Id == detail.OrderId);

            _context.OrderDetailModifiers.RemoveRange(detail.OrderDetailModifiers);

            _context.OrderDetails.Remove(detail);

            await _context.SaveChangesAsync();

            order.SubTotal = await _context.OrderDetails
                .Where(x => x.OrderId == order.Id)
                .Select(x => x.Price + x.OrderDetailModifiers.Sum(m => m.ExtraCost))
                .SumAsync();

            order.Total = order.SubTotal + order.Tax;

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }
    }
}