using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.Departments.Quality
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<QualityBatchViewModel> PendingBatches { get; set; }
        public List<QualityAssignmentViewModel> AssignmentsInProgress { get; set; }
        public List<CompletedBatchViewModel> CompletedBatches { get; set; }

        public async Task OnGetAsync()
        {
            PendingBatches = await _context.QualityBatches
                .Where(b => b.Status == QualityBatchStatus.Pending)
                .Select(b => new QualityBatchViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.QualityBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    Quantity = b.Quantity
                }).ToListAsync();

            AssignmentsInProgress = await _context.QualityAssignments
                .Where(a => a.Status == QualityAssignmentStatus.InProgress)
                .Select(a => new QualityAssignmentViewModel
                {
                    AssignmentId = a.Id,
                    WorkerName = a.Worker.Name,
                    BatchNumber = a.QualityBatch.QualityBatchNumber,
                    RemainingQuantity = a.RemainingQuantity
                }).ToListAsync();

            CompletedBatches = await _context.QualityBatches
                .Where(b => b.Status == QualityBatchStatus.Completed)
                .Select(b => new CompletedBatchViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.QualityBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    FinalQuantity = b.QualityAssignments.Sum(qa => qa.ReceivedQuantityGradeA + qa.ReceivedQuantityGradeB)
                }).ToListAsync();
        }

        public async Task<IActionResult> OnPostTransferToPackagingAsync(int batchId)
        {
            // 1. جلب بيانات تشغيلة الجودة مع الكميات المقبولة (درجة أ + ب)
            var qualityBatch = await _context.QualityBatches
                .Include(b => b.QualityAssignments)
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (qualityBatch == null) return NotFound();

            // 2. تحديث حالة تشغيلة الجودة لضمان عدم تحويلها مرتين
            qualityBatch.Status = QualityBatchStatus.Transferred;

            // 3. إنشاء تشغيلة جديدة في قسم التعبئة والتغليف
            var packagingBatch = new PackagingBatch
            {
                PackagingBatchNumber = $"PACK-{qualityBatch.QualityBatchNumber}",
                QualityBatchId = qualityBatch.Id,
                WorkOrderId = qualityBatch.WorkOrderId,
                // الكمية المحولة هي مجموع الدرجة الأولى والثانية المستلمة فعلياً
                Quantity = qualityBatch.QualityAssignments.Sum(a => a.ReceivedQuantityGradeA + a.ReceivedQuantityGradeB),
                Status = PackagingBatchStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.PackagingBatches.Add(packagingBatch);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم تحويل التشغيلة {qualityBatch.QualityBatchNumber} إلى قسم التعبئة بنجاح.";
            return RedirectToPage();
        }
    }

    public class QualityBatchViewModel
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class CompletedBatchViewModel
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public int FinalQuantity { get; set; }
    }
}
