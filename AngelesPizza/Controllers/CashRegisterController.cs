using AngelesPizza.Data;
using AngelesPizza.Enums;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelesPizza.Controllers
{
    public class CashRegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CashRegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Historial de Cajas
        public async Task<IActionResult> Index()
        {
            var registers = await _context.CashRegisters
                .OrderByDescending(r => r.OpenedAt)
                .ToListAsync();

            return View(registers);
        }

        // Pantalla de Apertura
        public async Task<IActionResult> Open()
        {
            var activeRegister = await _context.CashRegisters
                .FirstOrDefaultAsync(r => r.IsClosed == false || r.IsClosed == null);

            if (activeRegister != null)
                return RedirectToAction(nameof(Status));

            var lastRegister = await _context.CashRegisters
                .Where(r => r.IsClosed == true)
                .OrderByDescending(r => r.ClosedAt)
                .FirstOrDefaultAsync();

            int suggestedInitial = lastRegister?.FinalCashReal ?? 0;

            ViewBag.SuggestedInitial = suggestedInitial;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(int initialAmount)
        {
            var activeRegister = await _context.CashRegisters
                .FirstOrDefaultAsync(r => r.IsClosed == false || r.IsClosed == null);

            if (activeRegister != null)
                return RedirectToAction(nameof(Status));

            var newRegister = new CashRegister
            {
                OpenedAt = DateTime.UtcNow,
                InitialAmount = initialAmount,
                IsClosed = false
            };

            _context.CashRegisters.Add(newRegister);
            await _context.SaveChangesAsync();

            return RedirectToAction("New", "Order");
        }

        // Ver Estado de la Caja Activa / Pantalla de Cierre
        public async Task<IActionResult> Status()
        {
            var activeRegister = await _context.CashRegisters
                .Include(r => r.Payments)
                    .ThenInclude(p => p.Order)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(r => r.IsClosed == false || r.IsClosed == null);

            if (activeRegister == null)
                return RedirectToAction(nameof(Open));

            var cashSales = activeRegister.Payments
                .Where(p => p.PaymentType == PaymentType.Efectivo)
                .Sum(p => p.Amount);

            var transferSales = activeRegister.Payments
                .Where(p => p.PaymentType == PaymentType.Transferencia)
                .Sum(p => p.Amount);

            var cardSales = activeRegister.Payments
                .Where(p => p.PaymentType == PaymentType.Tarjeta)
                .Sum(p => p.Amount);

            ViewBag.CashSales = cashSales;
            ViewBag.TransferSales = transferSales;
            ViewBag.CardSales = cardSales;
            ViewBag.CashExpected = activeRegister.InitialAmount + cashSales;

            return View(activeRegister);
        }

        // Procesar Cierre de Caja
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int finalCashReal, string? notes)
        {
            var activeRegister = await _context.CashRegisters
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.IsClosed == false || r.IsClosed == null);

            if (activeRegister == null)
                return NotFound("No hay ninguna caja abierta.");

            int cashSales = activeRegister.Payments
                .Where(p => p.PaymentType == PaymentType.Efectivo)
                .Sum(p => p.Amount);

            int transferSales = activeRegister.Payments
                .Where(p => p.PaymentType == PaymentType.Transferencia)
                .Sum(p => p.Amount);

            int cardSales = activeRegister.Payments
                .Where(p => p.PaymentType == PaymentType.Tarjeta)
                .Sum(p => p.Amount);

            int cashExpected = activeRegister.InitialAmount + cashSales;

            activeRegister.ClosedAt = DateTime.UtcNow;
            activeRegister.TotalCashSales = cashSales;
            activeRegister.TotalTransferSales = transferSales;
            activeRegister.TotalCardSales = cardSales;
            activeRegister.FinalCashExpected = cashExpected;
            activeRegister.FinalCashReal = finalCashReal;
            activeRegister.CashDifference = finalCashReal - cashExpected;
            activeRegister.Notes = notes;
            activeRegister.IsClosed = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Ver Detalle de una Caja Pasada
        public async Task<IActionResult> Details(int id)
        {
            var register = await _context.CashRegisters
                .Include(r => r.Payments)
                    .ThenInclude(p => p.Order)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (register == null)
                return NotFound();

            return View(register);
        }
    }
}