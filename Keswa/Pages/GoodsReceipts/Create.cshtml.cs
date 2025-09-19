using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Keswa.Pages.GoodsReceipts
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GoodsReceiptNote GoodsReceiptNote { get; set; } = new();

        public SelectList WarehouseList { get; set; }
        public SelectList BaseMaterialList { get; set; }
        public string MaterialsJson { get; set; }

        public async Task OnGetAsync()
        {
            WarehouseList = new SelectList(await _context.Warehouses.Where(w => w.Type == WarehouseType.RawMaterials).OrderBy(w => w.Name).ToListAsync(), "Id", "Name");

            var allMaterials = await _context.Materials
                .Include(m => m.Color)
                .OrderBy(m => m.Name)
                .ToListAsync();

            var groupedMaterials = allMaterials
                .GroupBy(m => m.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new { id = m.Id, color = m.Color?.Name ?? "بدون لون" }).ToList()
                );

            MaterialsJson = JsonSerializer.Serialize(groupedMaterials);
            BaseMaterialList = new SelectList(groupedMaterials.Keys.ToList());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            GoodsReceiptNote.Details.RemoveAll(d => d.MaterialId == 0 || d.Quantity <= 0);

            if (GoodsReceiptNote.Details.Count == 0)
            {
                ModelState.AddModelError("", "يجب إضافة صنف واحد على الأقل للسند.");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // *** تم التعديل هنا: إضافة منطق تحديث المخزون ***
            // الخطوة 1: تحديث أرصدة المخزون
            foreach (var detail in GoodsReceiptNote.Details)
            {
                var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(i =>
                    i.WarehouseId == GoodsReceiptNote.WarehouseId &&
                    i.MaterialId == detail.MaterialId);

                if (inventoryItem != null)
                {
                    // إذا كان الصنف موجوداً، قم بزيادة الرصيد
                    inventoryItem.StockLevel += detail.Quantity;
                }
                else
                {
                    // إذا كان الصنف جديداً على هذا المخزن، قم بإنشاء سجل رصيد جديد
                    _context.InventoryItems.Add(new InventoryItem
                    {
                        WarehouseId = GoodsReceiptNote.WarehouseId,
                        MaterialId = detail.MaterialId,
                        ItemType = InventoryItemType.RawMaterial,
                        StockLevel = detail.Quantity
                    });
                }
            }

            // الخطوة 2: حفظ سند الاستلام
            _context.GoodsReceiptNotes.Add(GoodsReceiptNote);

            // الخطوة 3: حفظ جميع التغييرات (تحديث المخزون + السند الجديد)
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حفظ سند الاستلام وتحديث أرصدة المخزون بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}
