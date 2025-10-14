using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class EditCuttingStatementModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditCuttingStatementModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CuttingStatement CuttingStatement { get; set; }
        public SelectList WorkerList { get; set; }

        // --- تمت إضافة هذه الخاصية ---
        public double AvailableQuantity { get; set; }

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
                .Include(cs => cs.Material)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (CuttingStatement == null)
            {
                return NotFound();
            }

            // --- تم إضافة هذا الجزء لحساب الكمية المتاحة ---
            // 1. إجمالي الكمية المنصرفة من هذه الخامة لأمر الشغل
            var totalIssued = await _context.MaterialIssuanceNoteDetails
                .Where(d => d.MaterialIssuanceNote.WorkOrderId == CuttingStatement.WorkOrderId && d.MaterialId == CuttingStatement.MaterialId)
                .SumAsync(d => (double?)d.Quantity) ?? 0;

            // 2. إجمالي الكمية المستهلكة من هذه الخامة (باستثناء البيان الحالي)
            var totalConsumedByOthers = await _context.CuttingStatements
                .Where(cs => cs.WorkOrderId == CuttingStatement.WorkOrderId && cs.MaterialId == CuttingStatement.MaterialId && cs.Id != id)
                .SumAsync(cs => (double?)cs.Meterage) ?? 0;

            // 3. الكمية المتاحة للتعديل = (المنصرف - المستهلك) + قيمة البيان الحالي
            AvailableQuantity = totalIssued - totalConsumedByOthers;


            var workersInDepartment = await _context.Workers
                .Where(w => w.Department == Department.Cutting)
                .ToListAsync();
            WorkerList = new SelectList(workersInDepartment, "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(CuttingStatement.Id); // إعادة تحميل البيانات اللازمة في حالة وجود خطأ
                return Page();
            }

            var statementToUpdate = await _context.CuttingStatements.AsNoTracking().FirstOrDefaultAsync(cs => cs.Id == CuttingStatement.Id);
            if (statementToUpdate == null)
            {
                return NotFound();
            }

            int oldCount = statementToUpdate.Count;
            int newCount = CuttingStatement.Count;
            int difference = newCount - oldCount;

            if (difference != 0)
            {
                var productionLog = new ProductionLog
                {
                    WorkOrderId = CuttingStatement.WorkOrderId,
                    WorkerId = CuttingStatement.WorkerId,
                    Department = Department.Cutting,
                    QuantityProduced = difference
                };
                _context.ProductionLogs.Add(productionLog);
            }

            _context.Attach(CuttingStatement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل بيان القص بنجاح.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.CuttingStatements.AnyAsync(e => e.Id == CuttingStatement.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/Departments/Cutting", new { id = CuttingStatement.WorkOrderId });
        }
    }
}