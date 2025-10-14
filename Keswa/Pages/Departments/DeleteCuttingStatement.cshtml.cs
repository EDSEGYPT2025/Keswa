using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class DeleteCuttingStatementModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteCuttingStatementModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CuttingStatement CuttingStatement { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CuttingStatement = await _context.CuttingStatements
                .Include(cs => cs.WorkOrder)
                .Include(cs => cs.Product)
                .Include(cs => cs.Customer)
                .Include(cs => cs.Material).ThenInclude(m => m.Color)
                .Include(cs => cs.Worker)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (CuttingStatement == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statementToDelete = await _context.CuttingStatements.FindAsync(id);

            if (statementToDelete != null)
            {
                // عكس تأثير الإنتاج عن طريق إضافة سجل بكمية سالبة
                var productionLog = new ProductionLog
                {
                    WorkOrderId = statementToDelete.WorkOrderId,
                    WorkerId = statementToDelete.WorkerId,
                    Department = Department.Cutting,
                    QuantityProduced = -statementToDelete.Count // <-- الكمية السالبة
                };
                _context.ProductionLogs.Add(productionLog);

                _context.CuttingStatements.Remove(statementToDelete);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم حذف بيان القص بنجاح.";
            }

            return RedirectToPage("/Departments/Cutting", new { id = statementToDelete?.WorkOrderId });
        }
    }
}