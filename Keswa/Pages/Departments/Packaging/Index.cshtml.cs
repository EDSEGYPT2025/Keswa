using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Departments.Packaging
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PackagingBatchViewModel> PendingBatches { get; set; }
        public List<PackagingAssignmentViewModel> AssignmentsInProgress { get; set; }
        public List<ReadyForWarehouseViewModel> ReadyBatches { get; set; }

        public async Task OnGetAsync()
        {
            // 1. تشغيلات بانتظار التوزيع (Pending)
            PendingBatches = await _context.PackagingBatches
                .Where(b => b.Status == PackagingBatchStatus.Pending)
                .Select(b => new PackagingBatchViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.PackagingBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    Quantity = b.Quantity
                }).ToListAsync();

            // 2. مهام قيد التنفيذ لدى العمال (InProgress)
            AssignmentsInProgress = await _context.PackagingAssignments
                .Where(a => a.Status == PackagingAssignmentStatus.InProgress)
                .Select(a => new PackagingAssignmentViewModel
                {
                    AssignmentId = a.Id,
                    WorkerName = a.Worker.Name,
                    BatchNumber = a.PackagingBatch.PackagingBatchNumber,
                    RemainingQuantity = a.RemainingQuantity
                }).ToListAsync();

            // 3. تشغيلات مكتملة وجاهزة للمخزن (Completed)
            ReadyBatches = await _context.PackagingBatches
                .Where(b => b.Status == PackagingBatchStatus.Completed)
                .Select(b => new ReadyForWarehouseViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.PackagingBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    // نجمع الكميات المغلفة فعلياً من عهد العمال
                    ReadyQuantity = b.PackagingAssignments.Sum(pa => pa.CompletedQuantity)
                }).ToListAsync();
        }

        // دالة التحويل النهائي للمخزن (سنكمل منطقها البرمجي لاحقاً عند ربط المخازن)
        public async Task<IActionResult> OnPostTransferToWarehouseAsync(int batchId)
        {
            var batch = await _context.PackagingBatches.FindAsync(batchId);
            if (batch == null) return NotFound();

            batch.Status = PackagingBatchStatus.TransferredToStore;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم توريد الإنتاج للمخزن النهائي بنجاح.";
            return RedirectToPage();
        }
    }

    // ViewModels الخاصة بالصفحة لضمان سرعة التحميل
    public class PackagingBatchViewModel
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class PackagingAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string BatchNumber { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class ReadyForWarehouseViewModel
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public int ReadyQuantity { get; set; }
    }
}