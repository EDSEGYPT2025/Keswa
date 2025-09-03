using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkOrder WorkOrder { get; set; } = default!;

        [BindProperty]
        public ProductionLog NewProductionLog { get; set; } = new();
        public SelectList WorkerList { get; set; }
        public List<ProductionLog> ProductionLogs { get; set; }
        public List<WorkOrderRoutingViewModel> RoutingStages { get; set; }

        // *** تمت إضافة الخاصية التالية ***
        public List<MaterialIssuanceNote> MaterialIssuanceNotes { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await LoadWorkOrderData(id.Value);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddProductionLogAsync(int workOrderId)
        {
            WorkOrder = await _context.WorkOrders.FindAsync(workOrderId);
            if (WorkOrder == null)
            {
                return NotFound();
            }

            var totalInDepartment = await _context.ProductionLogs
                .Where(p => p.WorkOrderId == workOrderId && p.Department == NewProductionLog.Department)
                .SumAsync(p => p.QuantityProduced);

            var previousDepartmentLogs = await _context.ProductionLogs
                 .Where(p => p.WorkOrderId == workOrderId && p.Department < NewProductionLog.Department)
                 .SumAsync(p => p.QuantityProduced);

            var availableToProduce = (NewProductionLog.Department == Department.Cutting ? WorkOrder.QuantityToProduce : previousDepartmentLogs) - totalInDepartment;

            if (NewProductionLog.QuantityProduced > availableToProduce)
            {
                ModelState.AddModelError("NewProductionLog.QuantityProduced", $"الكمية القصوى المتاحة في هذا القسم هي {availableToProduce}");
            }

            if (!ModelState.IsValid)
            {
                await LoadWorkOrderData(workOrderId);
                return Page();
            }

            NewProductionLog.WorkOrderId = workOrderId;
            _context.ProductionLogs.Add(NewProductionLog);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = workOrderId });
        }

        private async Task LoadWorkOrderData(int id)
        {
            WorkOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (WorkOrder == null) return;

            await EnsureRoutingExists(id);

            ProductionLogs = await _context.ProductionLogs
                .Include(p => p.Worker)
                .Where(p => p.WorkOrderId == id)
                .OrderByDescending(p => p.LogDate)
                .ToListAsync();

            var routings = await _context.WorkOrderRoutings
                .Where(r => r.WorkOrderId == id)
                .OrderBy(r => r.Department)
                .ToListAsync();

            RoutingStages = new List<WorkOrderRoutingViewModel>();
            int previousStageOut = WorkOrder.QuantityToProduce;

            foreach (var stage in routings)
            {
                var stageViewModel = new WorkOrderRoutingViewModel
                {
                    Department = stage.Department,
                    QuantityIn = previousStageOut,
                    QuantityOut = ProductionLogs
                        .Where(p => p.Department == stage.Department)
                        .Sum(p => p.QuantityProduced)
                };
                RoutingStages.Add(stageViewModel);
                previousStageOut = stageViewModel.QuantityOut;
            }

            // *** تمت إضافة هذا الجزء ***
            // جلب أذونات الصرف المرتبطة بأمر الشغل
            MaterialIssuanceNotes = await _context.MaterialIssuanceNotes
                .Include(i => i.Warehouse)
                .Where(i => i.WorkOrderId == id)
                .OrderByDescending(i => i.IssuanceDate)
                .ToListAsync();

            WorkerList = new SelectList(await _context.Workers.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");
        }

        private async Task EnsureRoutingExists(int workOrderId)
        {
            bool hasRouting = await _context.WorkOrderRoutings.AnyAsync(r => r.WorkOrderId == workOrderId);
            if (!hasRouting)
            {
                var departments = Enum.GetValues(typeof(Department)).Cast<Department>();
                foreach (var dept in departments)
                {
                    _context.WorkOrderRoutings.Add(new WorkOrderRouting
                    {
                        WorkOrderId = workOrderId,
                        Department = dept
                    });
                }
                await _context.SaveChangesAsync();
            }
        }
    }

    public class WorkOrderRoutingViewModel
    {
        public Department Department { get; set; }
        public int QuantityIn { get; set; }
        public int QuantityOut { get; set; }
        public int WorkInProgress => QuantityIn - QuantityOut;
        public int ProgressPercentage => (QuantityIn > 0) ? (int)Math.Round((double)QuantityOut * 100 / QuantityIn) : 0;
    }
}
