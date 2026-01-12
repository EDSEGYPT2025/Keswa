using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Pages.Departments.Packaging
{
    public class ReceivePackagingProductionModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceivePackagingProductionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AssignmentViewModel AssignmentVM { get; set; }

        [BindProperty]
        public ReceiveInputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync(int assignmentId)
        {
            var assignment = await _context.PackagingAssignments
                .Include(a => a.Worker)
                .Include(a => a.PackagingBatch.WorkOrder.Product)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || assignment.Status != PackagingAssignmentStatus.InProgress)
            {
                TempData["ErrorMessage"] = "هذه العهدة غير صالحة للاستلام.";
                return RedirectToPage("./Index");
            }

            AssignmentVM = new AssignmentViewModel
            {
                AssignmentId = assignment.Id,
                WorkerName = assignment.Worker.Name,
                ProductName = assignment.PackagingBatch.WorkOrder.Product.Name,
                AssignedQuantity = assignment.AssignedQuantity,
                RemainingQuantity = assignment.RemainingQuantity
            };

            Input = new ReceiveInputModel { AssignmentId = assignmentId };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var assignment = await _context.PackagingAssignments
                .Include(a => a.Worker)
                .Include(a => a.PackagingBatch.WorkOrder.Product)
                .FirstOrDefaultAsync(a => a.Id == Input.AssignmentId);

            if (assignment == null) return NotFound();

            if (Input.QuantityToReceive <= 0)
            {
                ModelState.AddModelError("Input.QuantityToReceive", "يجب إدخال كمية أكبر من صفر.");
            }

            if (Input.QuantityToReceive > assignment.RemainingQuantity)
            {
                ModelState.AddModelError("Input.QuantityToReceive", "الكمية المدخلة أكبر من المتبقي لدى الموظف.");
            }

            if (!ModelState.IsValid)
            {
                // إعادة تعبئة بيانات العرض لضمان عدم اختفائها عند الخطأ
                AssignmentVM = new AssignmentViewModel
                {
                    AssignmentId = assignment.Id,
                    WorkerName = assignment.Worker.Name,
                    ProductName = assignment.PackagingBatch.WorkOrder.Product.Name,
                    AssignedQuantity = assignment.AssignedQuantity,
                    RemainingQuantity = assignment.RemainingQuantity
                };
                return Page();
            }

            // تحديث الكمية المستلمة في العهدة
            assignment.CompletedQuantity += Input.QuantityToReceive;

            // إذا تم استلام كامل الكمية، نغلق العهدة
            if (assignment.RemainingQuantity == 0)
            {
                assignment.Status = PackagingAssignmentStatus.Completed;
            }

            await _context.SaveChangesAsync();

            // التحقق مما إذا كانت التشغيلة الكبرى قد اكتملت بالكامل
            await CheckIfBatchIsComplete(assignment.PackagingBatchId);

            TempData["SuccessMessage"] = $"تم استلام {Input.QuantityToReceive} قطعة من {assignment.Worker.Name} بنجاح.";
            return RedirectToPage("./Index");
        }

        private async Task CheckIfBatchIsComplete(int batchId)
        {
            var batch = await _context.PackagingBatches
                .Include(b => b.PackagingAssignments)
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (batch == null) return;

            // إذا كان إجمالي ما تم تغليفه يساوي كمية التشغيلة الأصلية
            int totalCompleted = batch.PackagingAssignments.Sum(a => a.CompletedQuantity);

            if (totalCompleted >= batch.Quantity)
            {
                batch.Status = PackagingBatchStatus.Completed;
                await _context.SaveChangesAsync();
            }
        }
    }

    // ViewModels لضمان عدم حدوث خطأ ModelState وتسهيل العرض
    public class AssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string? WorkerName { get; set; } // Nullable لمنع خطأ التحقق
        public string? ProductName { get; set; }
        public int AssignedQuantity { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class ReceiveInputModel
    {
        public int AssignmentId { get; set; }

        [Display(Name = "الكمية المستلمة (مغلفة)")]
        [Required(ErrorMessage = "يرجى إدخال الكمية.")]
        public int QuantityToReceive { get; set; }
    }
}