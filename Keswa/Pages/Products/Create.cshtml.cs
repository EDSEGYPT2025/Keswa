using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; }

        public SelectList MaterialList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // *** تم التعديل هنا: تجهيز القائمة لعرض الاسم مع اللون ***
            var materialsForList = await _context.Materials
                .Include(m => m.Color) // جلب بيانات اللون المرتبطة
                .OrderBy(m => m.Name)
                .Select(m => new
                {
                    m.Id,
                    // دمج الاسم مع اللون في نص واحد
                    DisplayText = m.Name + (m.Color != null ? $" - {m.Color.Name}" : "")
                })
                .ToListAsync();

            MaterialList = new SelectList(materialsForList, "Id", "DisplayText");
            return Page();
        }
        // بدون لون الخامه 
        //public async Task<IActionResult> OnGetAsync()
        //{
        //    // *** تم التعديل هنا: تجهيز القائمة لعرض اسم المادة فقط ***
        //    var materialsForList = await _context.Materials
        //        .OrderBy(m => m.Name)
        //        .Select(m => new {
        //            m.Id,
        //            DisplayText = m.Name // تم تغييرها لتعرض الاسم فقط
        //        })
        //        .ToListAsync();

        //    MaterialList = new SelectList(materialsForList, "Id", "DisplayText");
        //    return Page();
        //}

        public async Task<IActionResult> OnPostAsync()
        {
            // إزالة المكونات الفارغة قبل التحقق
            Product.BillOfMaterialItems?.RemoveAll(i => i.MaterialId == 0);

            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // إعادة تحميل القائمة في حالة الخطأ
                return Page();
            }

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إنشاء الموديل '{Product.Name}' بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}
