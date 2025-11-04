using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class FinishingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FinishingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<FinishingBatchViewModel> PendingBatches { get; set; }
        public List<FinishingAssignmentViewModel> AssignmentsInProgress { get; set; }
        public List<ReadyForTransferViewModel> ReadyBatches { get; set; }

        public async Task OnGetAsync()
        {
            PendingBatches = await _context.FinishingBatches
                .Where(b => b.Status == FinishingBatchStatus.Pending)
                .Select(b => new FinishingBatchViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.FinishingBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    Quantity = b.Quantity
                }).ToListAsync();

            AssignmentsInProgress = await _context.FinishingAssignments
                .Where(a => a.Status == FinishingAssignmentStatus.InProgress)
                .Select(a => new FinishingAssignmentViewModel
                {
                    AssignmentId = a.Id,
                    WorkerName = a.Worker.Name,
                    BatchNumber = a.FinishingBatch.FinishingBatchNumber,
                    RemainingQuantity = a.RemainingQuantity
                }).ToListAsync();

            ReadyBatches = await _context.FinishingBatches
                .Where(b => b.Status == FinishingBatchStatus.Completed)
                .Select(b => new ReadyForTransferViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.FinishingBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    ReadyQuantity = b.FinishingAssignments.Sum(fa => fa.FinishingProductionLogs.Sum(fpl => fpl.QuantityProduced)),
                    WorkerNames = string.Join(", ", b.FinishingAssignments.Select(fa => fa.Worker.Name).Distinct())
                }).ToListAsync();
        }

        public async Task<IActionResult> OnPostTransferAsync(int batchId)
        {
            var finishingBatch = await _context.FinishingBatches
                                        .Include(b => b.WorkOrder)
                                        .Include(b => b.FinishingAssignments)
                                            .ThenInclude(fa => fa.FinishingProductionLogs)
                                        .FirstOrDefaultAsync(b => b.Id == batchId);

            if (finishingBatch == null) return NotFound();

            finishingBatch.Status = FinishingBatchStatus.Transferred;

            var qualityBatch = new QualityBatch
            {
                QualityBatchNumber = $"QUAL-{finishingBatch.FinishingBatchNumber}",
                FinishingBatchId = finishingBatch.Id,
                WorkOrderId = finishingBatch.WorkOrderId,
                Quantity = finishingBatch.FinishingAssignments.Sum(fa => fa.FinishingProductionLogs.Sum(fpl => fpl.QuantityProduced)),
                Status = QualityBatchStatus.Pending,
                CreatedAt = System.DateTime.Now
            };

            _context.QualityBatches.Add(qualityBatch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحويل التشغيلة إلى قسم الجودة بنجاح.";
            return RedirectToPage();
        }
    }

    public class FinishingBatchViewModel
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class FinishingAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string BatchNumber { get; set; }
        public int RemainingQuantity { get; set; }
    }
}
