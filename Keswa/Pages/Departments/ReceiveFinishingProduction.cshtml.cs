using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class ReceiveFinishingProductionModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceiveFinishingProductionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public FinishingAssignment Assignment { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int FinishingAssignmentId { get; set; }

            [Required]
            [Display(Name = "الكمية المستلمة (سليمة)")]
            [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 0 أو أكبر")]
            public int QuantityProduced { get; set; } = 0;

            [Required]
            [Display(Name = "كمية الهالك")]
            [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 0 أو أكبر")]
            public int QuantityScrapped { get; set; } = 0;

            [Display(Name = "سبب الهالك")]
            // --- التعديل هنا: جعل الحقل يقبل قيم فارغة لمنع خطأ ModelState ---
            public string? ScrapReason { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int assignmentId)
        {
            Assignment = await _context.FinishingAssignments
                .Include(a => a.Worker)
                .Include(a => a.FinishingBatch)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (Assignment == null || Assignment.Status != FinishingAssignmentStatus.InProgress)
            {
                TempData["ErrorMessage"] = "هذه العهدة غير صالحة للاستلام.";
                return RedirectToPage("./Finishing");
            }

            Input = new InputModel { FinishingAssignmentId = assignmentId };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Assignment = await _context.FinishingAssignments
                .Include(a => a.Worker)
                .Include(a => a.FinishingBatch)
                    .ThenInclude(sb => sb.WorkOrder)
                    .ThenInclude(wo => wo.Product)
                .FirstOrDefaultAsync(a => a.Id == Input.FinishingAssignmentId);

            if (Assignment == null) return NotFound();

            if (Input.QuantityProduced == 0 && Input.QuantityScrapped == 0)
            {
                ModelState.AddModelError(string.Empty, "يجب إدخال كمية سليمة أو كمية هالك (لا يمكن أن يكون كلاهما صفر).");
            }

            int totalReceived = Input.QuantityProduced + Input.QuantityScrapped;
            if (totalReceived > Assignment.RemainingQuantity)
            {
                ModelState.AddModelError(string.Empty, "إجمالي الكمية (السليمة + الهالك) لا يمكن أن يكون أكبر من المتبقي مع العامل.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. تسجيل الإنتاج السليم في سجلات التشطيب
            if (Input.QuantityProduced > 0)
            {
                var productionLog = new FinishingProductionLog
                {
                    FinishingAssignmentId = Assignment.Id,
                    QuantityProduced = Input.QuantityProduced,
                    Rate = Assignment.Rate,
                    TotalPay = Input.QuantityProduced * Assignment.Rate
                };
                _context.FinishingProductionLogs.Add(productionLog);
                Assignment.ReceivedQuantity += Input.QuantityProduced;
            }

            // 2. تسجيل الهالك في قسم التشطيب
            if (Input.QuantityScrapped > 0)
            {
                var scrapLog = new ScrapLog
                {
                    WorkOrderId = Assignment.FinishingBatch.WorkOrderId,
                    Quantity = Input.QuantityScrapped,
                    Department = Department.Finishing,
                    Reason = Input.ScrapReason,
                    WorkerId = Assignment.WorkerId
                };
                _context.ScrapLogs.Add(scrapLog);
                Assignment.TotalScrapped += Input.QuantityScrapped;
            }

            // 3. تحديث حالة العهدة عند الاكتمال
            if (Assignment.RemainingQuantity == 0)
            {
                Assignment.Status = FinishingAssignmentStatus.Completed;
            }

            await _context.SaveChangesAsync();

            // 4. تحديث حالة التشغيلة الرئيسية للتشطيب
            await CheckIfFinishingBatchIsComplete(Assignment.FinishingBatchId);

            TempData["SuccessMessage"] = $"تم استلام (سليم: {Input.QuantityProduced}, هالك: {Input.QuantityScrapped}) من العامل {Assignment.Worker.Name} بنجاح.";
            return RedirectToPage("./Finishing");
        }

        private async Task CheckIfFinishingBatchIsComplete(int finishingBatchId)
        {
            var batch = await _context.FinishingBatches
                .Include(b => b.FinishingAssignments)
                .FirstOrDefaultAsync(b => b.Id == finishingBatchId);

            if (batch == null) return;

            int totalProcessed = batch.FinishingAssignments.Sum(a => a.ReceivedQuantity + a.TotalScrapped);

            if (totalProcessed >= batch.Quantity)
            {
                batch.Status = FinishingBatchStatus.Completed;
                foreach (var assignment in batch.FinishingAssignments)
                {
                    assignment.Status = FinishingAssignmentStatus.Completed;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}