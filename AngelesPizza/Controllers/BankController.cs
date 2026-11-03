using AngelesPizza.Data;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class BankController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, bool showAll = false)
        {
            if (string.IsNullOrWhiteSpace(search) && !showAll)
            {
                return View(null);
            }

            IQueryable<Bank> query = _context.Banks;

            if (!showAll)
            {
                query = query.Where(b =>
                    b.Name.Contains(search!) ||
                    (b.SwiftCode != null && b.SwiftCode.Contains(search)));
            }

            var banks = await query
                .OrderBy(b => b.Name)
                .ToListAsync();

            return View(banks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bank model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            model.CreatedAt = DateTime.UtcNow;
            model.IsActive = true;

            _context.Banks.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El banco fue creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string? swiftCode, bool isActive)
        {
            var bank = await _context.Banks.FindAsync(id);

            if (bank == null)
                return NotFound();

            bank.Name = name;
            bank.SwiftCode = swiftCode;
            bank.IsActive = isActive;

            _context.Banks.Update(bank);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El banco fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var bank = await _context.Banks.FindAsync(id);

            if (bank == null)
                return NotFound();

            bank.IsActive = false;

            _context.Banks.Update(bank);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El banco fue desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
