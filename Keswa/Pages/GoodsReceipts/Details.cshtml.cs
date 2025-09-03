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

        public GoodsReceiptNote GoodsReceiptNote { get; set; }
        public decimal TotalValue { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // جلب بيانات السند المحدد مع تضمين جميع التفاصيل المرتبطة به
            GoodsReceiptNote = await _context.GoodsReceiptNotes
                .Include(g => g.Warehouse) // تضمين بيانات المخزن
                .Include(g => g.Details)    // تضمين قائمة تفاصيل السند
                    .ThenInclude(d => d.Material) // ولكل تفصيل، قم بتضمين بيانات المادة الخام
                .FirstOrDefaultAsync(m => m.Id == id);

            if (GoodsReceiptNote == null)
            {
                return NotFound();
            }

            // حساب القيمة الإجمالية للسند
            TotalValue = GoodsReceiptNote.Details.Sum(d => (decimal)d.Quantity * d.UnitPrice);

            return Page();
        }
    }
}
