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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GoodsReceiptNote GoodsReceiptNote { get; set; }

        public SelectList WarehouseList { get; set; }
        public SelectList MaterialList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // جلب السند مع تفاصيله للتعديل
            GoodsReceiptNote = await _context.GoodsReceiptNotes
                .Include(g => g.Details)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (GoodsReceiptNote == null)
            {
                return NotFound();
            }

            // تجهيز القوائم المنسدلة
            WarehouseList = new SelectList(_context.Warehouses.OrderBy(w => w.Name), "Id", "Name");
            MaterialList = new SelectList(_context.Materials.OrderBy(m => m.Name), "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                WarehouseList = new SelectList(_context.Warehouses.OrderBy(w => w.Name), "Id", "Name");
                MaterialList = new SelectList(_context.Materials.OrderBy(m => m.Name), "Id", "Name");
                return Page();
            }

            var noteToUpdate = await _context.GoodsReceiptNotes
                .Include(g => g.Details)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (noteToUpdate == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. التراجع عن الكميات القديمة في المخزن القديم
                    foreach (var oldDetail in noteToUpdate.Details)
                    {
                        var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(
                            i => i.WarehouseId == noteToUpdate.WarehouseId && i.MaterialId == oldDetail.MaterialId);
                        if (inventoryItem != null)
                        {
                            inventoryItem.StockLevel -= oldDetail.Quantity;
                        }
                    }

                    // 2. تحديث بيانات رأس السند
                    noteToUpdate.ReceiptDate = GoodsReceiptNote.ReceiptDate;
                    noteToUpdate.WarehouseId = GoodsReceiptNote.WarehouseId;
                    noteToUpdate.DocumentNumber = GoodsReceiptNote.DocumentNumber;
                    noteToUpdate.TransactionType = GoodsReceiptNote.TransactionType;
                    noteToUpdate.Notes = GoodsReceiptNote.Notes;

                    // 3. حذف التفاصيل القديمة وإضافة التفاصيل الجديدة
                    _context.GoodsReceiptNoteDetails.RemoveRange(noteToUpdate.Details);
                    noteToUpdate.Details = GoodsReceiptNote.Details;

                    // 4. إضافة الكميات الجديدة إلى المخزن الجديد (قد يكون نفس المخزن أو مخزن مختلف)
                    foreach (var newDetail in noteToUpdate.Details)
                    {
                        var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(
                            i => i.WarehouseId == noteToUpdate.WarehouseId && i.MaterialId == newDetail.MaterialId);

                        if (inventoryItem != null)
                        {
                            inventoryItem.StockLevel += newDetail.Quantity;
                        }
                        else
                        {
                            _context.InventoryItems.Add(new InventoryItem
                            {
                                WarehouseId = noteToUpdate.WarehouseId,
                                MaterialId = newDetail.MaterialId,
                                StockLevel = newDetail.Quantity
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
