using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class AssignFinishingBatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AssignFinishingBatchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public FinishingBatch BatchToAssign { get; set; }
        public SelectList Workers { get; set; }
        public int RemainingQuantityInBatch { get; set; }

        public class InputModel
        {
            public int FinishingBatchId { get; set; }

            [Display(Name = "العامل")]
            [Required(ErrorMessage = "يجب اختيار عامل.")]
            public int WorkerId { get; set; }

            [Display(Name = "الكمية المسلمة")]
            [Required(ErrorMessage = "حقل الكمية مطلوب.")]
            [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر.")]
            public int AssignedQuantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int finishingBatchId)
        {
            BatchToAssign = await _context.FinishingBatches
                .Include(b => b.WorkOrder.Product)
                .FirstOrDefaultAsync(b => b.Id == finishingBatchId);

            if (BatchToAssign == null) return NotFound();

            var totalAssigned = await _context.FinishingAssignments
                .Where(wa => wa.FinishingBatchId == finishingBatchId)
                .SumAsync(wa => wa.AssignedQuantity);

            RemainingQuantityInBatch = BatchToAssign.Quantity - totalAssigned;

            var workers = await _context.Workers
                .OrderBy(w => w.Name)
                .ToListAsync();

            Workers = new SelectList(workers, "Id", "Name");

            Input.FinishingBatchId = finishingBatchId;
            Input.AssignedQuantity = RemainingQuantityInBatch;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.FinishingBatchId);
                return Page();
            }

            var finishingBatch = await _context.FinishingBatches.FindAsync(Input.FinishingBatchId);
            if (finishingBatch == null) return NotFound();

            var isAlreadyAssigned = await _context.FinishingAssignments
                .AnyAsync(wa => wa.FinishingBatchId == Input.FinishingBatchId);

            if (isAlreadyAssigned)
            {
                ModelState.AddModelError("", "خطأ: هذه التشغيلة تم تسليمها بالفعل.");
                await OnGetAsync(Input.FinishingBatchId);
                return Page();
            }

            if (Input.AssignedQuantity != finishingBatch.Quantity)
            {
                ModelState.AddModelError("Input.AssignedQuantity", "خطأ! يجب تسليم كامل كمية التشغيلة.");
                await OnGetAsync(Input.FinishingBatchId);
                return Page();
            }

            var newAssignment = new FinishingAssignment
            {
                FinishingBatchId = Input.FinishingBatchId,
                WorkerId = Input.WorkerId,
                AssignedQuantity = Input.AssignedQuantity,
                Status = FinishingAssignmentStatus.InProgress,
                Rate = finishingBatch.WorkOrder.Product.FinishingFee
            };
            _context.FinishingAssignments.Add(newAssignment);

            finishingBatch.Status = FinishingBatchStatus.InProgress;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسليم كامل التشغيلة للعامل بنجاح.";
            return RedirectToPage("/Departments/Finishing");
        }
    }
}
