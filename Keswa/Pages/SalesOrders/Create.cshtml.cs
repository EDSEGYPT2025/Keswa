using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.SalesOrders
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SalesOrder SalesOrder { get; set; } = new();

        public SelectList CustomerList { get; set; }
        public SelectList ProductList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateSelectListsAsync();
            SalesOrder.OrderDate = DateTime.Today;
            SalesOrder.ExpectedDeliveryDate = DateTime.Today;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // *** تم التعديل هنا: استخدام طريقة تحقق أكثر دقة ***

            // أولاً، إزالة أي صفوف فارغة تماماً قد يرسلها المتصفح
            SalesOrder.Details.RemoveAll(d => d.ProductId == 0 && d.Quantity == 0);

            // ثانياً، التحقق من أن المستخدم أضاف صفاً واحداً على الأقل
            if (SalesOrder.Details == null || SalesOrder.Details.Count == 0)
            {
                ModelState.AddModelError("SalesOrder.Details", "يجب إضافة موديل واحد على الأقل للطلبية.");
            }
            else
            {
                // ثالثاً، التحقق من أن كل صف تمت إضافته يحتوي على موديل صالح
                foreach (var detail in SalesOrder.Details)
                {
                    if (detail.ProductId == 0)
                    {
                        ModelState.AddModelError("SalesOrder.Details", "يجب اختيار موديل صالح لكل صف في الطلبية.");
                        break; // التوقف عند أول خطأ
                    }
                }
            }


            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            // تعيين الحالة الافتراضية لكل بند في الطلبية
            foreach (var detail in SalesOrder.Details)
            {
                detail.Status = Enums.SalesOrderDetailStatus.PendingConversion;
            }

            _context.SalesOrders.Add(SalesOrder);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync()
        {
            CustomerList = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
        }
    }
}

