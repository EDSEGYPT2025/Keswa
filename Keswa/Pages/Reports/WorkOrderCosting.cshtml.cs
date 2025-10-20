using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // <-- تأكد من وجود هذا السطر
using System.Linq;
using System.Reflection; // <-- تأكد من وجود هذا السطر
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
        public CostingMethod SelectedCostingMethod { get; set; } = CostingMethod.Average;

        public WorkOrder WorkOrder { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public CostingSummaryViewModel CostSummary { get; set; }

        // --- تمت إضافة هذه الخاصية ---
        public string SelectedCostingMethodDisplayName { get; set; }

        public async Task<IActionResult> OnGetAsync(int workOrderId)
        {
            WorkOrder = await _context.WorkOrders.Include(wo => wo.Product).FirstOrDefaultAsync(wo => wo.Id == workOrderId);
            if (WorkOrder == null) return NotFound();

            // --- تمت إضافة هذا السطر لجلب الاسم العربي ---
            SelectedCostingMethodDisplayName = SelectedCostingMethod.GetDisplayName();

            var salesOrderDetail = await _context.SalesOrderDetails
                .Include(sod => sod.SalesOrder.Customer)
                .FirstOrDefaultAsync(sod => sod.WorkOrderId == workOrderId);

            if (salesOrderDetail != null)
            {
                SalesOrder = salesOrderDetail.SalesOrder;
            }

            decimal totalMaterialCost = 0;
            var issuedMaterials = await _context.MaterialIssuanceNoteDetails
                .Where(d => d.MaterialIssuanceNote.WorkOrderId == workOrderId)
                .Include(d => d.Material)
                .ToListAsync();

            foreach (var issuedMaterial in issuedMaterials)
            {
                var materialId = issuedMaterial.MaterialId;
                var issuedQuantity = issuedMaterial.Quantity;

                var purchasePrices = await _context.GoodsReceiptNoteDetails
                    .Where(d => d.MaterialId == materialId && d.UnitPrice > 0)
                    .Select(d => d.UnitPrice)
                    .ToListAsync();

                if (purchasePrices.Any())
                {
                    decimal unitCost = 0;
                    switch (SelectedCostingMethod)
                    {
                        case CostingMethod.Average:
                            unitCost = purchasePrices.Average();
                            break;
                        case CostingMethod.Min:
                            unitCost = purchasePrices.Min();
                            break;
                        case CostingMethod.Max:
                            unitCost = purchasePrices.Max();
                            break;
                        case CostingMethod.Last:
                            var lastReceipt = await _context.GoodsReceiptNoteDetails
                                .Where(d => d.MaterialId == materialId && d.UnitPrice > 0)
                                .OrderByDescending(d => d.GoodsReceiptNote.ReceiptDate)
                                .FirstOrDefaultAsync();
                            unitCost = lastReceipt?.UnitPrice ?? 0;
                            break;
                    }
                    totalMaterialCost += unitCost * (decimal)issuedQuantity;
                }
            }

            var laborCost = await _context.WorkerAssignments.Where(wa => wa.SewingBatch.CuttingStatement.WorkOrderId == workOrderId).SumAsync(wa => wa.Earnings);
            decimal revenue = 0;

            CostSummary = new CostingSummaryViewModel
            {
                MaterialCost = totalMaterialCost,
                LaborCost = laborCost,
                Revenue = revenue
            };

            return Page();
        }
    }

    public enum CostingMethod
    {
        [Display(Name = "متوسط التكلفة")]
        Average,
        [Display(Name = "أقل سعر شراء")]
        Min,
        [Display(Name = "أعلى سعر شراء")]
        Max,
        [Display(Name = "آخر سعر شراء")]
        Last
    }

    public class CostingSummaryViewModel
    {
        public decimal MaterialCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal TotalDirectCost => MaterialCost + LaborCost;
        public decimal Revenue { get; set; }
        public decimal GrossProfit => Revenue - TotalDirectCost;
        public decimal GrossProfitMargin => Revenue > 0 ? (GrossProfit / Revenue) * 100 : 0;
    }

    // --- تمت إضافة هذه الفئة المساعدة ---
    public static class EnumExtensions
    {
        public static string GetDisplayName(this System.Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.GetName() ?? enumValue.ToString();
        }
    }
}