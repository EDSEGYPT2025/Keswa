using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class SewingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SewingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<SewingBatchViewModel> PendingBatches { get; set; }

        // -- BEGIN MODIFICATION --
        public List<SewingAssignmentViewModel> AssignmentsInProgress { get; set; }
        // -- END MODIFICATION --

        public List<ReadyForTransferViewModel> ReadyBatches { get; set; }

        public async Task OnGetAsync()
        {
            PendingBatches = await _context.SewingBatches
                .Where(b => b.Status == BatchStatus.PendingTransfer)
                .Select(b => new SewingBatchViewModel
                {
                    SewingBatchId = b.Id,
                    SewingBatchNumber = b.SewingBatchNumber,
                    ProductName = b.CuttingStatement.WorkOrder.Product.Name,
                    Quantity = b.Quantity
                }).ToListAsync();

            // -- BEGIN MODIFICATION --
            AssignmentsInProgress = await _context.WorkerAssignments
                .Where(a => a.Status == AssignmentStatus.InProgress)
                .Select(a => new SewingAssignmentViewModel // <-- تم التغيير هنا
                {
                    AssignmentId = a.Id,
                    WorkerName = a.Worker.Name,
                    SewingBatchNumber = a.AssignmentNumber,
                    RemainingQuantity = a.RemainingQuantity
                }).ToListAsync();
            // -- END MODIFICATION --

            ReadyBatches = await _context.SewingBatches
                .Where(b => b.Status == BatchStatus.Completed)
                .Select(b => new ReadyForTransferViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.SewingBatchNumber,
                    ProductName = b.CuttingStatement.WorkOrder.Product.Name,
                    ReadyQuantity = b.WorkerAssignments.Sum(wa => wa.ReceivedQuantity),
                    WorkerNames = string.Join(", ", b.WorkerAssignments.Select(wa => wa.Worker.Name).Distinct()),
                    CompletionDate = _context.SewingProductionLogs
                                     .Where(spl => b.WorkerAssignments.Select(wa => wa.Id).Contains(spl.WorkerAssignmentId))
                                     .Max(spl => (DateTime?)spl.LogDate)
                }).ToListAsync();
        }

        public async Task<IActionResult> OnPostTransferAsync(int sewingBatchId)
        {
            var sewingBatch = await _context.SewingBatches
                                      .Include(b => b.CuttingStatement)
                                      .Include(b => b.WorkerAssignments)
                                      .FirstOrDefaultAsync(b => b.Id == sewingBatchId);

            if (sewingBatch == null || sewingBatch.Status != BatchStatus.Completed)
            {
                TempData["ErrorMessage"] = "لا يمكن تحويل هذه التشغيلة، حالتها غير مناسبة أو غير موجودة.";
                return RedirectToPage();
            }

            sewingBatch.Status = BatchStatus.Transferred;

            var finishingBatch = new FinishingBatch
            {
                FinishingBatchNumber = $"FIN-{sewingBatch.SewingBatchNumber}",
                SewingBatchId = sewingBatch.Id,
                WorkOrderId = sewingBatch.CuttingStatement.WorkOrderId,
                Quantity = sewingBatch.WorkerAssignments.Sum(wa => wa.ReceivedQuantity),
                Status = FinishingBatchStatus.Pending,
            };
            _context.FinishingBatches.Add(finishingBatch);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم تحويل التشغيلة {sewingBatch.SewingBatchNumber} إلى قسم التشطيب بنجاح.";

            return RedirectToPage();
        }
    }

    public class SewingBatchViewModel
    {
        public int SewingBatchId { get; set; }
        public string SewingBatchNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    // -- BEGIN MODIFICATION --
    public class SewingAssignmentViewModel // <-- تم التغيير هنا
    {
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string SewingBatchNumber { get; set; }
        public int RemainingQuantity { get; set; }
    }
    // -- END MODIFICATION --

}