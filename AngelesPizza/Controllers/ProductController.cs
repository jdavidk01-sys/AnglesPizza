using AngelesPizza.Data;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===========================
        // INDEX
        // ===========================
        public async Task<IActionResult> Index(string? search, bool showAll = false)
        {
            // Primera vez que entra al módulo
            if (string.IsNullOrWhiteSpace(search) && !showAll)
            {
                return View(null);
            }

            IQueryable<Product> query = _context.Products
                .Include(p => p.Category);

            if (!showAll)
            {
                query = query.Where(p =>
                    p.Name.Contains(search!) ||
                    (p.Description != null && p.Description.Contains(search)) ||
                    p.Category.Name.Contains(search));
            }

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // ===========================
        // CREATE
        // ===========================
        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El producto fue creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // EDIT
        // ===========================
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            await LoadCategories();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            _context.Products.Update(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El producto fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // MÉTODOS PRIVADOS
        // ===========================
        private async Task LoadCategories()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name");
        }
    }
}