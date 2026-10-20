using AngelesPizza.Data;
using AngelesPizza.Enums;
using AngelesPizza.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;

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
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.CashRegisters.AsQueryable();

            // Filtro por fecha inicial (desde el comienzo del día)
            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(r => r.OpenedAt >= start);
            }

            // Filtro por fecha final (hasta el último milisegundo del día)
            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.OpenedAt <= end);
            }

            var registers = await query
                .OrderByDescending(r => r.OpenedAt)
                .ToListAsync();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

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
                .Include(r => r.Payments)
                    .ThenInclude(p => p.Bank)
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
                .Include(r => r.Payments)
                    .ThenInclude(p => p.Bank)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (register == null)
                return NotFound();

            return View(register);
        }

        // GET: CashRegister/ExportExcel
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.CashRegisters.AsQueryable();

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(r => r.OpenedAt >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.OpenedAt <= end);
            }

            var registers = await query
                .OrderBy(r => r.OpenedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Historial de Cajas");

                // 1. Encabezados de la tabla
                string[] headers = {
            "ID", "Apertura", "Cierre", "Monto Inicial",
            "Ventas Efec.", "Ventas Transf.", "Ventas Tarj.",
            "Esperado", "Real", "Diferencia", "Estado", "Notas"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#212529");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // 2. Filas de datos
                int row = 2;
                foreach (var item in registers)
                {
                    bool isOpen = item.IsClosed == false || item.IsClosed == null;

                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.OpenedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 3).Value = item.ClosedAt.HasValue ? item.ClosedAt.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "-";
                    worksheet.Cell(row, 4).Value = item.InitialAmount;
                    worksheet.Cell(row, 5).Value = item.TotalCashSales;
                    worksheet.Cell(row, 6).Value = item.TotalTransferSales;
                    worksheet.Cell(row, 7).Value = item.TotalCardSales;
                    worksheet.Cell(row, 8).Value = item.FinalCashExpected;
                    worksheet.Cell(row, 9).Value = item.FinalCashReal ?? 0;
                    worksheet.Cell(row, 10).Value = !isOpen ? (item.CashDifference ?? 0) : 0;
                    worksheet.Cell(row, 11).Value = isOpen ? "Abierta" : "Cerrada";
                    worksheet.Cell(row, 12).Value = item.Notes ?? "";

                    // Formato de moneda para columnas de dinero
                    for (int col = 4; col <= 10; col++)
                    {
                        worksheet.Cell(row, col).Style.NumberFormat.Format = "$#,##0";
                    }

                    row++;
                }

                // 3. Fila de Totales con fórmulas SUMA
                if (registers.Any())
                {
                    worksheet.Cell(row, 1).Value = "TOTALES";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;

                    worksheet.Cell(row, 4).FormulaA1 = $"SUM(D2:D{row - 1})";
                    worksheet.Cell(row, 5).FormulaA1 = $"SUM(E2:E{row - 1})";
                    worksheet.Cell(row, 6).FormulaA1 = $"SUM(F2:F{row - 1})";
                    worksheet.Cell(row, 7).FormulaA1 = $"SUM(G2:G{row - 1})";
                    worksheet.Cell(row, 8).FormulaA1 = $"SUM(H2:H{row - 1})";
                    worksheet.Cell(row, 9).FormulaA1 = $"SUM(I2:I{row - 1})";
                    worksheet.Cell(row, 10).FormulaA1 = $"SUM(J2:J{row - 1})";

                    var totalRow = worksheet.Row(row);
                    totalRow.Style.Font.Bold = true;
                    totalRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#E9ECEF");

                    for (int col = 4; col <= 10; col++)
                    {
                        worksheet.Cell(row, col).Style.NumberFormat.Format = "$#,##0";
                    }
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Historial_Cajas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
        }
    }
}