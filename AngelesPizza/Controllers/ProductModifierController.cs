using AngelesPizza.Data;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class ProductModifierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductModifierController(ApplicationDbContext context)
        {
            _context = context;
        }

        //==========================================
        // INDEX
        //==========================================
        public async Task<IActionResult> Index(string? search, bool showAll = false)
        {
            if (string.IsNullOrWhiteSpace(search) && !showAll)
                return View(null);

            var query = _context.ProductModifiers
                .Include(x => x.Product)
                .AsQueryable();

            if (!showAll)
            {
                search = search!.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    x.Product.Name.Contains(search));
            }

            var modifiers = await query
                .OrderBy(x => x.Product.Name)
                .ThenBy(x => x.Name)
                .ToListAsync();

            return View(modifiers);
        }

        //==========================================
        // CREATE GET
        //==========================================
        public async Task<IActionResult> Create()
        {
            await LoadProducts();

            return View(new ProductModifier());
        }

        //==========================================
        // CREATE POST
        //==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModifier model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProducts();
                return View(model);
            }

            _context.ProductModifiers.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Modificador creado correctamente.";

            return RedirectToAction(nameof(Index), new { showAll = true });
        }

        //==========================================
        // EDIT GET
        //==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var modifier = await _context.ProductModifiers.FindAsync(id);

            if (modifier == null)
            {
                TempData["Error"] = "Modificador no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            await LoadProducts();

            return View(modifier);
        }

        //==========================================
        // EDIT POST
        //==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModifier model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProducts();
                return View(model);
            }

            var modifier = await _context.ProductModifiers.FindAsync(model.Id);

            if (modifier == null)
            {
                TempData["Error"] = "Modificador no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            modifier.Name = model.Name;
            modifier.ExtraCost = model.ExtraCost;
            modifier.IsActive = model.IsActive;
            modifier.ProductId = model.ProductId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Modificador actualizado correctamente.";

            return RedirectToAction(nameof(Index), new { showAll = true });
        }

        //==========================================
        // MÉTODOS PRIVADOS
        //==========================================
        private async Task LoadProducts()
        {
            var products = await _context.Products
                .Where(x => x.IsAvailable)
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.Products = new SelectList(products, "Id", "Name");
        }
    }
}