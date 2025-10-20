using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            // إزالة أي صفوف فارغة أضافها المستخدم
            SalesOrder.Details.RemoveAll(d => d.ProductId == 0 || d.Quantity == 0);

            // --- بداية الحل القاطع ---

            // 1. نقوم بإزالة خطأ التحقق الخاص برقم الطلبية من ModelState
            //    لأننا سنقوم بإنشائه هنا في الكود
            ModelState.Remove("SalesOrder.OrderNumber");

            // 2. نقوم بإنشاء وتعيين رقم الطلبية الفريد (GUID)
            SalesOrder.OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            // --- نهاية الحل القاطع ---

            // 3. الآن نقوم بالتحقق من صحة باقي البيانات
            if (SalesOrder.Details == null || SalesOrder.Details.Count == 0)
            {
                ModelState.AddModelError("SalesOrder.Details", "يجب إضافة موديل واحد على الأقل للطلبية.");
            }
            else
            {
                foreach (var detail in SalesOrder.Details)
                {
                    if (detail.ProductId == 0)
                    {
                        ModelState.AddModelError("SalesOrder.Details", "يجب اختيار موديل صالح لكل صف.");
                        break;
                    }
                }
            }

            // 4. التحقق النهائي من ModelState بعد إزالة الخطأ الأول
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                return Page();
            }

            // الحالة الافتراضية لكل بند
            foreach (var detail in SalesOrder.Details)
            {
                detail.Status = Enums.SalesOrderDetailStatus.PendingConversion;
            }

            _context.SalesOrders.Add(SalesOrder);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إنشاء الطلبية بنجاح برقم: {SalesOrder.OrderNumber}";
            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectListsAsync()
        {
            CustomerList = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ProductList = new SelectList(await _context.Products.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
        }
    }
}