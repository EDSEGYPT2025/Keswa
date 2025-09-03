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

namespace Keswa.Pages.Reports
{
    public class WorkOrderCostingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WorkOrderCostingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedWorkOrderId { get; set; }

        public SelectList WorkOrderList { get; set; }
        public CostingReportViewModel? Report { get; set; }

        public async Task OnGetAsync()
        {
            WorkOrderList = new SelectList(await _context.WorkOrders
                .OrderByDescending(wo => wo.CreationDate)
                .ToListAsync(), "Id", "WorkOrderNumber");

            if (SelectedWorkOrderId.HasValue)
            {
                await GenerateReport(SelectedWorkOrderId.Value);
            }
        }

        private async Task GenerateReport(int workOrderId)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);

            if (workOrder == null) return;

            Report = new CostingReportViewModel
            {
                WorkOrderNumber = workOrder.WorkOrderNumber,
                ProductName = workOrder.Product.Name,
                QuantityToProduce = workOrder.QuantityToProduce
            };

            // 1. حساب تكلفة المواد الخام
            var issuedMaterials = await _context.MaterialIssuanceNoteDetails
                .Where(d => d.MaterialIssuanceNote.WorkOrderId == workOrderId)
                .Include(d => d.Material)
                .ToListAsync();

            foreach (var item in issuedMaterials)
            {
                var avgCost = await GetAverageMaterialCost(item.MaterialId);
                var materialCost = new MaterialCostDetail
                {
                    MaterialName = item.Material.Name,
                    QuantityIssued = item.Quantity,
                    Unit = item.Material.Unit,
                    AverageUnitCost = avgCost,
                    TotalCost = (decimal)item.Quantity * avgCost
                };
                Report.MaterialCosts.Add(materialCost);
            }

            // 2. حساب تكلفة الأجور المباشرة
            var productionLogs = await _context.ProductionLogs
                .Where(p => p.WorkOrderId == workOrderId)
                .ToListAsync();

            var departmentCosts = await _context.DepartmentCosts.ToDictionaryAsync(dc => dc.Department, dc => dc.CostPerUnit);

            Report.LaborCosts = productionLogs
                .GroupBy(p => p.Department)
                .Select(g => new LaborCostDetail
                {
                    Department = g.Key,
                    TotalQuantityProduced = g.Sum(p => p.QuantityProduced),
                    CostPerUnit = departmentCosts.GetValueOrDefault(g.Key, 0),
                    TotalCost = g.Sum(p => p.QuantityProduced) * departmentCosts.GetValueOrDefault(g.Key, 0)
                })
                .ToList();

            // 3. حساب الإجماليات
            Report.CalculateTotals();
        }

        private async Task<decimal> GetAverageMaterialCost(int materialId)
        {
            var receipts = await _context.GoodsReceiptNoteDetails
                .Where(d => d.MaterialId == materialId && d.UnitPrice > 0 && d.Quantity > 0)
                .ToListAsync();

            if (!receipts.Any()) return 0;

            // *** تم التعديل هنا: تحويل الكمية إلى decimal قبل الضرب ***
            var totalCost = receipts.Sum(d => (decimal)d.Quantity * d.UnitPrice);
            var totalQuantity = receipts.Sum(d => d.Quantity);

            // التحقق من أن الكمية ليست صفراً لتجنب القسمة على صفر
            if (totalQuantity == 0) return 0;

            return totalCost / (decimal)totalQuantity;
        }
    }

    // ViewModels for the report
    public class CostingReportViewModel
    {
        public string WorkOrderNumber { get; set; }
        public string ProductName { get; set; }
        public int QuantityToProduce { get; set; }
        public List<MaterialCostDetail> MaterialCosts { get; set; } = new();
        public List<LaborCostDetail> LaborCosts { get; set; } = new();
        public decimal TotalMaterialCost { get; private set; }
        public decimal TotalLaborCost { get; private set; }
        public decimal TotalDirectCost { get; private set; }
        public decimal CostPerUnit { get; private set; }

        public void CalculateTotals()
        {
            TotalMaterialCost = MaterialCosts.Sum(m => m.TotalCost);
            TotalLaborCost = LaborCosts.Sum(l => l.TotalCost);
            TotalDirectCost = TotalMaterialCost + TotalLaborCost;
            if (QuantityToProduce > 0)
            {
                CostPerUnit = TotalDirectCost / QuantityToProduce;
            }
        }
    }

    public class MaterialCostDetail
    {
        public string MaterialName { get; set; }
        public double QuantityIssued { get; set; }
        public UnitOfMeasure Unit { get; set; }
        public decimal AverageUnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class LaborCostDetail
    {
        public Department Department { get; set; }
        public int TotalQuantityProduced { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal TotalCost { get; set; }
    }
}
