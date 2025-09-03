using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WorkOrder WorkOrder { get; set; }

        public SelectList ProductList { get; set; }

        public IActionResult OnGet()
        {
            // تجهيز قائمة الموديلات للاختيار منها
            ProductList = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ProductList = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
                return Page();
            }

            // تعيين القيم الافتراضية قبل الحفظ
            WorkOrder.CreationDate = DateTime.Now;
            WorkOrder.Status = Enums.WorkOrderStatus.New;
            // إنشاء رقم فريد لأمر الشغل (مثال: WO-2023-101)
            WorkOrder.WorkOrderNumber = $"WO-{DateTime.Now.Year}-{(_context.WorkOrders.Count() + 1)}";


            _context.WorkOrders.Add(WorkOrder);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index"); // سنقوم بإنشاء هذه الصفحة لاحقاً
        }
    }
}
