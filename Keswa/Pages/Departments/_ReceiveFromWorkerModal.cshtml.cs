using Keswa.Data;
using Keswa.Enums;
using Keswa.Helpers;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class ReceiveFromWorkerModalModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceiveFromWorkerModalModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public WorkerAssignment Assignment { get; set; }

        public class InputModel
        {
            public int AssignmentId { get; set; }

            [Display(Name = "الكمية السليمة المستلمة")]
            [Required(ErrorMessage = "هذا الحقل مطلوب")]
            [Range(0, int.MaxValue)]
            public int ReceivedQuantity { get; set; }

            [Display(Name = "الكمية الهالكة")]
            [Range(0, int.MaxValue)]
            public int ScrappedQuantity { get; set; }

            [Display(Name = "سبب الهالك (اختياري)")]
            public string? ScrapReason { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int assignmentId)
        {
            Assignment = await _context.WorkerAssignments
                .Include(wa => wa.Worker)
                .Include(wa => wa.SewingBatch.CuttingStatement.WorkOrder.Product)
                .FirstOrDefaultAsync(wa => wa.Id == assignmentId);

            if (Assignment == null)
            {
                return NotFound();
            }
            Input.AssignmentId = assignmentId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var assignment = await _context.WorkerAssignments
                .Include(wa => wa.SewingBatch.CuttingStatement.WorkOrder.Product)
                .FirstOrDefaultAsync(wa => wa.Id == Input.AssignmentId);

            if (assignment == null) return NotFound();

            // التحقق من أن الإجمالي لا يتجاوز المتبقي
            if ((Input.ReceivedQuantity + Input.ScrappedQuantity) > assignment.RemainingQuantity)
            {
                ModelState.AddModelError("Input.ReceivedQuantity", "إجمالي الكمية (السليمة + الهالكة) أكبر من المتبقي في عهدة العامل.");
            }

            if (!ModelState.IsValid)
            {
                Assignment = assignment;
                return Page();
            }

            // 1. تسجيل الهالك إن وجد
            if (Input.ScrappedQuantity > 0)
            {
                var scrapLog = new ScrapLog
                {
                    WorkerAssignmentId = assignment.Id,
                    ScrappedQuantity = Input.ScrappedQuantity,
                    Reason = Input.ScrapReason,
                    LogDate = DateTimeHelper.EgyptNow
                };
                _context.ScrapLogs.Add(scrapLog);
            }

            // 2. تسجيل الإنتاج لحساب المستحقات
            if (Input.ReceivedQuantity > 0)
            {
                var product = assignment.SewingBatch.CuttingStatement.WorkOrder.Product;
                var sewingLog = new SewingProductionLog
                {
                    WorkerAssignmentId = assignment.Id,
                    WorkerId = assignment.WorkerId,
                    ProductId = product.Id,
                    QuantityProduced = Input.ReceivedQuantity,
                    Rate = product.SewingRate,
                    TotalPay = Input.ReceivedQuantity * product.SewingRate,
                    LogDate = DateTimeHelper.EgyptNow
                };
                _context.SewingProductionLogs.Add(sewingLog);
            }

            // 3. تحديث عهدة العامل
            assignment.ReceivedQuantity += Input.ReceivedQuantity;
            assignment.ScrappedQuantity += Input.ScrappedQuantity;
            if (assignment.RemainingQuantity == 0)
            {
                assignment.Status = AssignmentStatus.Completed;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم استلام الإنتاج من العامل بنجاح.";
            return RedirectToPage("/Departments/Sewing");
        }
    }
}