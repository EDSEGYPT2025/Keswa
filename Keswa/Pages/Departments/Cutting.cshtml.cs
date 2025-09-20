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
using System.Text.Json;
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

        public bool CanCreateStatement { get; set; } = false;
        public string PreRequisiteMessage { get; set; }
        public string MaterialStockJson { get; set; }

        // --- تمت إضافة هذه الخاصية ---
        public string MaterialColorJson { get; set; }


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
                //.Where(vm => vm.RemainingToCut > 0)
                .OrderBy(vm => vm.WorkOrderId)
                .ToList();

            if (id.HasValue)
            {
                SelectedWorkOrder = await _context.WorkOrders.FindAsync(id.Value);
                if (SelectedWorkOrder != null)
                {
                    var issuedMaterialIds = await _context.MaterialIssuanceNoteDetails.Where(d => d.MaterialIssuanceNote.WorkOrderId == id.Value).Select(d => d.MaterialId).Distinct().ToListAsync();
                    var workersInDepartment = await _context.Workers.Where(w => w.Department == Department.Cutting).ToListAsync();

                    if (!issuedMaterialIds.Any()) { PreRequisiteMessage = "لا يمكن إنشاء بيان قص. يجب أولاً إنشاء 'إذن صرف مواد خام' واحد على الأقل لهذا الأمر."; }
                    else if (!workersInDepartment.Any()) { PreRequisiteMessage = "لا يمكن إنشاء بيان قص. يجب أولاً تعريف عامل واحد على الأقل في 'قسم القص'."; }
                    else { CanCreateStatement = true; }

                    var issuedQuantities = await _context.MaterialIssuanceNoteDetails.Where(d => d.MaterialIssuanceNote.WorkOrderId == id.Value).GroupBy(d => d.MaterialId).Select(g => new { MaterialId = g.Key, TotalIssued = g.Sum(d => d.Quantity) }).ToDictionaryAsync(x => x.MaterialId, x => x.TotalIssued);
                    var usedMeterage = await _context.CuttingStatements.Where(cs => cs.WorkOrderId == id.Value).GroupBy(cs => cs.MaterialId).Select(g => new { MaterialId = g.Key, TotalUsed = g.Sum(cs => cs.Meterage) }).ToDictionaryAsync(x => x.MaterialId, x => x.TotalUsed);

                    var issuedMaterials = await _context.Materials.Where(m => issuedMaterialIds.Contains(m.Id)).Include(m => m.Color).ToListAsync();

                    var materialInfo = new Dictionary<int, object>();
                    foreach (var material in issuedMaterials)
                    {
                        var issued = issuedQuantities.GetValueOrDefault(material.Id, 0);
                        var used = usedMeterage.GetValueOrDefault(material.Id, 0);
                        materialInfo[material.Id] = new { issued, remaining = issued - used, color = material.Color?.Name ?? "بدون لون" };
                    }
                    MaterialStockJson = JsonSerializer.Serialize(materialInfo);


                    CuttingStatements = await _context.CuttingStatements.Include(cs => cs.Material).ThenInclude(m => m.Color).Include(cs => cs.Product).Include(cs => cs.Worker).Include(cs => cs.Customer).Where(cs => cs.WorkOrderId == id.Value).OrderByDescending(cs => cs.StatementDate).ToListAsync();
                    IssuedMaterialsList = new SelectList(issuedMaterials.Select(m => new { Id = m.Id, DisplayText = m.Name + (m.Color != null ? $" - {m.Color.Name}" : "") }).ToList(), "Id", "DisplayText");
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

            var workOrder = await _context.WorkOrders.FindAsync(workOrderId);
            if (workOrder == null) return NotFound();

            var routingStage = await _context.WorkOrderRoutings.FirstOrDefaultAsync(r => r.WorkOrderId == workOrderId && r.Department == Department.Cutting);
            if (routingStage != null && routingStage.Status == WorkOrderStageStatus.Pending)
            {
                routingStage.Status = WorkOrderStageStatus.InProgress;
            }

            var runCount = await _context.CuttingStatements.CountAsync(cs => cs.WorkOrderId == workOrderId) + 1;
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

