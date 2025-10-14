using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Inventory
{
    public class ProcessReturnModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProcessReturnModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MaterialReturnNote MaterialReturnNote { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MaterialReturnNote = await _context.MaterialReturnNotes
                .Include(m => m.WorkOrder)
                .Include(m => m.Details).ThenInclude(d => d.Material).ThenInclude(ma => ma.Color)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (MaterialReturnNote == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var returnNote = await _context.MaterialReturnNotes
                .Include(m => m.Details)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (returnNote == null)
            {
                return NotFound();
            }

            // *** تم التصحيح هنا: استخدام الحالة الصحيحة من ملف الـ Enum ***
            returnNote.Status = RequisitionStatus.Approved;

            // تحديث المخزون
            foreach (var detail in returnNote.Details)
            {
                var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(i => i.MaterialId == detail.MaterialId);
                if (inventoryItem != null)
                {
                    // *** تم التصحيح هنا: استخدام اسم حقل الكمية الصحيح ***
                    inventoryItem.StockLevel += detail.QuantityReturned;
                }
                else
                {
                    _context.InventoryItems.Add(new InventoryItem
                    {
                        MaterialId = detail.MaterialId,
                        // *** تم التصحيح هنا: استخدام اسم حقل الكمية الصحيح ***
                        StockLevel = detail.QuantityReturned
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تمت الموافقة على سند المرتجع وتحديث المخزون بنجاح.";
            return RedirectToPage("./PendingReturns");
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var returnNote = await _context.MaterialReturnNotes.FindAsync(id);
            if (returnNote == null)
            {
                return NotFound();
            }

            // *** تم التصحيح هنا: استخدام الحالة الصحيحة من ملف الـ Enum ***
            returnNote.Status = RequisitionStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["InfoMessage"] = "تم رفض سند المرتجع.";
            return RedirectToPage("./PendingReturns");
        }
    }
}