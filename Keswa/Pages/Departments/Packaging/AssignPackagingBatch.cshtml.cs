using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Pages.Departments.Packaging
{
    public class AssignPackagingBatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AssignPackagingBatchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public PackagingBatch BatchToAssign { get; set; }
        public SelectList Workers { get; set; }
        public int RemainingQuantityInBatch { get; set; }

        public class InputModel
        {
            public int PackagingBatchId { get; set; }

            [Display(Name = "موظف التعبئة")]
            [Required(ErrorMessage = "يجب اختيار موظف.")]
            public int WorkerId { get; set; }

            [Display(Name = "الكمية المسلمة")]
            [Required(ErrorMessage = "حقل الكمية مطلوب.")]
            [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر.")]
            public int AssignedQuantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // جلب بيانات التشغيلة مع الموديل
            BatchToAssign = await _context.PackagingBatches
                .Include(b => b.WorkOrder.Product)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (BatchToAssign == null) return NotFound();

            // حساب إجمالي الكميات التي تم توزيعها مسبقاً من هذه التشغيلة
            var totalAssigned = await _context.PackagingAssignments
                .Where(pa => pa.PackagingBatchId == id)
                .SumAsync(pa => pa.AssignedQuantity);

            RemainingQuantityInBatch = BatchToAssign.Quantity - totalAssigned;

            // جلب قائمة الموظفين
            var workers = await _context.Workers.OrderBy(w => w.Name).ToListAsync();
            Workers = new SelectList(workers, "Id", "Name");

            Input.PackagingBatchId = id;
            Input.AssignedQuantity = RemainingQuantityInBatch; // اقتراح الكمية المتبقية بالكامل

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.PackagingBatchId);
                return Page();
            }

            var batch = await _context.PackagingBatches.FindAsync(Input.PackagingBatchId);
            if (batch == null) return NotFound();

            var totalAssigned = await _context.PackagingAssignments
                .Where(pa => pa.PackagingBatchId == Input.PackagingBatchId)
                .SumAsync(pa => pa.AssignedQuantity);

            int currentRemaining = batch.Quantity - totalAssigned;

            if (Input.AssignedQuantity > currentRemaining)
            {
                ModelState.AddModelError("Input.AssignedQuantity", "الكمية لا يمكن أن تتخطى المتبقي في التشغيلة.");
                await OnGetAsync(Input.PackagingBatchId);
                return Page();
            }

            // إنشاء العهدة الجديدة للموظف
            var newAssignment = new PackagingAssignment
            {
                PackagingBatchId = Input.PackagingBatchId,
                WorkerId = Input.WorkerId,
                AssignedQuantity = Input.AssignedQuantity,
                Status = PackagingAssignmentStatus.InProgress,
                AssignmentDate = DateTime.Now
            };

            _context.PackagingAssignments.Add(newAssignment);

            // تحديث حالة التشغيلة الرئيسية إلى "قيد التنفيذ"
            if (batch.Status == PackagingBatchStatus.Pending)
            {
                batch.Status = PackagingBatchStatus.InProgress;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسليم الكمية لموظف التعبئة بنجاح.";
            return RedirectToPage(new { id = Input.PackagingBatchId });
        }
    }
}