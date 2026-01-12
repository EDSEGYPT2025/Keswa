using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Pages.Departments.Quality
{
    public class AssignQualityBatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AssignQualityBatchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public QualityBatch BatchToAssign { get; set; }
        public SelectList Workers { get; set; }
        public int RemainingQuantityInBatch { get; set; }

        public class InputModel
        {
            public int QualityBatchId { get; set; }

            [Required(ErrorMessage = "يجب اختيار موظف.")]
            public int WorkerId { get; set; }

            [Required(ErrorMessage = "حقل الكمية مطلوب.")]
            [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر.")]
            public int AssignedQuantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            BatchToAssign = await _context.QualityBatches
                .Include(b => b.WorkOrder.Product)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (BatchToAssign == null) return NotFound();

            var totalAssigned = await _context.QualityAssignments
                .Where(qa => qa.QualityBatchId == id)
                .SumAsync(qa => qa.AssignedQuantity);

            RemainingQuantityInBatch = BatchToAssign.Quantity - totalAssigned;

            var workers = await _context.Workers.OrderBy(w => w.Name).ToListAsync();
            Workers = new SelectList(workers, "Id", "Name");

            Input.QualityBatchId = id;
            Input.AssignedQuantity = RemainingQuantityInBatch;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.QualityBatchId);
                return Page();
            }

            var batch = await _context.QualityBatches.FindAsync(Input.QualityBatchId);
            if (batch == null) return NotFound();

            var totalAssigned = await _context.QualityAssignments
                .Where(qa => qa.QualityBatchId == Input.QualityBatchId)
                .SumAsync(qa => qa.AssignedQuantity);

            if (Input.AssignedQuantity > (batch.Quantity - totalAssigned))
            {
                ModelState.AddModelError("Input.AssignedQuantity", "الكمية لا يمكن أن تتخطى المتبقي في التشغيلة.");
                await OnGetAsync(Input.QualityBatchId);
                return Page();
            }

            var newAssignment = new QualityAssignment
            {
                QualityBatchId = Input.QualityBatchId,
                WorkerId = Input.WorkerId,
                AssignedQuantity = Input.AssignedQuantity,
                Status = QualityAssignmentStatus.InProgress,
                AssignmentDate = DateTime.Now
            };

            _context.QualityAssignments.Add(newAssignment);

            // تحديث حالة التشغيلة إلى "قيد التنفيذ" عند بدء أول توزيع
            if (batch.Status == QualityBatchStatus.Pending)
            {
                batch.Status = QualityBatchStatus.InProgress;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسليم الكمية للموظف بنجاح.";
            return RedirectToPage(new { id = Input.QualityBatchId });
        }
    }
}