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
                .AsQueryable();


            var modifiers = await query
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

            foreach (var productId in model.ProductIds)
            {
                _context.ProductModifierProducts.Add(new ProductModifierProduct
                {
                    ProductId = productId,
                    ProductModifierId = model.Id
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Modificador creado correctamente.";

            return RedirectToAction(nameof(Index), new { showAll = true });
        }

        //==========================================
        // EDIT GET
        //==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var modifier = await _context.ProductModifiers
                .Include(x => x.ProductModifierProducts)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (modifier == null)
            {
                TempData["Error"] = "Modificador no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            modifier.ProductIds = modifier.ProductModifierProducts
                .Select(x => x.ProductId)
                .ToList();

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

            var relations = await _context.ProductModifierProducts
                .Where(x => x.ProductModifierId == modifier.Id)
                .ToListAsync();

            _context.ProductModifierProducts.RemoveRange(relations);

            foreach (var productId in model.ProductIds)
            {
                _context.ProductModifierProducts.Add(new ProductModifierProduct
                {
                    ProductId = productId,
                    ProductModifierId = modifier.Id
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Modificador actualizado correctamente.";

            return RedirectToAction(nameof(Index), new { showAll = true });
        }

        //==========================================
        // MÉTODOS PRIVADOS
        //==========================================
        private async Task LoadProducts()
        {
            ViewBag.Products = await _context.Products
                .Where(x => x.IsAvailable)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}