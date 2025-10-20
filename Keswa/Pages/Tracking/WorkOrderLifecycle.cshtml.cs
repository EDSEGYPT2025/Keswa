using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Tracking
{
    public class WorkOrderLifecycleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WorkOrderLifecycleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkOrder WorkOrder { get; set; }
        public List<CuttingStatement> CuttingStatements { get; set; }
        public List<SewingBatchViewModel> SewingBatches { get; set; }

        public async Task<IActionResult> OnGetAsync(int workOrderId)
        {
            // 1. جلب أمر الشغل الرئيسي مع بياناته الأساسية
            WorkOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            // 2. جلب كل بيانات القص التابعة لأمر الشغل هذا
            CuttingStatements = await _context.CuttingStatements
                .Where(cs => cs.WorkOrderId == workOrderId)
                .Include(cs => cs.Worker) // تضمين اسم عامل القص
                .OrderBy(cs => cs.RunNumber)
                .ToListAsync();

            // 3. جلب كل تشغيلات الخياطة الناتجة عن بيانات القص السابقة
            var cuttingStatementIds = CuttingStatements.Select(cs => cs.Id).ToList();

            SewingBatches = await _context.SewingBatches
                .Where(sb => cuttingStatementIds.Contains(sb.CuttingStatementId))
                .Include(sb => sb.WorkerAssignments) // تضمين تشغيلات العمال
                    .ThenInclude(wa => wa.Worker) // تضمين اسم عامل الخياطة
                .Select(sb => new SewingBatchViewModel
                {
                    SewingBatch = sb,
                    TotalReceived = sb.WorkerAssignments.Sum(wa => wa.ReceivedQuantity),
                    TotalScrapped = sb.WorkerAssignments.Sum(wa => wa.ScrappedQuantity)
                })
                .ToListAsync();

            return Page();
        }
    }

    // نموذج مساعد لعرض بيانات إضافية في الواجهة
    public class SewingBatchViewModel
    {
        public SewingBatch SewingBatch { get; set; }
        public int TotalReceived { get; set; }
        public int TotalScrapped { get; set; }
    }
}