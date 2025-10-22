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
            [Display(Name = "الكمية السليمة")]
            [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 0 أو أكبر")]
            public int QuantityProduced { get; set; } = 0;

            [Required]
            [Display(Name = "كمية الهالك")]
            [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 0 أو أكبر")]
            public int QuantityScrapped { get; set; } = 0;
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
                .Include(a => a.FinishingBatch).ThenInclude(fb => fb.WorkOrder)
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
            }

            // -- BEGIN CORRECTION --
            // الكود الآن سيتطابق مع الموديل الجديد
            if (Input.QuantityScrapped > 0)
            {
                var scrapLog = new ScrapLog
                {
                    WorkOrderId = Assignment.FinishingBatch.WorkOrderId,
                    Quantity = Input.QuantityScrapped, // (تم تغيير الاسم)
                    Department = Department.Finishing, // (أصبح الحقل موجوداً)
                    Reason = "هالك تشطيب",
                    WorkerId = Assignment.WorkerId // (أصبح الحقل موجوداً)
                };
                _context.ScrapLogs.Add(scrapLog);
            }
            // -- END CORRECTION --

            Assignment.RemainingQuantity -= totalReceived;

            if (Assignment.RemainingQuantity == 0)
            {
                Assignment.Status = FinishingAssignmentStatus.Completed;
            }

            await _context.SaveChangesAsync();

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

            bool allAssignmentsCompleted = batch.FinishingAssignments
                                                .All(a => a.Status == FinishingAssignmentStatus.Completed);

            if (allAssignmentsCompleted)
            {
                batch.Status = FinishingBatchStatus.Completed;
                await _context.SaveChangesAsync();
            }
        }
    }
}