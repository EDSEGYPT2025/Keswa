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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; }

        public SelectList MaterialList { get; set; }

        //public async Task<IActionResult> OnGetAsync(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    // جلب الموديل مع قائمة مكوناته للتعديل

        //    Product = await _context.Products
        //        .Include(p => p.BillOfMaterialItems)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (Product == null)
        //    {
        //        return NotFound();
        //    }

        //    // تجهيز قائمة المواد الخام
        //    MaterialList = new SelectList(_context.Materials.OrderBy(m => m.Name), "Id", "Name");
        //    return Page();
        //}

        // اظهار اللون مع الحامه 
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // جلب الموديل مع قائمة مكوناته للتعديل
            Product = await _context.Products
                .Include(p => p.BillOfMaterialItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Product == null)
            {
                return NotFound();
            }

            // *** تم التعديل هنا: تجهيز قائمة المواد الخام مع اللون ***
            var materialsForList = await _context.Materials
                .Include(m => m.Color) // جلب بيانات اللون المرتبطة
                .OrderBy(m => m.Name)
                .Select(m => new {
                    m.Id,
                    // دمج الاسم مع اللون في نص واحد
                    DisplayText = m.Name + (m.Color != null ? $" - {m.Color.Name}" : "")
                })
                .ToListAsync();

            MaterialList = new SelectList(materialsForList, "Id", "DisplayText");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var productToUpdate = await _context.Products
                .Include(p => p.BillOfMaterialItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productToUpdate == null)
            {
                return NotFound();
            }

            // تحديث بيانات الموديل الأساسية
            productToUpdate.Code = Product.Code;
            productToUpdate.Name = Product.Name;
            productToUpdate.Description = Product.Description;

            // حذف المكونات القديمة وإضافة المكونات الجديدة
            _context.BillOfMaterialItems.RemoveRange(productToUpdate.BillOfMaterialItems);
            if (Product.BillOfMaterialItems != null)
            {
                productToUpdate.BillOfMaterialItems = Product.BillOfMaterialItems;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
