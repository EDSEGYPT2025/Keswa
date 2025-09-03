using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
        public GoodsReceiptNote GoodsReceiptNote { get; set; }

        public SelectList WarehouseList { get; set; }
        public SelectList MaterialList { get; set; }

        public IActionResult OnGet()
        {
            // تجهيز القوائم المنسدلة
            WarehouseList = new SelectList(_context.Warehouses.OrderBy(w => w.Name), "Id", "Name");
            MaterialList = new SelectList(_context.Materials.OrderBy(m => m.Name), "Id", "Name");

            // تهيئة السند بتاريخ اليوم
            GoodsReceiptNote = new GoodsReceiptNote
            {
                ReceiptDate = System.DateTime.Now
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // إعادة تجهيز القوائم في حالة وجود خطأ
                WarehouseList = new SelectList(_context.Warehouses.OrderBy(w => w.Name), "Id", "Name");
                MaterialList = new SelectList(_context.Materials.OrderBy(m => m.Name), "Id", "Name");
                return Page();
            }

            // بدء معاملة لضمان حفظ البيانات معاً أو عدم حفظها على الإطلاق
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. حفظ رأس السند أولاً
                    _context.GoodsReceiptNotes.Add(GoodsReceiptNote);
                    await _context.SaveChangesAsync();

                    // 2. تحديث أرصدة المخزون بناءً على تفاصيل السند
                    foreach (var detail in GoodsReceiptNote.Details)
                    {
                        // البحث عن الصنف في المخزن المحدد
                        var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(
                            i => i.WarehouseId == GoodsReceiptNote.WarehouseId && i.MaterialId == detail.MaterialId);

                        if (inventoryItem != null)
                        {
                            // إذا كان الصنف موجوداً، قم بزيادة الرصيد
                            inventoryItem.StockLevel += detail.Quantity;
                        }
                        else
                        {
                            // إذا كان الصنف غير موجود، قم بإنشاء سجل جديد له
                            inventoryItem = new InventoryItem
                            {
                                WarehouseId = GoodsReceiptNote.WarehouseId,
                                MaterialId = detail.MaterialId,
                                StockLevel = detail.Quantity
                            };
                            _context.InventoryItems.Add(inventoryItem);
                        }
                    }

                    // 3. حفظ التغييرات في أرصدة المخزون
                    await _context.SaveChangesAsync();

                    // 4. إتمام المعاملة بنجاح
                    await transaction.CommitAsync();
                }
                catch
                {
                    // في حالة حدوث أي خطأ، تراجع عن كل التغييرات
                    await transaction.RollbackAsync();
                    // يمكنك إضافة رسالة خطأ هنا
                    return Page();
                }
            }

            return RedirectToPage("./Index"); // سنقوم بإنشاء هذه الصفحة لاحقاً
        }
    }
}
