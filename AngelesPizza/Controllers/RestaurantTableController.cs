using AngelesPizza.Data;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class RestaurantTableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantTableController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // INDEX
        // ==========================================
        public async Task<IActionResult> Index(string? search, bool showAll = false)
        {
            if (string.IsNullOrWhiteSpace(search) && !showAll)
            {
                return View(null);
            }

            IQueryable<RestaurantTable> query = _context.RestaurantTables;

            if (!showAll)
            {
                query = query.Where(t =>
                    t.Name.Contains(search!));
            }

            var tables = await query
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(tables);
        }

        // ==========================================
        // CREATE GET
        // ==========================================
        public IActionResult Create()
        {
            return View(new RestaurantTable());
        }

        // ==========================================
        // CREATE POST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RestaurantTable model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.RestaurantTables.Add(model);

            await _context.SaveChangesAsync();

            TempData["Success"] = "La mesa fue creada correctamente.";

            return RedirectToAction(nameof(Index), new { showAll = true });
        }

        // ==========================================
        // EDIT GET
        // ==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var table = await _context.RestaurantTables.FindAsync(id);

            if (table == null)
            {
                TempData["Error"] = "La mesa no existe.";

                return RedirectToAction(nameof(Index));
            }

            return View(table);
        }

        // ==========================================
        // EDIT POST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RestaurantTable model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var table = await _context.RestaurantTables.FindAsync(model.Id);

            if (table == null)
            {
                TempData["Error"] = "La mesa no existe.";

                return RedirectToAction(nameof(Index));
            }

            table.Name = model.Name;
            table.Capacity = model.Capacity;
            table.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "La mesa fue actualizada correctamente.";

            return RedirectToAction(nameof(Index), new { showAll = true });
        }
    }
}