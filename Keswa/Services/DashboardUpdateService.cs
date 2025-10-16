using Keswa.Data;
using Keswa.Hubs;
using Keswa.Pages; // <-- إضافة مهمة لاستخدام ViewModel
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // <-- إضافة مهمة

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
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // --- تمت إضافة هذا الجزء بالكامل ---
                // 1. جلب الكميات التي تم قصها لكل أمر شغل
                var quantitiesCut = await dbContext.CuttingStatements
                    .GroupBy(p => p.WorkOrderId)
                    .Select(g => new { WorkOrderId = g.Key, TotalCut = g.Sum(p => p.Count) })
                    .ToDictionaryAsync(x => x.WorkOrderId, x => x.TotalCut, stoppingToken);

                // 2. جلب جميع أوامر الشغل المفتوحة
                var allOpenWorkOrders = await dbContext.WorkOrders
                    .Where(wo => wo.Status != Keswa.Enums.WorkOrderStatus.Completed)
                    .ToListAsync(stoppingToken);

                // 3. فلترة الأوامر في الذاكرة لتحديد الأوامر التي لا تزال في قسم القص
                var workOrdersInCutting = allOpenWorkOrders
                    .Where(wo => wo.QuantityToProduce > (quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0))
                    .ToList();

                // 4. حساب إحصائيات قسم القص
                var cuttingSummary = new CuttingDeptSummaryViewModel
                {
                    WorkOrderCount = workOrdersInCutting.Count,
                    TotalRequired = workOrdersInCutting.Sum(wo => wo.QuantityToProduce),
                    TotalCompleted = workOrdersInCutting.Sum(wo => quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0)
                };

                // جلب باقي البيانات
                var dashboardData = new
                {
                    activeWorkOrders = allOpenWorkOrders.Count,
                    itemsInSewing = await dbContext.WorkerAssignments.Where(a => a.Status == Keswa.Enums.AssignmentStatus.InProgress).SumAsync(a => a.AssignedQuantity - (a.ReceivedQuantity + a.ScrappedQuantity), stoppingToken),
                    producedToday = await dbContext.WorkerAssignments.Where(a => a.Status == Keswa.Enums.AssignmentStatus.Completed && a.AssignedDate.Date == DateTime.Today).SumAsync(a => a.ReceivedQuantity, stoppingToken),
                    cuttingSummary = cuttingSummary // <-- إضافة بيانات قسم القص هنا
                };

                // إرسال البيانات المحدثة لجميع العملاء
                await _hubContext.Clients.All.SendAsync("ReceiveDashboardUpdate", dashboardData, stoppingToken);
            }

            // التحديث كل 5 ثوانٍ
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}