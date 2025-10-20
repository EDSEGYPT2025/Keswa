using Keswa.Data;
using Keswa.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class DashboardUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<DashboardHub> _hubContext;

    public DashboardUpdateService(IServiceProvider serviceProvider, IHubContext<DashboardHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // --- 1. حساب المؤشرات الرئيسية ---
                    var activeWorkOrders = await dbContext.WorkOrders
                        .CountAsync(wo => wo.Status != Keswa.Enums.WorkOrderStatus.Completed, stoppingToken);

                    var itemsInSewing = await dbContext.WorkerAssignments
                        .Where(a => a.Status == Keswa.Enums.AssignmentStatus.InProgress)
                        .SumAsync(a => a.AssignedQuantity - (a.ReceivedQuantity + a.ScrappedQuantity), stoppingToken);

                    var producedToday = await dbContext.WorkerAssignments
                        .Where(a => a.Status == Keswa.Enums.AssignmentStatus.Completed && a.AssignedDate.Date == DateTime.Today)
                        .SumAsync(a => a.ReceivedQuantity, stoppingToken);

                    // --- 2. حساب قيمة مخزون المواد الخام (بمتوسط التكلفة) ---
                    var inventoryValue = await dbContext.InventoryItems
                        .Where(i => i.ItemType == Keswa.Enums.InventoryItemType.RawMaterial && i.StockLevel > 0)
                        .Select(i => new {
                            StockLevel = i.StockLevel,
                            AvgCost = dbContext.GoodsReceiptNoteDetails
                                        .Where(grd => grd.MaterialId == i.MaterialId && grd.UnitPrice > 0)
                                        .Average(grd => (decimal?)grd.UnitPrice) ?? 0
                        })
                        .SumAsync(x => (decimal)x.StockLevel * x.AvgCost, stoppingToken);

                    // --- 3. حساب إجمالي مستحقات العمال ---
                    var totalEarnings = await dbContext.WorkerAssignments.SumAsync(a => a.Earnings, stoppingToken);
                    var totalPayments = await dbContext.WorkerPayments.SumAsync(p => p.Amount, stoppingToken);
                    var pendingPayments = totalEarnings - totalPayments;


                    // --- 4. حساب ملخص قسم القص ---
                    var quantitiesCut = await dbContext.CuttingStatements
                        .GroupBy(p => p.WorkOrderId)
                        .Select(g => new { WorkOrderId = g.Key, TotalCut = g.Sum(p => p.Count) })
                        .ToDictionaryAsync(x => x.WorkOrderId, x => x.TotalCut, stoppingToken);

                    var allOpenWorkOrders = await dbContext.WorkOrders
                        .Where(wo => wo.Status != Keswa.Enums.WorkOrderStatus.Completed)
                        .ToListAsync(stoppingToken);

                    var workOrdersInCutting = allOpenWorkOrders
                        .Where(wo => wo.QuantityToProduce > (quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0))
                        .ToList();

                    var cuttingSummary = new DeptSummaryViewModel
                    {
                        DepartmentName = "القص",
                        WorkOrderCount = workOrdersInCutting.Count,
                        TotalRequired = workOrdersInCutting.Sum(wo => wo.QuantityToProduce),
                        TotalCompleted = workOrdersInCutting.Sum(wo => quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0)
                    };

                    // --- 5. حساب ملخص قسم الخياطة ---
                    var sewingSummary = new DeptSummaryViewModel
                    {
                        DepartmentName = "الخياطة",
                        WorkOrderCount = await dbContext.SewingBatches.CountAsync(sb => sb.Status == Keswa.Enums.BatchStatus.Transferred || sb.Status == Keswa.Enums.BatchStatus.Pending, stoppingToken),
                        TotalRequired = await dbContext.SewingBatches.Where(sb => sb.Status != Keswa.Enums.BatchStatus.Completed && sb.Status != Keswa.Enums.BatchStatus.Archived).SumAsync(sb => sb.Quantity, stoppingToken),
                        TotalCompleted = await dbContext.WorkerAssignments.Where(wa => wa.SewingBatch.Status != Keswa.Enums.BatchStatus.Completed && wa.SewingBatch.Status != Keswa.Enums.BatchStatus.Archived).SumAsync(wa => wa.ReceivedQuantity, stoppingToken)
                    };


                    // --- تجميع البيانات النهائية ---
                    var dashboardData = new
                    {
                        activeWorkOrders,
                        itemsInSewing,
                        producedToday,
                        inventoryValue,
                        pendingPayments,
                        cuttingSummary,
                        sewingSummary
                    };

                    // إرسال البيانات المحدثة لجميع العملاء
                    await _hubContext.Clients.All.SendAsync("ReceiveDashboardUpdate", dashboardData, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                // يمكنك تسجيل الخطأ هنا لتجنب توقف الخدمة
                Console.WriteLine($"Error in DashboardUpdateService: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    // نموذج مساعد لتوحيد شكل ملخصات الأقسام
    public class DeptSummaryViewModel
    {
        public string DepartmentName { get; set; }
        public int WorkOrderCount { get; set; }
        public int TotalRequired { get; set; }
        public int TotalCompleted { get; set; }
        public int Remaining => TotalRequired - TotalCompleted;
        public int ProgressPercentage => (TotalRequired > 0) ? (int)System.Math.Round((double)TotalCompleted * 100 / TotalRequired) : 0;
    }
}

