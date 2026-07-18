using AngelesPizza.Data;
using AngelesPizza.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AngelesPizza.Extensions;
using AngelesPizza.Models;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lista de pedidos
    public async Task<IActionResult> Index(string? search, bool showAll = false)
    {
        if (string.IsNullOrWhiteSpace(search) && !showAll)
            return View(null);

        var query = _context.Orders
            .Include(o => o.Customer)
            .AsQueryable();

        if (!showAll)
        {
            query = query.Where(o =>
                o.OrderNumber.ToString().Contains(search!) ||
                (o.Customer != null && o.Customer.Name.Contains(search)));
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // Pantalla del POS
    public async Task<IActionResult> New()
    {
        ViewBag.OrderTypes = new SelectList(
            Enum.GetValues<OrderType>()
                .Select(x => new
                {
                    Id = (int)x,
                    Name = x.ToString()
                }),
            "Id",
            "Name");

        ViewBag.Categories = await _context.Categories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.Customers = new SelectList(
            await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id",
            "Name");

        ViewBag.Tables = new SelectList(
            _context.RestaurantTables
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name),
            "Id",
            "Name");

        return View();
    }

    // Carga de productos
    [HttpGet]
    public async Task<IActionResult> GetProducts(int categoryId)
    {
        var products = await _context.Products
            .Where(p => p.IsAvailable && p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                HasImage = p.ImageData != null
            })
            .ToListAsync();

        return Json(products);
    }


    //Carga modificadores
    [HttpGet]
    public async Task<IActionResult> GetModifiers()
    {
        var modifiers = await _context.ProductModifiers
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.ExtraCost
            })
            .ToListAsync();

        return Json(modifiers);
    }

    //Agragar Productos
    [HttpPost]
    public async Task<IActionResult> AddProduct(
        int productId,
        string? notes,
        List<int> modifierIds)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return BadRequest();

        var modifiers = await _context.ProductModifiers
            .Where(m => modifierIds.Contains(m.Id))
            .ToListAsync();

        var order = HttpContext.Session
            .GetObject<List<TempOrderItem>>("ORDER");

        if (order == null)
            order = new List<TempOrderItem>();

        order.Add(new TempOrderItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Price = product.Price,
            Notes = notes,
            ModifierIds = modifiers.Select(x => x.Id).ToList(),
            ModifierNames = modifiers.Select(x => x.Name).ToList(),
            ExtraCost = modifiers.Sum(x => x.ExtraCost)
        });

        HttpContext.Session.SetObject("ORDER", order);

        return Json(new
        {
            ok = true,
            items = order.Count
        });
    }

    //Carga el pedido
    [HttpGet]
    public IActionResult GetCurrentOrder()
    {
        var order = HttpContext.Session
            .GetObject<List<TempOrderItem>>("ORDER") ?? new List<TempOrderItem>();

        return Json(order);
    }

    //Borrar producto
    [HttpPost]
    public IActionResult RemoveProduct(int index)
    {
        var order = HttpContext.Session
            .GetObject<List<TempOrderItem>>("ORDER") ?? new List<TempOrderItem>();

        if (index >= 0 && index < order.Count)
        {
            order.RemoveAt(index);

            HttpContext.Session.SetObject("ORDER", order);
        }

        return Json(new
        {
            ok = true
        });
    }

    //Crear pedido
    [HttpPost]
    public async Task<IActionResult> Confirm(OrderType orderType, int? customerId, int? restaurantTableId)
    {
        var tempOrder = HttpContext.Session
            .GetObject<List<TempOrderItem>>("ORDER");

        if (tempOrder == null || !tempOrder.Any())
            return Json(new { ok = false });

        if (orderType == OrderType.Mesa && restaurantTableId == null)
        {
            return Json(new
            {
                ok = false,
                message = "Debes seleccionar una mesa."
            });
        }

        var order = new Order
        {
            OrderNumber = await _context.Orders.CountAsync() + 1,
            CreatedAt = DateTime.UtcNow,
            OrderType = orderType,
            CustomerId = customerId,
            RestaurantTableId = restaurantTableId,
            Status = OrderStatus.Creado
        };

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        foreach (var item in tempOrder)
        {
            var detail = new OrderDetail
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Price = item.Price,
                Status = OrderStatus.Fila,
                Notes = item.Notes
            };

            _context.OrderDetails.Add(detail);

            await _context.SaveChangesAsync();

            foreach (var modifierId in item.ModifierIds)
            {
                var modifier = await _context.ProductModifiers
                    .FirstAsync(x => x.Id == modifierId);

                _context.OrderDetailModifiers.Add(new OrderDetailModifier
                {
                    OrderDetailId = detail.Id,
                    ProductModifierId = modifier.Id,
                    ExtraCost = modifier.ExtraCost
                });
            }
        }

        order.SubTotal = tempOrder.Sum(x => x.Total);

        // Por ahora sin IVA
        order.Tax = 0;

        order.Total = order.SubTotal + order.Tax;

        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("ORDER");

        return Json(new
        {
            ok = true,
            orderId = order.Id
        });
    }

    public async Task<IActionResult> ReceivePayment(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.OrderDetailModifiers)
                    .ThenInclude(m => m.ProductModifier)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return View(order);
    }

    //Recibir el pago
    [HttpPost]
    public async Task<IActionResult> Pay(
    int orderId,
    PaymentType paymentType,
    int receivedAmount)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
            return Json(new { ok = false });

        if (receivedAmount < order.Total)
        {
            return Json(new
            {
                ok = false,
                message = "El valor recibido es menor al total."
            });
        }

        var payment = new Payment
        {
            OrderId = order.Id,
            PaymentType = paymentType,
            Amount = order.Total,
            ReceivedAmount = receivedAmount,
            ChangeAmount = receivedAmount - order.Total
        };

        _context.Payments.Add(payment);

        order.Status = OrderStatus.Fila;

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Fila,
            Notes = "Pago recibido."
        });

        await _context.SaveChangesAsync();

        return Json(new
        {
            ok = true,
            orderId = order.Id
        });
    }

    //Impresion de pedidos
    public async Task<IActionResult> PrintTicket(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.RestaurantTable)
            .Include(o => o.Payments)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.OrderDetailModifiers)
                    .ThenInclude(m => m.ProductModifier)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return View(order);
    }

    //Impresion Comanda
    public async Task<IActionResult> PrintKitchen(int id, string copy = "COCINA")
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.RestaurantTable)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.OrderDetailModifiers)
                    .ThenInclude(m => m.ProductModifier)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        ViewBag.Copy = copy;

        return View(order);
    }
}