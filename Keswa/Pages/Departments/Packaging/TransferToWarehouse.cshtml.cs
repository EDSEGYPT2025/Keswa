using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Pages.Departments.Packaging
{
    public class TransferToWarehouseModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TransferToWarehouseModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TransferInputModel Input { get; set; } = new();
        public PackagingBatch Batch { get; set; }
        public SelectList FinishedGoodsWarehouses { get; set; }

        public class TransferInputModel
        {
            public int BatchId { get; set; }

            [Display(Name = "مخزن الإنتاج التام")]
            [Required(ErrorMessage = "يجب اختيار مخزن.")]
            public int WarehouseId { get; set; }

            [Display(Name = "الكمية الموردة")]
            public int Quantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // جلب بيانات التشغيلة مع الموديل والمخازن
            Batch = await _context.PackagingBatches
                .Include(b => b.WorkOrder.Product)
                .Include(b => b.PackagingAssignments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (Batch == null) return NotFound();

            // تصفية المخازن لتشمل فقط مخازن الإنتاج التام
            var warehouses = await _context.Warehouses
                .Where(w => w.Type == WarehouseType.FinishedGoods)
                .ToListAsync();

            FinishedGoodsWarehouses = new SelectList(warehouses, "Id", "Name");

            Input.BatchId = id;
            Input.Quantity = Batch.PackagingAssignments.Sum(a => a.CompletedQuantity);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync(Input.BatchId);
            }

            var batch = await _context.PackagingBatches
                .Include(b => b.WorkOrder)
                .FirstOrDefaultAsync(b => b.Id == Input.BatchId);

            if (batch == null) return NotFound();

            // 1. البحث عن الصنف في المخزن المختار
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.ProductId == batch.WorkOrder.ProductId && i.WarehouseId == Input.WarehouseId);

            if (inventoryItem != null)
            {
                // تحديث الرصيد الحالي
                inventoryItem.StockLevel += (double)Input.Quantity;
            }
            else
            {
                // إنشاء سجل مخزني جديد
                _context.InventoryItems.Add(new InventoryItem
                {
                    WarehouseId = Input.WarehouseId,
                    ProductId = batch.WorkOrder.ProductId,
                    StockLevel = (double)Input.Quantity,
                    // --- التعديل الصحيح هنا: استخدام FinishedGood بدلاً من Product ---
                    ItemType = InventoryItemType.FinishedGood
                });
            }

            // 2. تحديث حالة التشغيلة إلى "تم التوريد للمخزن"
            batch.Status = PackagingBatchStatus.TransferredToStore;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم توريد الكمية للمخزن كمنتج نهائي بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}