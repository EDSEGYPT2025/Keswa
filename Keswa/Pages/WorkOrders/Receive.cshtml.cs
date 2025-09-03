using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class ReceiveModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReceiveModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public WorkOrder WorkOrder { get; set; }
        public SelectList FinishedGoodsWarehouseList { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int WorkOrderId { get; set; }

            [Display(Name = "الكمية المستلمة (درجة أولى)")]
            public int FirstGradeQuantity { get; set; }

            [Display(Name = "المخزن (درجة أولى)")]
            public int? FirstGradeWarehouseId { get; set; }

            [Display(Name = "الكمية المستلمة (درجة ثانية)")]
            public int SecondGradeQuantity { get; set; }

            [Display(Name = "المخزن (درجة ثانية)")]
            public int? SecondGradeWarehouseId { get; set; }

            // *** تمت إضافة هذا الحقل ***
            [Display(Name = "التكلفة النهائية للقطعة")]
            [DataType(DataType.Currency)]
            public decimal FinalUnitCost { get; set; }
        }

        public decimal CalculatedUnitCost { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            WorkOrder = await _context.WorkOrders.Include(wo => wo.Product).FirstOrDefaultAsync(wo => wo.Id == id);
            if (WorkOrder == null || WorkOrder.Status == WorkOrderStatus.Completed) return NotFound();

            var quantityProduced = await _context.ProductionLogs
                .Where(p => p.WorkOrderId == id && p.Department == Department.Packaging)
                .SumAsync(p => p.QuantityProduced);

            CalculatedUnitCost = await CalculateWorkOrderUnitCostAsync(id.Value, quantityProduced);

            Input = new InputModel
            {
                WorkOrderId = WorkOrder.Id,
                FirstGradeQuantity = quantityProduced,
                FinalUnitCost = CalculatedUnitCost // وضع التكلفة المحسوبة كقيمة افتراضية
            };

            FinishedGoodsWarehouseList = new SelectList(
                await _context.Warehouses.Where(w => w.Type == WarehouseType.FinishedGoods).OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            WorkOrder = await _context.WorkOrders.FindAsync(Input.WorkOrderId);
            if (WorkOrder == null) return NotFound();

            // ... (باقي عمليات التحقق كما هي)
            if (Input.FirstGradeQuantity > 0 && Input.FirstGradeWarehouseId == null)
            {
                ModelState.AddModelError("Input.FirstGradeWarehouseId", "يجب تحديد مخزن لمنتجات الدرجة الأولى.");
            }
            if (Input.SecondGradeQuantity > 0 && Input.SecondGradeWarehouseId == null)
            {
                ModelState.AddModelError("Input.SecondGradeWarehouseId", "يجب تحديد مخزن لمنتجات الدرجة الثانية.");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.WorkOrderId);
                return Page();
            }

            // إعادة حساب التكلفة الأصلية للمقارنة
            var totalReceived = Input.FirstGradeQuantity + Input.SecondGradeQuantity;
            var originalCalculatedCost = await CalculateWorkOrderUnitCostAsync(WorkOrder.Id, totalReceived);

            // *** تم التعديل هنا: تسجيل العملية في سجل المراجعة إذا تغيرت التكلفة ***
            if (originalCalculatedCost != Input.FinalUnitCost)
            {
                var user = await _userManager.GetUserAsync(User);
                var auditLog = new AuditLog
                {
                    UserName = user?.UserName ?? "System",
                    ScreenName = "استلام منتج نهائي",
                    Action = "تعديل يدوي للتكلفة",
                    Details = $"تم تغيير تكلفة المنتج لأمر الشغل {WorkOrder.WorkOrderNumber} من {originalCalculatedCost:C} إلى {Input.FinalUnitCost:C}"
                };
                _context.AuditLogs.Add(auditLog);
            }

            // استلام المنتجات بالتكلفة النهائية (المعدلة أو الأصلية)
            if (Input.FirstGradeQuantity > 0 && Input.FirstGradeWarehouseId.HasValue)
            {
                await ReceiveProductAsync(WorkOrder.ProductId, Input.FirstGradeWarehouseId.Value, Input.FirstGradeQuantity, "First-Grade", Input.FinalUnitCost);
            }
            if (Input.SecondGradeQuantity > 0 && Input.SecondGradeWarehouseId.HasValue)
            {
                await ReceiveProductAsync(WorkOrder.ProductId, Input.SecondGradeWarehouseId.Value, Input.SecondGradeQuantity, "Second-Grade", Input.FinalUnitCost);
            }

            WorkOrder.Status = WorkOrderStatus.Completed;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = Input.WorkOrderId });
        }

        // ... (باقي الوظائف المساعدة تبقى كما هي)
        private async Task ReceiveProductAsync(int productId, int warehouseId, int quantity, string qualityGrade, decimal unitCost)
        {
            var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(i => i.WarehouseId == warehouseId && i.ProductId == productId);
            if (inventoryItem != null) { inventoryItem.StockLevel += quantity; }
            else { _context.InventoryItems.Add(new InventoryItem { WarehouseId = warehouseId, ProductId = productId, ItemType = InventoryItemType.FinishedGood, StockLevel = quantity }); }
            _context.ProductionReceiptLogs.Add(new ProductionReceiptLog { WorkOrderId = WorkOrder.Id, ProductId = productId, WarehouseId = warehouseId, Quantity = quantity, QualityGrade = qualityGrade, UnitCost = unitCost });
        }
        private async Task<decimal> CalculateWorkOrderUnitCostAsync(int workOrderId, int totalReceivedQuantity)
        {
            if (totalReceivedQuantity == 0) return 0;
            var issuedMaterials = await _context.MaterialIssuanceNoteDetails.Where(d => d.MaterialIssuanceNote.WorkOrderId == workOrderId).ToListAsync();
            decimal totalMaterialCost = 0;
            foreach (var item in issuedMaterials) { var avgCost = await GetAverageMaterialCost(item.MaterialId); totalMaterialCost += (decimal)item.Quantity * avgCost; }
            var productionLogs = await _context.ProductionLogs.Where(p => p.WorkOrderId == workOrderId).ToListAsync();
            var departmentCosts = await _context.DepartmentCosts.ToDictionaryAsync(dc => dc.Department, dc => dc.CostPerUnit);
            decimal totalLaborCost = productionLogs.Sum(p => p.QuantityProduced * departmentCosts.GetValueOrDefault(p.Department, 0));
            decimal totalDirectCost = totalMaterialCost + totalLaborCost;
            return totalDirectCost / totalReceivedQuantity;
        }
        private async Task<decimal> GetAverageMaterialCost(int materialId)
        {
            var receipts = await _context.GoodsReceiptNoteDetails.Where(d => d.MaterialId == materialId && d.UnitPrice > 0 && d.Quantity > 0).ToListAsync();
            if (!receipts.Any()) return 0;
            var totalCost = receipts.Sum(d => (decimal)d.Quantity * d.UnitPrice);
            var totalQuantity = receipts.Sum(d => d.Quantity);
            if (totalQuantity == 0) return 0;
            return totalCost / (decimal)totalQuantity;
        }
    }
}
