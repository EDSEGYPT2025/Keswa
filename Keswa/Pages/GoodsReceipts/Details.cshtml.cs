using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.GoodsReceipts
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public GoodsReceiptNote GoodsReceiptNote { get; set; } = default!;
        public decimal TotalValue { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // *** تم التعديل هنا: إضافة ThenInclude لجلب بيانات اللون ***
            var goodsreceiptnote = await _context.GoodsReceiptNotes
                .Include(g => g.Warehouse)
                .Include(g => g.Details)!
                    .ThenInclude(d => d.Material)
                        .ThenInclude(m => m.Color) // جلب اللون المرتبط بالمادة
                .FirstOrDefaultAsync(m => m.Id == id);

            if (goodsreceiptnote == null)
            {
                return NotFound();
            }

            GoodsReceiptNote = goodsreceiptnote;
            TotalValue = GoodsReceiptNote.Details.Sum(d => (decimal)d.Quantity * d.UnitPrice);

            return Page();
        }
    }
}
