using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.GoodsReceipts
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GoodsReceiptNote GoodsReceiptNote { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GoodsReceiptNote = await _context.GoodsReceiptNotes
                .Include(g => g.Warehouse)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (GoodsReceiptNote == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goodsReceiptNoteToDelete = await _context.GoodsReceiptNotes
                .Include(g => g.Details)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (goodsReceiptNoteToDelete == null)
            {
                return RedirectToPage("./Index");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // التراجع عن تحديث الرصيد
                    foreach (var detail in goodsReceiptNoteToDelete.Details)
                    {
                        var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(
                            i => i.WarehouseId == goodsReceiptNoteToDelete.WarehouseId && i.MaterialId == detail.MaterialId);

                        if (inventoryItem != null)
                        {
                            // طرح الكمية التي تم إضافتها بهذا السند
                            inventoryItem.StockLevel -= detail.Quantity;
                        }
                    }

                    // سيتم حذف تفاصيل السند تلقائياً بسبب علاقة الربط
                    _context.GoodsReceiptNotes.Remove(goodsReceiptNoteToDelete);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    // يمكنك إضافة رسالة خطأ هنا لعرضها للمستخدم
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
