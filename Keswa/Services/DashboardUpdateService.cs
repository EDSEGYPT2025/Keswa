using Keswa.Data;
using Keswa.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

                // جلب البيانات بشكل دوري
                var dashboardData = new
                {
                    activeWorkOrders = dbContext.WorkOrders.Count(wo => wo.Status == Keswa.Enums.WorkOrderStatus.InProgress),

                    // ================== بداية التعديل ==================
                    // بدلاً من استخدام الخاصية المحسوبة، قمنا بإجراء الحساب مباشرة هنا
                    itemsInSewing = dbContext.WorkerAssignments
                                        .Where(a => a.Status == Keswa.Enums.AssignmentStatus.InProgress)
                                        .Sum(a => a.AssignedQuantity - (a.ReceivedQuantity + a.ScrappedQuantity)),
                    // ================== نهاية التعديل ===================

                    producedToday = dbContext.WorkerAssignments
                                        .Where(a => a.Status == Keswa.Enums.AssignmentStatus.Completed && a.AssignedDate.Date == DateTime.Today)
                                        .Sum(a => a.ReceivedQuantity),
                };

                // إرسال البيانات المحدثة لجميع العملاء
                await _hubContext.Clients.All.SendAsync("ReceiveDashboardUpdate", dashboardData, stoppingToken);
            }

            // التحديث كل 5 ثوانٍ
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}