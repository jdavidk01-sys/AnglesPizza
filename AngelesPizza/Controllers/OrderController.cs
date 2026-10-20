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

        var customers = await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                Text = string.IsNullOrWhiteSpace(c.Phone)
                    ? c.Name
                    : $"{c.Name} - {c.Phone}"
            })
            .ToListAsync();

        ViewBag.Customers = new SelectList(
            customers,
            "Id",
            "Text");

        var activeTables = _context.RestaurantTables
            .Where(x => x.IsActive)
            .ToList(); 

        ViewBag.Tables = new SelectList(
            activeTables
                .OrderBy(x => int.TryParse(x.Name, out int num) ? num : 9999)
                .ThenBy(x => x.Name) 
                .ToList(),
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
    public async Task<IActionResult> GetModifiers(int productId)
    {
        var modifiers = await _context.ProductModifierProducts
            .Where(x => x.ProductId == productId && x.ProductModifier.IsActive)
            .OrderBy(x => x.ProductModifier.Name)
            .Select(x => new
            {
                x.ProductModifier.Id,
                x.ProductModifier.Name,
                x.ProductModifier.ExtraCost
            })
            .ToListAsync();

        return Json(modifiers);
    }

    //Agragar Productos
    [HttpPost]
    public async Task<IActionResult> AddProduct(
        int productId,
        int quantity,
        string? notes,
        List<int> modifierIds)
    {

        if (quantity < 1)
            quantity = 1;

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
            Quantity = quantity,
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

    // Crear pedido
    [HttpPost]
    public async Task<IActionResult> Confirm(OrderType orderType, int? customerId, int? restaurantTableId, int deliveryCost = 0)
    {
        // 1. Validar que exista una caja abierta antes de permitir crear el pedido
        var activeCashRegister = await _context.CashRegisters
            .FirstOrDefaultAsync(r => r.IsClosed == false || r.IsClosed == null);

        if (activeCashRegister == null)
        {
            return Json(new
            {
                ok = false,
                message = "No hay ninguna caja abierta. Debe abrir la caja antes de registrar pedidos."
            });
        }

        var tempOrder = HttpContext.Session
            .GetObject<List<TempOrderItem>>("ORDER");

        if (tempOrder == null || !tempOrder.Any())
            return Json(new { ok = false, message = "El pedido está vacío." });

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
            Status = OrderStatus.Creado,
            DeliveryCost = deliveryCost
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in tempOrder)
        {
            var detail = new OrderDetail
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
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
        order.Tax = 0;
        order.Total = order.SubTotal + order.Tax + order.DeliveryCost;

        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("ORDER");

        return Json(new
        {
            ok = true,
            orderId = order.Id
        });
    }

    // Recibir el pago
    [HttpPost]
    public async Task<IActionResult> Pay(
        int orderId,
        PaymentType paymentType,
        int receivedAmount,
        string unitPrices,
        int? bankId = null)
    {
        // 1. Verificar que exista una caja activa
        var activeCashRegister = await _context.CashRegisters
            .FirstOrDefaultAsync(r => r.IsClosed == false || r.IsClosed == null);

        if (activeCashRegister == null)
        {
            return Json(new
            {
                ok = false,
                message = "No hay ninguna caja abierta. Debe abrir la caja antes de procesar el pago."
            });
        }

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.OrderDetailModifiers)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return Json(new
            {
                ok = false,
                message = "La orden no existe."
            });
        }

        if (order.Status != OrderStatus.Creado)
        {
            return Json(new
            {
                ok = false,
                message = "Esta orden ya fue procesada."
            });
        }

        List<OrderDetailPriceDto>? prices;

        try
        {
            prices = System.Text.Json.JsonSerializer
                .Deserialize<List<OrderDetailPriceDto>>(
                    unitPrices,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }
        catch (System.Text.Json.JsonException)
        {
            return Json(new
            {
                ok = false,
                message = "Los precios enviados no tienen un formato válido."
            });
        }

        if (prices == null)
        {
            return Json(new
            {
                ok = false,
                message = "No se recibieron los precios de los productos."
            });
        }

        if (prices.Count != order.OrderDetails.Count ||
            prices.Select(x => x.OrderDetailId).Distinct().Count() != prices.Count ||
            prices.Any(x => !order.OrderDetails.Any(d => d.Id == x.OrderDetailId)))
        {
            return Json(new
            {
                ok = false,
                message = "Los productos enviados no corresponden a la orden."
            });
        }

        var subtotal = 0;

        foreach (var detail in order.OrderDetails)
        {
            var price = prices
                .FirstOrDefault(x => x.OrderDetailId == detail.Id);

            if (price == null)
            {
                return Json(new
                {
                    ok = false,
                    message = "No se recibió el precio de uno de los productos."
                });
            }

            if (price.UnitPrice < 0)
            {
                return Json(new
                {
                    ok = false,
                    message = "El precio de un producto no puede ser negativo."
                });
            }

            var modifierCost = detail.OrderDetailModifiers.Sum(x => x.ExtraCost);

            if (price.UnitPrice < modifierCost)
            {
                return Json(new
                {
                    ok = false,
                    message = "El precio unitario no puede ser menor al costo de los adicionales."
                });
            }

            detail.Price = price.UnitPrice - modifierCost;
            subtotal += price.UnitPrice * detail.Quantity;
        }

        order.SubTotal = subtotal;
        order.Total = order.SubTotal + order.Tax + order.DeliveryCost;

        if (receivedAmount < 0)
        {
            return Json(new
            {
                ok = false,
                message = "El valor recibido no puede ser negativo."
            });
        }

        if (receivedAmount < order.Total)
        {
            return Json(new
            {
                ok = false,
                message = "El valor recibido es menor al total."
            });
        }

        // Validar que el banco sea obligatorio para transferencias
        if (paymentType == PaymentType.Transferencia && (!bankId.HasValue || bankId.Value <= 0))
        {
            return Json(new
            {
                ok = false,
                message = "Debe seleccionar un banco para pagos por transferencia."
            });
        }

        // Validar que el banco exista si se proporciona
        if (bankId.HasValue && bankId.Value > 0)
        {
            var bank = await _context.Banks.FindAsync(bankId.Value);
            if (bank == null)
            {
                return Json(new
                {
                    ok = false,
                    message = "El banco seleccionado no existe."
                });
            }

            if (!bank.IsActive)
            {
                return Json(new
                {
                    ok = false,
                    message = "El banco seleccionado no está activo."
                });
            }
        }

        // 2. Crear pago asociado a la caja activa
        var payment = new Payment
        {
            OrderId = order.Id,
            CashRegisterId = activeCashRegister.Id,
            PaymentType = paymentType,
            Amount = order.Total,
            ReceivedAmount = receivedAmount,
            ChangeAmount = receivedAmount - order.Total,
            BankId = bankId.HasValue && bankId.Value > 0 ? bankId.Value : null
        };

        _context.Payments.Add(payment);

        order.Status = OrderStatus.Fila;

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Fila,
            Notes = $"Pago recibido vía {paymentType}."
        });

        await _context.SaveChangesAsync();

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

        // Cargar bancos activos para la transferencia
        var banks = await _context.Banks
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();

        ViewBag.Banks = new SelectList(banks, "Id", "Name");

        return View(order);
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

public class OrderDetailPriceDto
{
    public int OrderDetailId { get; set; }
    public int UnitPrice { get; set; }
}