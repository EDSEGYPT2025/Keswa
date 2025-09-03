using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;

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

        public string UserFullName { get; set; }
        public int OpenWorkOrdersCount { get; set; }
        public int RecentSalesOrdersCount { get; set; }
        public int PendingPurchaseOrdersCount { get; set; }
        public int LowStockItemsCount { get; set; }

        public List<WorkOrder> RecentWorkOrders { get; set; }

        // Chart Data
        public string ProductionChartLabels { get; set; }
        public string ProductionChartDataPlanned { get; set; }
        public string ProductionChartDataCompleted { get; set; }
        public string OrderStatusChartLabels { get; set; }
        public string OrderStatusChartData { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            UserFullName = user.FullName;

            // KPIs
            OpenWorkOrdersCount = await _context.WorkOrders
                .CountAsync(wo => wo.Status != WorkOrderStatus.Completed);

            RecentSalesOrdersCount = await _context.SalesOrders
                .CountAsync(so => so.OrderDate >= DateTime.Today.AddDays(-30));

            PendingPurchaseOrdersCount = await _context.PurchaseOrders
                .CountAsync(po => po.Status != PurchaseOrderStatus.Received);

            var lowStockItems = await _context.InventoryItems
                .Where(i => i.StockLevel < 10)
                .ToListAsync();
            LowStockItemsCount = lowStockItems.Count;


            // Recent Activity
            RecentWorkOrders = await _context.WorkOrders
                .Include(wo => wo.Product)
                .OrderByDescending(wo => wo.CreationDate)
                .Take(5)
                .ToListAsync();

            // Production Chart Data (Last 6 Months)
            var recentWorkOrdersForChart = await _context.WorkOrders
                .Where(wo => wo.CreationDate >= DateTime.Today.AddMonths(-6))
                .ToListAsync();

            var productionData = recentWorkOrdersForChart
                .GroupBy(wo => new { wo.CreationDate.Year, wo.CreationDate.Month })
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Planned = g.Sum(wo => wo.QuantityToProduce),
                    Completed = g.Where(wo => wo.Status == WorkOrderStatus.Completed).Sum(wo => wo.QuantityToProduce)
                })
                .OrderBy(r => r.Date)
                .ToList();

            ProductionChartLabels = System.Text.Json.JsonSerializer.Serialize(productionData.Select(d => d.Date.ToString("MMM yyyy")));
            ProductionChartDataPlanned = System.Text.Json.JsonSerializer.Serialize(productionData.Select(d => d.Planned));
            ProductionChartDataCompleted = System.Text.Json.JsonSerializer.Serialize(productionData.Select(d => d.Completed));

            // Order Status Chart Data (Improved)
            var orderStatusGroups = await _context.SalesOrders
                .GroupBy(so => so.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var allStatuses = Enum.GetValues(typeof(SalesOrderStatus)).Cast<SalesOrderStatus>();
            var statusDataDict = allStatuses.ToDictionary(
                status => status,
                status => 0
            );

            foreach (var group in orderStatusGroups)
            {
                statusDataDict[group.Status] = group.Count;
            }

            string GetDisplayName(Enum enumValue)
            {
                return enumValue.GetType()
                                .GetMember(enumValue.ToString())
                                .First()
                                .GetCustomAttribute<DisplayAttribute>()?
                                .GetName() ?? enumValue.ToString();
            }

            // *** تم التعديل هنا: استخدام تعبير لامدا لحل المشكلة ***
            OrderStatusChartLabels = System.Text.Json.JsonSerializer.Serialize(statusDataDict.Keys.Select(status => GetDisplayName(status)));
            OrderStatusChartData = System.Text.Json.JsonSerializer.Serialize(statusDataDict.Values);


            return Page();
        }
    }
}

