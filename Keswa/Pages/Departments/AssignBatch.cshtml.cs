using Keswa.Data;
using Keswa.Enums;
using Keswa.Helpers;
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
    public class AssignBatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AssignBatchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SewingBatch SewingBatch { get; set; }
        public SelectList WorkerSelectList { get; set; }
        public int RemainingQuantityInBatch { get; set; }

        public class InputModel
        {
            public int SewingBatchId { get; set; }

            [Display(Name = "العامل")]
            [Required(ErrorMessage = "يجب اختيار عامل.")]
            public int WorkerId { get; set; }

            [Display(Name = "الكمية المسلمة")]
            [Required(ErrorMessage = "حقل الكمية مطلوب.")]
            [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر.")]
            public int AssignedQuantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int sewingBatchId)
        {
            SewingBatch = await _context.SewingBatches
                .Include(b => b.CuttingStatement.WorkOrder.Product)
                .FirstOrDefaultAsync(b => b.Id == sewingBatchId);

            if (SewingBatch == null) return NotFound();

            var totalAssigned = await _context.WorkerAssignments
                .Where(wa => wa.SewingBatchId == sewingBatchId)
                .SumAsync(wa => wa.AssignedQuantity);

            RemainingQuantityInBatch = SewingBatch.Quantity - totalAssigned;

            var workers = await _context.Workers
                .OrderBy(w => w.Name)
                .ToListAsync();

            WorkerSelectList = new SelectList(workers, "Id", "Name");

            Input.SewingBatchId = sewingBatchId;
            Input.AssignedQuantity = RemainingQuantityInBatch;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.SewingBatchId);
                return Page();
            }

            var sewingBatch = await _context.SewingBatches.FindAsync(Input.SewingBatchId);
            if (sewingBatch == null) return NotFound();

            var totalAssigned = await _context.WorkerAssignments
                .Where(wa => wa.SewingBatchId == Input.SewingBatchId)
                .SumAsync(wa => wa.AssignedQuantity);

            if (Input.AssignedQuantity > (sewingBatch.Quantity - totalAssigned))
            {
                ModelState.AddModelError("Input.AssignedQuantity", "الكمية المسلمة أكبر من الكمية المتبقية.");
                await OnGetAsync(Input.SewingBatchId);
                return Page();
            }

            var existingAssignmentsCount = await _context.WorkerAssignments
                .CountAsync(wa => wa.SewingBatchId == Input.SewingBatchId);

            var newSubSequence = (existingAssignmentsCount + 1).ToString("D3");
            var newAssignmentNumber = $"{sewingBatch.SewingBatchNumber}-{newSubSequence}";

            var newAssignment = new WorkerAssignment
            {
                AssignmentNumber = newAssignmentNumber,
                SewingBatchId = Input.SewingBatchId,
                WorkerId = Input.WorkerId,
                AssignedQuantity = Input.AssignedQuantity,
                AssignedDate = DateTimeHelper.EgyptNow,
                Status = AssignmentStatus.InProgress
            };
            _context.WorkerAssignments.Add(newAssignment);

            sewingBatch.Status = BatchStatus.Transferred;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسليم التشغيلة للعامل بنجاح.";
            return RedirectToPage("/Departments/Sewing");
        }
    }
}