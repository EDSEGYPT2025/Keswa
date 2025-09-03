using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IActionResult OnGet()
        {
            // تجهيز قائمة المواد الخام للاختيار منها
            MaterialList = new SelectList(_context.Materials.OrderBy(m => m.Name), "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // *** تم التعديل هنا: إنشاء كائن جديد ونقل المكونات إليه ***
            var newProduct = new Product();

            if (await TryUpdateModelAsync<Product>(
                newProduct,
                "Product", // Prefix for form value matching
                p => p.Code, p => p.Name, p => p.Description))
            {
                // التحقق من وجود مكونات وتعيينها للموديل الجديد
                if (Product.BillOfMaterialItems != null)
                {
                    newProduct.BillOfMaterialItems = Product.BillOfMaterialItems;
                }

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }

            // في حالة فشل الربط، أعد تحميل القائمة
            MaterialList = new SelectList(_context.Materials.OrderBy(m => m.Name), "Id", "Name");
            return Page();
        }
    }
}
