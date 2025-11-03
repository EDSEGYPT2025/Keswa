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
        public BatchViewModel BatchVM { get; set; }

        [BindProperty]
        public QualityAssignmentViewModel AssignmentVM { get; set; }

        public SelectList WorkerList { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var batch = await _context.QualityBatches
                .Include(b => b.WorkOrder.Product)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null) return NotFound();

            BatchVM = new BatchViewModel
            {
                BatchId = batch.Id,
                ProductName = batch.WorkOrder.Product.Name,
                Quantity = batch.Quantity
            };

            WorkerList = new SelectList(await _context.Workers.ToListAsync(), "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Repopulate lists if model state is invalid
                WorkerList = new SelectList(await _context.Workers.ToListAsync(), "Id", "Name");
                return Page();
            }

            var batch = await _context.QualityBatches.FindAsync(BatchVM.BatchId);
            if (batch == null) return NotFound();

            var assignment = new QualityAssignment
            {
                QualityBatchId = batch.Id,
                WorkerId = AssignmentVM.WorkerId,
                AssignedQuantity = AssignmentVM.AssignedQuantity,
                Status = QualityAssignmentStatus.InProgress,
                AssignmentDate = System.DateTime.Now
            };

            _context.QualityAssignments.Add(assignment);

            batch.Status = QualityBatchStatus.InProgress;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }

    public class BatchViewModel
    {
        public int BatchId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
