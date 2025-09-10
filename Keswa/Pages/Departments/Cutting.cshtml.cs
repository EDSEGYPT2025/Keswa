using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Departments
{
    public class CuttingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CuttingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CuttingWorkOrderViewModel> WorkOrdersInCutting { get; set; } = new();

        public WorkOrder? SelectedWorkOrder { get; set; }
        public List<CuttingStatement>? CuttingStatements { get; set; }

        public SelectList? IssuedMaterialsList { get; set; }
        public SelectList? ProductList { get; set; }
        public SelectList? WorkerList { get; set; }
        public SelectList? CustomerList { get; set; }

        [BindProperty]
        public CuttingStatement NewCuttingStatement { get; set; } = new();

        // --- تمت إضافة هذه الخصائص للتحقق الذكي ---
        public bool CanCreateStatement { get; set; } = false;
        public string PreRequisiteMessage { get; set; }

        public async Task OnGetAsync(int? id)
        {
            var quantitiesCut = await _context.ProductionLogs
                .Where(p => p.Department == Department.Cutting)
                .GroupBy(p => p.WorkOrderId)
                .Select(g => new { WorkOrderId = g.Key, TotalCut = g.Sum(p => p.QuantityProduced) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.TotalCut);

            var allOpenWorkOrders = await _context.WorkOrders
                .Include(wo => wo.Product)
                .Where(wo => wo.Status != WorkOrderStatus.Completed)
                .Select(wo => new CuttingWorkOrderViewModel
                {
                    WorkOrderId = wo.Id,
                    WorkOrderNumber = wo.WorkOrderNumber,
                    ProductName = wo.Product.Name,
                    QuantityToProduce = wo.QuantityToProduce,
                    QuantityCut = quantitiesCut.ContainsKey(wo.Id) ? quantitiesCut[wo.Id] : 0
                })
                .ToListAsync();

            WorkOrdersInCutting = allOpenWorkOrders
                .Where(vm => vm.RemainingToCut > 0)
                .OrderBy(vm => vm.WorkOrderId)
                .ToList();

            if (id.HasValue)
            {
                SelectedWorkOrder = await _context.WorkOrders.FindAsync(id.Value);
                if (SelectedWorkOrder != null)
                {
                    // --- تم التعديل هنا: إضافة منطق التحقق الذكي ---
                    var issuedMaterialIds = await _context.MaterialIssuanceNoteDetails
                        .Where(d => d.MaterialIssuanceNote.WorkOrderId == id.Value)
                        .Select(d => d.MaterialId)
                        .Distinct()
                        .ToListAsync();

                    var workersInDepartment = await _context.Workers.Where(w => w.Department == Department.Cutting).ToListAsync();

                    if (!issuedMaterialIds.Any())
                    {
                        PreRequisiteMessage = "لا يمكن إنشاء بيان قص حالياً. يجب أولاً إنشاء 'إذن صرف مواد خام' واحد على الأقل لهذا الأمر لتتمكن من تحديد الخامة المصروفة.";
                    }
                    else if (!workersInDepartment.Any())
                    {
                        PreRequisiteMessage = "لا يمكن إنشاء بيان قص حالياً. يجب أولاً تعريف عامل واحد على الأقل في 'قسم القص' من شاشة إدارة العمال.";
                    }
                    else
                    {
                        CanCreateStatement = true; // كل الشروط متوفرة
                    }


                    CuttingStatements = await _context.CuttingStatements
                        .Include(cs => cs.Material)
                        .Include(cs => cs.Product)
                        .Include(cs => cs.Worker)
                        .Include(cs => cs.Customer)
                        .Where(cs => cs.WorkOrderId == id.Value)
                        .OrderByDescending(cs => cs.StatementDate)
                        .ToListAsync();

                    IssuedMaterialsList = new SelectList(await _context.Materials.Where(m => issuedMaterialIds.Contains(m.Id)).ToListAsync(), "Id", "Name");
                    ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
                    WorkerList = new SelectList(workersInDepartment, "Id", "Name");
                    CustomerList = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int workOrderId)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(workOrderId);
                return Page();
            }

            var runCount = await _context.CuttingStatements.CountAsync(cs => cs.WorkOrderId == workOrderId) + 1;
            var workOrder = await _context.WorkOrders.FindAsync(workOrderId);
            NewCuttingStatement.RunNumber = $"{workOrder.WorkOrderNumber}-{runCount:D3}";
            NewCuttingStatement.WorkOrderId = workOrderId;

            var productionLog = new ProductionLog
            {
                WorkOrderId = workOrderId,
                WorkerId = NewCuttingStatement.WorkerId,
                Department = Department.Cutting,
                QuantityProduced = NewCuttingStatement.Count
            };

            _context.CuttingStatements.Add(NewCuttingStatement);
            _context.ProductionLogs.Add(productionLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم حفظ بيان القص بنجاح برقم تشغيل {NewCuttingStatement.RunNumber}.";
            return RedirectToPage(new { id = workOrderId });
        }
    }

    public class CuttingWorkOrderViewModel
    {
        public int WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        public string? ProductName { get; set; }
        public int QuantityToProduce { get; set; }
        public int QuantityCut { get; set; }
        public int RemainingToCut => QuantityToProduce - QuantityCut;
    }
}

