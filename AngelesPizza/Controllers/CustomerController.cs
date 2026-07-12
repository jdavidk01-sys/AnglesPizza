using AngelesPizza.Data;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener un cliente
        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Where(x => x.IsActive && x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Phone,
                    x.Address,
                    x.Reference
                })
                .FirstOrDefaultAsync();

            if (customer == null)
                return Json(new { ok = false });

            return Json(new
            {
                ok = true,
                customer
            });
        }

        // Crear cliente
        [HttpPost]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false });

            _context.Customers.Add(model);

            await _context.SaveChangesAsync();

            return Json(new
            {
                ok = true,
                id = model.Id,
                name = model.Name
            });
        }

        // Actualizar cliente
        [HttpPost]
        public async Task<IActionResult> Update(Customer model)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (customer == null)
                return Json(new { ok = false });

            customer.Name = model.Name;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.Reference = model.Reference;

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }
    }
}