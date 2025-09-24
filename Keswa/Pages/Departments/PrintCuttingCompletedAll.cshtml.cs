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
    public class PrintCuttingCompletedAllModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintCuttingCompletedAllModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<FinishedOrderViewModel> FinishedOrders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var finishedCuttingWoIds = await _context.WorkOrderRoutings
                .Where(r => r.Department == Department.Cutting && r.Status == WorkOrderStageStatus.Completed)
                .Select(r => r.WorkOrderId)
                .ToListAsync();

            var quantitiesCut = await _context.ProductionLogs
                .Where(p => p.Department == Department.Cutting && finishedCuttingWoIds.Contains(p.WorkOrderId))
                .GroupBy(p => p.WorkOrderId)
                .Select(g => new { WorkOrderId = g.Key, TotalCut = g.Sum(p => p.QuantityProduced) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.TotalCut);

            FinishedOrders = await _context.WorkOrders
                .Include(wo => wo.Product)
                .Where(wo => finishedCuttingWoIds.Contains(wo.Id))
                .Select(wo => new FinishedOrderViewModel
                {
                    WorkOrderId = wo.Id,
                    WorkOrderNumber = wo.WorkOrderNumber,
                    ProductName = wo.Product.Name,
                    QuantityToProduce = wo.QuantityToProduce,
                    TotalCut = quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0
                })
                .OrderByDescending(o => o.WorkOrderId)
                .ToListAsync();
        }
    }

    // Using the same ViewModel from the main page
    public class FinishedOrderViewModel
    {
        public int WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        public string? ProductName { get; set; }
        public int QuantityToProduce { get; set; }
        public int TotalCut { get; set; }
    }
}
