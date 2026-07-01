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
        public async Task<IActionResult> Create(Product model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();

                await ImageFile.CopyToAsync(ms);

                model.ImageData = ms.ToArray();
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
        public async Task<IActionResult> Edit(Product model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.Id);

            if (product == null)
                return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;
            product.IsAvailable = model.IsAvailable;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();

                await ImageFile.CopyToAsync(ms);

                product.ImageData = ms.ToArray();
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "El producto fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetImage(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null || product.ImageData == null)
                return NotFound();

            return File(product.ImageData, "image/jpeg");
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