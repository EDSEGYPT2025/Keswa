using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
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
        public List<ReadyForWarehouseViewModel> ReadyBatches { get; set; }


        public async Task OnGetAsync()
        {
            // 1. جلب التشغيلات المستلمة
            PendingBatches = await _context.FinishingBatches
                .Where(b => b.Status == FinishingBatchStatus.Pending)
                .Include(b => b.WorkOrder)
                .ThenInclude(wo => wo.Product)
                .Select(b => new FinishingBatchViewModel
                {
                    FinishingBatchId = b.Id,
                    FinishingBatchNumber = b.FinishingBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    Quantity = b.Quantity
                }).ToListAsync();

            // 2. جلب التشغيلات قيد التنفيذ
            AssignmentsInProgress = await _context.FinishingAssignments
                .Where(a => a.Status == FinishingAssignmentStatus.InProgress)
                .Include(a => a.FinishingBatch)
                .Select(a => new FinishingAssignmentViewModel
                {
                    AssignmentId = a.Id,
                    WorkerName = a.Worker.Name,
                    FinishingBatchNumber = a.FinishingBatch.FinishingBatchNumber,
                    RemainingQuantity = a.RemainingQuantity,
                    AssignmentType = a.AssignmentType
                }).ToListAsync();

            // 3. جلب التشغيلات المكتملة
            ReadyBatches = await _context.FinishingBatches
                .Where(b => b.Status == FinishingBatchStatus.Completed)
                .Include(b => b.WorkOrder).ThenInclude(wo => wo.Product)
                .Include(b => b.FinishingAssignments).ThenInclude(fa => fa.FinishingProductionLogs) // <-- هذا السطر يعمل الآن
                .Select(b => new ReadyForWarehouseViewModel
                {
                    FinishingBatchId = b.Id,
                    FinishingBatchNumber = b.FinishingBatchNumber,
                    ProductName = b.WorkOrder.Product.Name,
                    // جمع كل الكميات المنتجة من كل العهد لهذه التشغيلة
                    ReadyQuantity = b.FinishingAssignments
                                     .SelectMany(fa => fa.FinishingProductionLogs) // <-- وهذا السطر يعمل الآن
                                     .Sum(fpl => fpl.QuantityProduced)
                }).ToListAsync();
        }
    }

    public class FinishingBatchViewModel
    {
        public int FinishingBatchId { get; set; }
        public string FinishingBatchNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class FinishingAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string FinishingBatchNumber { get; set; }
        public int RemainingQuantity { get; set; }
        public AssignmentType AssignmentType { get; set; }
    }

    public class ReadyForWarehouseViewModel
    {
        public int FinishingBatchId { get; set; }
        public string FinishingBatchNumber { get; set; }
        public string ProductName { get; set; }
        public int ReadyQuantity { get; set; }
    }
}