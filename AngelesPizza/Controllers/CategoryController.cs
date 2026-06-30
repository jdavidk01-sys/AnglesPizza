using AngelesPizza.Data;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
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

            IQueryable<Category> query = _context.Categories;

            if (!showAll)
            {
                query = query.Where(c =>
                    c.Name.Contains(search!) ||
                    (c.Description != null && c.Description.Contains(search)));
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // ===========================
        // CREATE
        // ===========================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "La categoría fue creada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // EDIT
        // ===========================
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Categories.Update(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "La categoría fue actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}