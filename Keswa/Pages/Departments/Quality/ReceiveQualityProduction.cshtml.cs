using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments.Quality
{
    public class ReceiveQualityProductionModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceiveQualityProductionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AssignmentViewModel AssignmentVM { get; set; }

        [BindProperty]
        public ReceiveViewModel ReceiveVM { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var assignment = await _context.QualityAssignments
                .Include(a => a.Worker)
                .Include(a => a.QualityBatch.WorkOrder.Product)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null) return NotFound();

            AssignmentVM = new AssignmentViewModel
            {
                AssignmentId = assignment.Id,
                WorkerName = assignment.Worker.Name,
                ProductName = assignment.QualityBatch.WorkOrder.Product.Name,
                AssignedQuantity = assignment.AssignedQuantity
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var assignment = await _context.QualityAssignments.FindAsync(AssignmentVM.AssignmentId);
            if (assignment == null) return NotFound();

            if (ReceiveVM.ReceivedQuantityGradeA + ReceiveVM.ReceivedQuantityGradeB > assignment.AssignedQuantity)
            {
                ModelState.AddModelError("", "مجموع الكميات المستلمة لا يمكن أن يكون أكبر من الكمية المسلمة.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            assignment.ReceivedQuantityGradeA = ReceiveVM.ReceivedQuantityGradeA;
            assignment.ReceivedQuantityGradeB = ReceiveVM.ReceivedQuantityGradeB;
            assignment.Status = QualityAssignmentStatus.Completed;

            // Check if all assignments for the batch are completed
            var batch = await _context.QualityBatches
                .Include(b => b.QualityAssignments)
                .FirstOrDefaultAsync(b => b.Id == assignment.QualityBatchId);

            if (batch.QualityAssignments.All(a => a.Status == QualityAssignmentStatus.Completed))
            {
                batch.Status = QualityBatchStatus.Completed;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }

    public class AssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string ProductName { get; set; }
        public int AssignedQuantity { get; set; }
    }

    public class ReceiveViewModel
    {
        [Range(0, int.MaxValue)]
        [Display(Name = "كمية الدرجة الأولى")]
        public int ReceivedQuantityGradeA { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "كمية الدرجة الثانية")]
        public int ReceivedQuantityGradeB { get; set; }
    }
}
