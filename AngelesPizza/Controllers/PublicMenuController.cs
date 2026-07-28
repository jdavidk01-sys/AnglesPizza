using AngelesPizza.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class PublicMenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicMenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /PublicMenu
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Filtrar solo productos disponibles
            foreach (var category in categories)
            {
                category.Products = category.Products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.Name)
                    .ToList();
            }

            // Remover categorías sin productos disponibles
            var categoriesWithProducts = categories
                .Where(c => c.Products.Any())
                .ToList();

            return View(categoriesWithProducts);
        }

        // GET: /PublicMenu/GetProductImage/5
        public async Task<IActionResult> GetProductImage(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product?.ImageData == null)
                return File("~/images/no-image.png", "image/png");

            return File(product.ImageData, "image/jpeg");
        }

        // GET: /PublicMenu/GetProductModifiers/5
        public async Task<IActionResult> GetProductModifiers(int id)
        {
            var modifiers = await _context.ProductModifierProducts
                .Include(pmp => pmp.ProductModifier)
                .Where(pmp => pmp.ProductId == id)
                .Select(pmp => new
                {
                    id = pmp.ProductModifier.Id,
                    name = pmp.ProductModifier.Name,
                    extraCost = pmp.ProductModifier.ExtraCost
                })
                .ToListAsync();

            return Json(modifiers);
        }
    }
}
