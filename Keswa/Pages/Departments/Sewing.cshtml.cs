using Keswa.Data;
using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        // قوائم لعرض البيانات في الواجهة
        public List<SewingBatchViewModel> PendingBatches { get; set; }
        public List<AssignmentViewModel> AssignmentsInProgress { get; set; }
        public List<ReadyForTransferViewModel> ReadyBatches { get; set; }

        public async Task OnGetAsync()
        {
            // 1. جلب التشغيلات الجاهزة للتوزيع (التي لم توزع بعد)
            // ملاحظة: تم تعديل الحالة لتطابق ما لديك BatchStatus.PendingTransfer
            PendingBatches = await _context.SewingBatches
                .Where(b => b.Status == BatchStatus.PendingTransfer)
                .Select(b => new SewingBatchViewModel
                {
                    SewingBatchId = b.Id,
                    SewingBatchNumber = b.SewingBatchNumber,
                    ProductName = b.CuttingStatement.WorkOrder.Product.Name,
                    Quantity = b.Quantity
                }).ToListAsync();

            // 2. جلب التشغيلات التي قيد التنفيذ لدى العمال حالياً
            AssignmentsInProgress = await _context.WorkerAssignments
                .Where(a => a.Status == AssignmentStatus.InProgress)
                .Select(a => new AssignmentViewModel
                {
                    AssignmentId = a.Id,
                    WorkerName = a.Worker.Name,
                    SewingBatchNumber = a.AssignmentNumber,
                    RemainingQuantity = a.RemainingQuantity
                }).ToListAsync();

            // 3. جلب الإنتاج الجاهز (التشغيلات المكتملة) للتحويل للمرحلة التالية
            ReadyBatches = await _context.SewingBatches
                .Where(b => b.Status == BatchStatus.Completed)
                .Select(b => new ReadyForTransferViewModel
                {
                    SewingBatchId = b.Id,
                    SewingBatchNumber = b.SewingBatchNumber,
                    // جمع كل الكميات السليمة المستلمة من العمال لهذه التشغيلة
                    ReadyQuantity = b.WorkerAssignments.Sum(wa => wa.ReceivedQuantity)
                }).ToListAsync();
        }
    }

    // ViewModels لتمرير البيانات بشكل منظم للواجهة
    public class SewingBatchViewModel
    {
        public int SewingBatchId { get; set; }
        public string SewingBatchNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class AssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string SewingBatchNumber { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class ReadyForTransferViewModel
    {
        public int SewingBatchId { get; set; }
        public string SewingBatchNumber { get; set; }
        public int ReadyQuantity { get; set; }
    }
}