using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Keswa.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- تمت إضافة هذه الخاصية ---
        public CuttingDeptSummaryViewModel CuttingSummary { get; set; }

        // Other properties remain the same...
        public string UserFullName { get; set; }
        public int OpenWorkOrdersCount { get; set; }
        public int RecentSalesOrdersCount { get; set; }
        public int PendingPurchaseOrdersCount { get; set; }
        public int LowStockItemsCount { get; set; }
        public List<WorkOrder> RecentWorkOrders { get; set; }
        public string ProductionChartLabels { get; set; }
        public string ProductionChartDataPlanned { get; set; }
        public string ProductionChartDataCompleted { get; set; }
        public string OrderStatusChartLabels { get; set; }
        public string OrderStatusChartData { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            UserFullName = user.FullName;

            // KPIs
            OpenWorkOrdersCount = await _context.WorkOrders.CountAsync(wo => wo.Status != WorkOrderStatus.Completed);
            RecentSalesOrdersCount = await _context.SalesOrders.CountAsync(so => so.OrderDate >= System.DateTime.Today.AddDays(-30));
            PendingPurchaseOrdersCount = await _context.PurchaseOrders.CountAsync(po => po.Status != PurchaseOrderStatus.Received);
            LowStockItemsCount = await _context.InventoryItems.CountAsync(i => i.ItemType == InventoryItemType.RawMaterial && i.StockLevel < 10);

            // --- تمت إضافة هذا الجزء بالكامل ---
            // 1. جلب الكميات التي تم قصها لكل أمر شغل
            var quantitiesCut = await _context.ProductionLogs
                .Where(p => p.Department == Department.Cutting)
                .GroupBy(p => p.WorkOrderId)
                .Select(g => new { WorkOrderId = g.Key, TotalCut = g.Sum(p => p.QuantityProduced) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.TotalCut);

            // 2. جلب جميع أوامر الشغل المفتوحة
            var allOpenWorkOrders = await _context.WorkOrders
                .Where(wo => wo.Status != WorkOrderStatus.Completed)
                .ToListAsync();

            // 3. فلترة الأوامر في الذاكرة لتحديد الأوامر الموجودة في قسم القص
            var workOrdersInCutting = allOpenWorkOrders
                .Where(wo => wo.QuantityToProduce > (quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0))
                .ToList();

            // 4. حساب الإحصائيات وتعبئة النموذج
            CuttingSummary = new CuttingDeptSummaryViewModel
            {
                WorkOrderCount = workOrdersInCutting.Count,
                TotalRequired = workOrdersInCutting.Sum(wo => wo.QuantityToProduce),
                TotalCompleted = workOrdersInCutting.Sum(wo => quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0)
            };


            // Recent Activity
            RecentWorkOrders = await _context.WorkOrders
                .Include(wo => wo.Product)
                .OrderByDescending(wo => wo.CreationDate)
                .Take(5)
                .ToListAsync();

            // Chart Data remains the same...
            var recentWorkOrdersForChart = await _context.WorkOrders.Where(wo => wo.CreationDate >= System.DateTime.Today.AddMonths(-6)).ToListAsync();
            var productionData = recentWorkOrdersForChart.GroupBy(wo => new { wo.CreationDate.Year, wo.CreationDate.Month }).Select(g => new { Date = new System.DateTime(g.Key.Year, g.Key.Month, 1), Planned = g.Sum(wo => wo.QuantityToProduce), Completed = g.Where(wo => wo.Status == WorkOrderStatus.Completed).Sum(wo => wo.QuantityToProduce) }).OrderBy(r => r.Date).ToList();
            ProductionChartLabels = System.Text.Json.JsonSerializer.Serialize(productionData.Select(d => d.Date.ToString("MMM yyyy")));
            ProductionChartDataPlanned = System.Text.Json.JsonSerializer.Serialize(productionData.Select(d => d.Planned));
            ProductionChartDataCompleted = System.Text.Json.JsonSerializer.Serialize(productionData.Select(d => d.Completed));
            var orderStatusGroups = await _context.SalesOrders.GroupBy(so => so.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
            var allStatuses = System.Enum.GetValues(typeof(SalesOrderStatus)).Cast<SalesOrderStatus>();
            var statusDataDict = allStatuses.ToDictionary(status => status, status => 0);
            foreach (var group in orderStatusGroups) { statusDataDict[group.Status] = group.Count; }
            string GetDisplayName(System.Enum enumValue) { return enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttribute<DisplayAttribute>()?.GetName() ?? enumValue.ToString(); }
            OrderStatusChartLabels = System.Text.Json.JsonSerializer.Serialize(statusDataDict.Keys.Select(status => GetDisplayName(status)));
            OrderStatusChartData = System.Text.Json.JsonSerializer.Serialize(statusDataDict.Values);
        }
    }

    // --- تمت إضافة هذا النموذج المساعد ---
    public class CuttingDeptSummaryViewModel
    {
        public int WorkOrderCount { get; set; }
        public int TotalRequired { get; set; }
        public int TotalCompleted { get; set; }
        public int Remaining => TotalRequired - TotalCompleted;
        public int ProgressPercentage => (TotalRequired > 0) ? (int)System.Math.Round((double)TotalCompleted * 100 / TotalRequired) : 0;
    }
}
