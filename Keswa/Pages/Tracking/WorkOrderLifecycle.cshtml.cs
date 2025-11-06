using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Tracking
{
    public class WorkOrderLifecycleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WorkOrderLifecycleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkOrder WorkOrder { get; set; }
        public List<CuttingStatementViewModel> CuttingStatements { get; set; }
        public List<SewingAssignmentViewModel> SewingAssignments { get; set; }
        public List<FinishingAssignmentViewModel> FinishingAssignments { get; set; }

        public async Task<IActionResult> OnGetAsync(int workOrderId)
        {
            WorkOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                // -- CORRECTION: Removed Include for SalesOrderDetail as it doesn't exist directly on WorkOrder --
                // .Include(wo => wo.SalesOrderDetail)
                //     .ThenInclude(sod => sod.SalesOrder)
                //     .ThenInclude(so => so.Customer)
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            // جلب بيانات القص
            CuttingStatements = await _context.CuttingStatements
                .Where(cs => cs.WorkOrderId == workOrderId)
                .Select(cs => new CuttingStatementViewModel
                {
                    // -- BEGIN CORRECTION: Correct property names from CuttingStatement model --
                    StatementNumber = cs.RunNumber,         // Correct name
                    Quantity = cs.Count,                // Correct name
                    Status = cs.Status.ToString(),
                    CreationDate = cs.StatementDate     // Correct name
                    // -- END CORRECTION --
                }).ToListAsync();

            // جلب بيانات الخياطة (الكود هنا صحيح)
            var sewingBatches = await _context.SewingBatches
                .Include(sb => sb.WorkerAssignments)
                .ThenInclude(wa => wa.Worker)
                .Where(sb => sb.CuttingStatement.WorkOrderId == workOrderId)
                .ToListAsync();

            SewingAssignments = sewingBatches.SelectMany(sb => sb.WorkerAssignments)
                .Select(wa => new SewingAssignmentViewModel
                {
                    WorkerName = wa.Worker.Name,
                    AssignedQuantity = wa.AssignedQuantity,
                    ReceivedQuantity = wa.ReceivedQuantity,
                    ScrappedQuantity = wa.TotalScrapped,
                    RemainingQuantity = wa.RemainingQuantity,
                    Status = wa.Status.ToString(),
                    AssignmentDate = wa.AssignedDate
                }).ToList();

            // جلب بيانات التشطيب (الكود هنا صحيح)
            var finishingBatches = await _context.FinishingBatches
                .Include(fb => fb.FinishingAssignments)
                .ThenInclude(fa => fa.Worker)
                .Include(fb => fb.FinishingAssignments)
                .ThenInclude(fa => fa.FinishingProductionLogs)
                .Where(fb => fb.WorkOrderId == workOrderId)
                .ToListAsync();

            FinishingAssignments = finishingBatches.SelectMany(fb => fb.FinishingAssignments)
                .Select(fa => new FinishingAssignmentViewModel
                {
                    WorkerName = fa.Worker.Name,
                    AssignedQuantity = fa.AssignedQuantity,
                    ReceivedQuantity = fa.ReceivedQuantity,
                    RemainingQuantity = fa.RemainingQuantity,
                    Status = fa.Status.ToString(),
                    AssignmentDate = fa.AssignmentDate
                }).ToList();

            return Page();
        }

        // ViewModels
        public class CuttingStatementViewModel
        {
            public string StatementNumber { get; set; }
            public int Quantity { get; set; }
            public string Status { get; set; }
            public DateTime CreationDate { get; set; }
        }

        public class SewingAssignmentViewModel
        {
            public string WorkerName { get; set; }
            public int AssignedQuantity { get; set; }
            public int ReceivedQuantity { get; set; }
            public int ScrappedQuantity { get; set; }
            public int RemainingQuantity { get; set; }
            public string Status { get; set; }
            public DateTime AssignmentDate { get; set; }
        }

        public class FinishingAssignmentViewModel
        {
            public string WorkerName { get; set; }
            public int AssignedQuantity { get; set; }
            public int ReceivedQuantity { get; set; }
            public int RemainingQuantity { get; set; }
            public string Status { get; set; }
            public DateTime AssignmentDate { get; set; }
        }
    }
}