using Keswa.Data;
using Keswa.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class PrintAllCuttingWorkOrdersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintAllCuttingWorkOrdersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<WorkOrderViewModel> WorkOrdersToPrint { get; set; } = new();

        // *** تم تحديث النموذج هنا ***
        public class WorkOrderViewModel
        {
            public int WorkOrderId { get; set; }
            public string WorkOrderNumber { get; set; }
            public string ProductName { get; set; }
            public int QuantityToProduce { get; set; }
            public int QuantityCut { get; set; }
            public int RemainingToCut => QuantityToProduce - QuantityCut;
            public double TotalMaterialIssued { get; set; }
            public double TotalMaterialConsumed { get; set; }
            public double TotalMaterialRemaining => TotalMaterialIssued - TotalMaterialConsumed;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // جلب أوامر الشغل التي ما زالت في مرحلة القص
            var workOrderIdsInCutting = await _context.WorkOrderRoutings
                .Where(r => r.Department == Department.Cutting && r.Status != WorkOrderStageStatus.Completed)
                .Select(r => r.WorkOrderId)
                .ToListAsync();

            if (!workOrderIdsInCutting.Any())
            {
                return Page(); // لا يوجد شيء للطباعة
            }

            // *** تم إضافة العمليات الحسابية لكميات الخامات هنا ***

            // 1. حساب الكميات المقصوصة من سجلات الإنتاج
            var quantitiesCut = await _context.ProductionLogs
                .Where(p => workOrderIdsInCutting.Contains(p.WorkOrderId) && p.Department == Department.Cutting)
                .GroupBy(p => p.WorkOrderId)
                .Select(g => new { WorkOrderId = g.Key, TotalCut = g.Sum(p => p.QuantityProduced) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.TotalCut);

            // 2. حساب إجمالي الخامات المنصرفة
            var totalIssued = await _context.MaterialIssuanceNoteDetails
                .Where(d => workOrderIdsInCutting.Contains(d.MaterialIssuanceNote.WorkOrderId))
                .GroupBy(d => d.MaterialIssuanceNote.WorkOrderId)
                .Select(g => new { WorkOrderId = g.Key, Total = g.Sum(d => d.Quantity) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.Total);

            // 3. حساب إجمالي الخامات المستهلكة
            var totalConsumed = await _context.CuttingStatements
                .Where(cs => workOrderIdsInCutting.Contains(cs.WorkOrderId))
                .GroupBy(cs => cs.WorkOrderId)
                .Select(g => new { WorkOrderId = g.Key, Total = g.Sum(cs => cs.Meterage) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.Total);

            // 4. جلب بيانات أوامر الشغل وتجميع كل شيء
            WorkOrdersToPrint = await _context.WorkOrders
                .Include(wo => wo.Product)
                .Where(wo => workOrderIdsInCutting.Contains(wo.Id))
                .Select(wo => new WorkOrderViewModel
                {
                    WorkOrderId = wo.Id,
                    WorkOrderNumber = wo.WorkOrderNumber,
                    ProductName = wo.Product.Name,
                    QuantityToProduce = wo.QuantityToProduce,
                    QuantityCut = quantitiesCut.GetValueOrDefault(wo.Id, 0),
                    TotalMaterialIssued = totalIssued.GetValueOrDefault(wo.Id, 0),
                    TotalMaterialConsumed = totalConsumed.GetValueOrDefault(wo.Id, 0)
                })
                .OrderBy(wo => wo.WorkOrderNumber)
                .ToListAsync();

            return Page();
        }
    }
}