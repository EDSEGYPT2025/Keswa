using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Keswa.Pages.Customers
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // الخطوة الأولى: إضافة العميل وحفظه للحصول على ID
            _context.Customers.Add(Customer);
            await _context.SaveChangesAsync();

            // الخطوة الثانية: التحقق من وجود رصيد افتتاحي وإنشاء القيد
            if (Customer.OpeningBalance > 0)
            {
                var openingTransaction = new CustomerTransaction
                {
                    CustomerId = Customer.Id, // ربط القيد بالعميل الجديد
                    TransactionDate = DateTime.Today,
                    TransactionType = "رصيد افتتاحي",
                    Debit = (Customer.BalanceType == Enums.BalanceType.Debit) ? Customer.OpeningBalance : 0,
                    Credit = (Customer.BalanceType == Enums.BalanceType.Credit) ? Customer.OpeningBalance : 0,
                };
                _context.CustomerTransactions.Add(openingTransaction);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"تم إنشاء العميل '{Customer.Name}' بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}
