using Keswa.Data;
using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class CuttingCompletedModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CuttingCompletedModel(ApplicationDbContext context) => _context = context;

        public List<FinishedCuttingViewModel> FinishedOrders { get; set; } = new();

        public class FinishedCuttingViewModel
        {
            public int WorkOrderId { get; set; }
            public string WorkOrderNumber { get; set; } = "";
            public string ProductName { get; set; } = "";
            public int QuantityToProduce { get; set; }
            public int TotalCut { get; set; }
        }

        public async Task OnGetAsync()
        {
            // احسب كمية المنتج المقطوعة لكل أمر في قسم القص
            var cutSums = await _context.ProductionLogs
                .Where(p => p.Department == Department.Cutting)
                .GroupBy(p => p.WorkOrderId)
                .Select(g => new { WorkOrderId = g.Key, Total = g.Sum(x => x.QuantityProduced) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.Total);

            // جلب أوامر الشغل التي مرحلة القص فيها مكتملة
            var finishedRoutingIds = await _context.WorkOrderRoutings
                .Where(r => r.Department == Department.Cutting && r.Status == WorkOrderStageStatus.Completed)
                .Select(r => r.WorkOrderId)
                .ToListAsync();

            FinishedOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Where(w => finishedRoutingIds.Contains(w.Id))
                .Select(w => new FinishedCuttingViewModel
                {
                    WorkOrderId = w.Id,
                    WorkOrderNumber = w.WorkOrderNumber,
                    ProductName = w.Product != null ? w.Product.Name : "",
                    QuantityToProduce = w.QuantityToProduce,
                    TotalCut = cutSums.ContainsKey(w.Id) ? cutSums[w.Id] : 0
                })
                .OrderByDescending(f => f.WorkOrderId)
                .ToListAsync();
        }
    }
}
