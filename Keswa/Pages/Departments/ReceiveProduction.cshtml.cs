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
    public class ReceiveProductionModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceiveProductionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkerAssignment Assignment { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int WorkerAssignmentId { get; set; }

            [Required]
            [Display(Name = "الكمية المستلمة (سليمة)")]
            [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 0 أو أكبر")]
            public int QuantityProduced { get; set; } = 0;

            [Required]
            [Display(Name = "كمية الهالك")]
            [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 0 أو أكبر")]
            public int QuantityScrapped { get; set; } = 0;

            [Display(Name = "سبب الهالك")]
            public string ScrapReason { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int assignmentId)
        {
            Assignment = await _context.WorkerAssignments
                .Include(a => a.Worker)
                .Include(a => a.SewingBatch)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (Assignment == null || Assignment.Status != AssignmentStatus.InProgress)
            {
                TempData["ErrorMessage"] = "هذه العهدة غير صالحة للاستلام.";
                return RedirectToPage("./Sewing");
            }

            Input = new InputModel { WorkerAssignmentId = assignmentId };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Assignment = await _context.WorkerAssignments
                .Include(a => a.Worker)
                .Include(a => a.SewingBatch)
                    .ThenInclude(sb => sb.CuttingStatement)
                    .ThenInclude(cs => cs.WorkOrder)
                    .ThenInclude(wo => wo.Product)
                .FirstOrDefaultAsync(a => a.Id == Input.WorkerAssignmentId);

            if (Assignment == null) return NotFound();

            if (Input.QuantityProduced == 0 && Input.QuantityScrapped == 0)
            {
                ModelState.AddModelError(string.Empty, "يجب إدخال كمية سليمة أو كمية هالك (لا يمكن أن يكون كلاهما صفر).");
            }

            int totalReceived = Input.QuantityProduced + Input.QuantityScrapped;
            // استخدام الخاصية المحسوبة للتحقق
            if (totalReceived > Assignment.RemainingQuantity)
            {
                ModelState.AddModelError(string.Empty, "إجمالي الكمية (السليمة + الهالك) لا يمكن أن يكون أكبر من المتبقي مع العامل.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. تسجيل الإنتاج السليم
            if (Input.QuantityProduced > 0)
            {
                decimal rate = Assignment.SewingBatch.CuttingStatement.WorkOrder.Product.SewingFee;

                var productionLog = new SewingProductionLog
                {
                    WorkerAssignmentId = Assignment.Id,
                    WorkerId = Assignment.WorkerId,
                    ProductId = Assignment.SewingBatch.CuttingStatement.WorkOrder.ProductId,
                    QuantityProduced = Input.QuantityProduced,
                    Rate = rate,
                    TotalPay = Input.QuantityProduced * rate
                };
                _context.SewingProductionLogs.Add(productionLog);

                // تحديث إجمالي السليم في العهدة
                Assignment.ReceivedQuantity += Input.QuantityProduced;
            }

            // 2. تسجيل الهالك (بالهيكل الجديد)
            if (Input.QuantityScrapped > 0)
            {
                var scrapLog = new ScrapLog
                {
                    WorkOrderId = Assignment.SewingBatch.CuttingStatement.WorkOrderId,
                    Quantity = Input.QuantityScrapped,
                    Department = Department.Sewing,
                    Reason = Input.ScrapReason,
                    WorkerId = Assignment.WorkerId
                };
                _context.ScrapLogs.Add(scrapLog);

                // تحديث إجمالي الهالك في العهدة (الآن الحقل موجود)
                Assignment.TotalScrapped += Input.QuantityScrapped;
            }

            // 3. تحديث العهدة (التحقق من الاكتمال)
            if (Assignment.RemainingQuantity == 0) // الخاصية المحسوبة ستعطي 0 الآن
            {
                Assignment.Status = AssignmentStatus.Completed;
            }

            await _context.SaveChangesAsync();

            // 4. تحديث التشغيلة الرئيسية
            await CheckIfSewingBatchIsComplete(Assignment.SewingBatchId);

            TempData["SuccessMessage"] = $"تم استلام (سليم: {Input.QuantityProduced}, هالك: {Input.QuantityScrapped}) من العامل {Assignment.Worker.Name} بنجاح.";
            return RedirectToPage("./Sewing");
        }

        private async Task CheckIfSewingBatchIsComplete(int sewingBatchId)
        {
            var batch = await _context.SewingBatches
                .Include(b => b.WorkerAssignments)
                .FirstOrDefaultAsync(b => b.Id == sewingBatchId);

            if (batch == null) return;

            bool allAssignmentsCompleted = batch.WorkerAssignments
                                                .All(a => a.Status == AssignmentStatus.Completed);

            if (allAssignmentsCompleted)
            {
                batch.Status = BatchStatus.Completed;
                await _context.SaveChangesAsync();
            }
        }
    }
}