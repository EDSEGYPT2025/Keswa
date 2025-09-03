using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.PurchaseOrders
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PurchaseOrder PurchaseOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // جلب بيانات أمر الشراء مع تضمين جميع التفاصيل المرتبطة به
            PurchaseOrder = await _context.PurchaseOrders
                .Include(p => p.WorkOrder) // تضمين بيانات أمر الشغل
                .Include(p => p.Details)    // تضمين قائمة تفاصيل أمر الشراء
                    .ThenInclude(d => d.Material) // ولكل تفصيل، قم بتضمين بيانات المادة الخام
                .FirstOrDefaultAsync(m => m.Id == id);

            if (PurchaseOrder == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
