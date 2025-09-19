using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Keswa.Pages.Inventory
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Warehouse Warehouse { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // *** تم التعديل هنا: إضافة ThenInclude لجلب بيانات اللون والمنتج ***
            Warehouse = await _context.Warehouses
                .Include(w => w.InventoryItems)
                    .ThenInclude(i => i.Material)
                        .ThenInclude(m => m.Color) // جلب اللون المرتبط بالمادة الخام
                .Include(w => w.InventoryItems)
                    .ThenInclude(i => i.Product) // جلب المنتج النهائي
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Warehouse == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
