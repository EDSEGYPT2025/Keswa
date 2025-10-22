using Keswa.Data;
using Keswa.Hubs;
using Keswa.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Keswa.Services
{
    public class DashboardUpdateService : BackgroundService
    {
        private readonly ILogger<DashboardUpdateService> _logger;
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly IServiceProvider _serviceProvider;

        public DashboardUpdateService(ILogger<DashboardUpdateService> logger, IHubContext<DashboardHub> hubContext, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dashboard Update Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateDashboardData(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating dashboard data.");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task UpdateDashboardData(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Updating dashboard data at: {time}", DateTimeOffset.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var today = DateTime.Today;
                var startOfDay = today;
                var endOfDay = today.AddDays(1).AddTicks(-1);

                // --- Sewing Data --- (الكود هنا صحيح)
                var sewingAssignments = await _context.WorkerAssignments
                    .Include(wa => wa.SewingProductionLogs)
                    .Where(wa => wa.AssignedDate >= startOfDay && wa.AssignedDate <= endOfDay)
                    .ToListAsync(stoppingToken);

                int totalAssignedSewing = sewingAssignments.Sum(wa => wa.AssignedQuantity);
                int totalReceivedSewing = sewingAssignments.Sum(wa => wa.ReceivedQuantity);
                int totalScrappedSewing = sewingAssignments.Sum(wa => wa.TotalScrapped);
                int totalRemainingSewing = sewingAssignments.Sum(wa => wa.RemainingQuantity);
                decimal totalSewingEarningsToday = sewingAssignments.Sum(a => a.Earnings);

                // --- Finishing Data --- (الكود هنا صحيح)
                var finishingAssignments = await _context.FinishingAssignments
                     .Where(fa => fa.AssignmentDate >= startOfDay && fa.AssignmentDate <= endOfDay)
                     .Include(fa => fa.FinishingProductionLogs)
                     .ToListAsync(stoppingToken);

                int totalAssignedFinishing = finishingAssignments.Sum(fa => fa.AssignedQuantity);
                int totalReceivedFinishing = finishingAssignments.SelectMany(fa => fa.FinishingProductionLogs).Sum(fpl => fpl.QuantityProduced);
                int totalRemainingFinishing = finishingAssignments.Sum(fa => fa.RemainingQuantity);
                decimal totalFinishingEarningsToday = finishingAssignments
                    .SelectMany(fa => fa.FinishingProductionLogs)
                    .Sum(fpl => fpl.TotalPay);

                // --- Work Order Data ---
                // -- BEGIN CORRECTION: Correct property name --
                var workOrdersToday = await _context.WorkOrders
                    .Where(wo => wo.CreationDate >= startOfDay && wo.CreationDate <= endOfDay)
                    .ToListAsync(stoppingToken);

                int newWorkOrdersCount = workOrdersToday.Count();
                int totalQuantityWorkOrders = workOrdersToday.Sum(wo => wo.QuantityToProduce); // الاسم الصحيح
                // -- END CORRECTION --

                // --- Send Data ---
                await _hubContext.Clients.All.SendAsync("ReceiveDashboardData", new
                {
                    TotalAssignedSewing = totalAssignedSewing,
                    TotalReceivedSewing = totalReceivedSewing,
                    TotalScrappedSewing = totalScrappedSewing,
                    TotalRemainingSewing = totalRemainingSewing,
                    TotalAssignedFinishing = totalAssignedFinishing,
                    TotalReceivedFinishing = totalReceivedFinishing,
                    TotalRemainingFinishing = totalRemainingFinishing,
                    NewWorkOrdersCount = newWorkOrdersCount,
                    TotalQuantityWorkOrders = totalQuantityWorkOrders,
                    TotalSewingEarningsToday = totalSewingEarningsToday,
                    TotalFinishingEarningsToday = totalFinishingEarningsToday
                }, stoppingToken);
            }
        }
    }
}