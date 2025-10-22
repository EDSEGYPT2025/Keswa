using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection; // <-- ضروري لجلب الاسم العربي
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
        public int WorkOrderId { get; set; }

        [BindProperty(SupportsGet = true)]
        public CostingMethod SelectedCostingMethod { get; set; } = CostingMethod.Average;

        public WorkOrder WorkOrder { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public CostingSummaryViewModel CostSummary { get; set; }

        // خاصية مساعدة لعرض الاسم العربي في الواجهة
        public string SelectedCostingMethodDisplayName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (WorkOrderId == 0) return Page();

            WorkOrder = await _context.WorkOrders.Include(wo => wo.Product).FirstOrDefaultAsync(wo => wo.Id == WorkOrderId);
            if (WorkOrder == null) return NotFound();

            // جلب الاسم العربي لطريقة الحساب المحددة
            SelectedCostingMethodDisplayName = SelectedCostingMethod.GetType()
                .GetMember(SelectedCostingMethod.ToString()).First()
                .GetCustomAttribute<DisplayAttribute>()?.GetName() ?? SelectedCostingMethod.ToString();

            // محاولة جلب أمر البيع والعميل
            var firstCuttingStatement = await _context.CuttingStatements
                .Include(cs => cs.Customer)
                .Where(cs => cs.WorkOrderId == WorkOrderId)
                .FirstOrDefaultAsync();
            if (firstCuttingStatement != null)
            {
                // بما أن أمر البيع غير مرتبط مباشرة بأمر الشغل، سنكتفي بالعميل من بيان القص
                // SalesOrder = ...
                // Customer = firstCuttingStatement.Customer;
            }


            // 1. حساب تكلفة الخامات بالطريقة المختارة
            var cuttingStatements = await _context.CuttingStatements
                .Where(cs => cs.WorkOrderId == WorkOrderId).ToListAsync();

            decimal totalMaterialCost = 0;
            foreach (var statement in cuttingStatements)
            {
                var materialPricesQuery = _context.GoodsReceiptNoteDetails
                    .Where(d => d.MaterialId == statement.MaterialId && d.UnitPrice > 0);

                decimal unitCost = 0;
                if (await materialPricesQuery.AnyAsync())
                {
                    switch (SelectedCostingMethod)
                    {
                        case CostingMethod.Highest: // أعلى سعر
                            unitCost = await materialPricesQuery.MaxAsync(d => d.UnitPrice);
                            break;
                        case CostingMethod.Last: // آخر سعر
                            var lastReceipt = await materialPricesQuery
                                .Include(d => d.GoodsReceiptNote)
                                .OrderByDescending(d => d.GoodsReceiptNote.ReceiptDate)
                                .FirstOrDefaultAsync();
                            unitCost = lastReceipt?.UnitPrice ?? 0;
                            break;
                        case CostingMethod.Average: // متوسط السعر
                        default:
                            unitCost = await materialPricesQuery.AverageAsync(d => d.UnitPrice);
                            break;
                    }
                }
                totalMaterialCost += (decimal)statement.IssuedQuantity * unitCost;
            }

            // 2. حساب تكلفة الخياطة والتشطيب (إجمالي الفيات)
            var sewingCost = await _context.SewingProductionLogs
                .Where(spl => spl.WorkerAssignment.SewingBatch.CuttingStatement.WorkOrderId == WorkOrderId)
                .SumAsync(spl => spl.TotalPay);

            var finishingCost = await _context.FinishingProductionLogs
                .Where(fpl => fpl.FinishingAssignment.FinishingBatch.WorkOrderId == WorkOrderId)
                .SumAsync(fpl => fpl.TotalPay);

            // 3. تجميع النتائج في ViewModel
            CostSummary = new CostingSummaryViewModel
            {
                MaterialCost = totalMaterialCost,
                LaborCost = sewingCost + finishingCost, // دمج تكلفة الخياطة والتشطيب
                Revenue = 0 // يمكنك حساب الإيرادات من أمر البيع إذا كان مرتبطاً
            };

            return Page();
        }
    }

    // يمكنك نقل هذه التعريفات إلى ملف منفصل لاحقاً
    public enum CostingMethod
    {
        [Display(Name = "متوسط التكلفة")]
        Average,
        [Display(Name = "أعلى سعر شراء")]
        Highest,
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
}