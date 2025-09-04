using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keswa.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();

            Customer = customer;

            var openingBalanceTransaction = await _context.CustomerTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.CustomerId == id && t.TransactionType == "رصيد افتتاحي");

            if (openingBalanceTransaction != null)
            {
                if (openingBalanceTransaction.Debit > 0)
                {
                    Customer.OpeningBalance = openingBalanceTransaction.Debit;
                    Customer.BalanceType = Enums.BalanceType.Debit;
                }
                else
                {
                    Customer.OpeningBalance = openingBalanceTransaction.Credit;
                    Customer.BalanceType = Enums.BalanceType.Credit;
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // جلب البيانات الأصلية للمقارنة قبل التعديل
            var originalCustomer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == Customer.Id);

            if (originalCustomer == null) return NotFound();

            var auditDetails = new StringBuilder();

            // مقارنة البيانات الأساسية
            if (originalCustomer.Name != Customer.Name)
                auditDetails.AppendLine($"تم تغيير الاسم من '{originalCustomer.Name}' إلى '{Customer.Name}'.");
            if (originalCustomer.PhoneNumber != Customer.PhoneNumber)
                auditDetails.AppendLine($"تم تغيير رقم الهاتف من '{originalCustomer.PhoneNumber}' إلى '{Customer.PhoneNumber}'.");
            if (originalCustomer.Address != Customer.Address)
                auditDetails.AppendLine($"تم تغيير العنوان.");

            _context.Attach(Customer).State = EntityState.Modified;

            // تحديث أو إنشاء قيد الرصيد الافتتاحي
            var openingBalanceTransaction = await _context.CustomerTransactions
                .FirstOrDefaultAsync(t => t.CustomerId == Customer.Id && t.TransactionType == "رصيد افتتاحي");

            decimal originalBalance = openingBalanceTransaction?.Debit ?? openingBalanceTransaction?.Credit ?? 0;

            if (originalBalance != Customer.OpeningBalance)
            {
                auditDetails.AppendLine($"تم تغيير الرصيد الافتتاحي من '{originalBalance}' إلى '{Customer.OpeningBalance}'.");
            }

            if (Customer.OpeningBalance > 0)
            {
                if (openingBalanceTransaction == null)
                {
                    openingBalanceTransaction = new CustomerTransaction { CustomerId = Customer.Id, TransactionType = "رصيد افتتاحي" };
                    _context.CustomerTransactions.Add(openingBalanceTransaction);
                }

                openingBalanceTransaction.TransactionDate = System.DateTime.Today;
                openingBalanceTransaction.Debit = (Customer.BalanceType == Enums.BalanceType.Debit) ? Customer.OpeningBalance : 0;
                openingBalanceTransaction.Credit = (Customer.BalanceType == Enums.BalanceType.Credit) ? Customer.OpeningBalance : 0;
            }
            else if (openingBalanceTransaction != null)
            {
                _context.CustomerTransactions.Remove(openingBalanceTransaction);
            }

            // إنشاء سجل المراجعة فقط إذا كان هناك تغييرات
            if (auditDetails.Length > 0)
            {
                var user = await _userManager.GetUserAsync(User);
                var auditLog = new AuditLog
                {
                    UserName = user?.UserName ?? "System",
                    ScreenName = "تعديل بيانات العميل",
                    Action = "تعديل",
                    Details = $"تعديل بيانات العميل '{originalCustomer.Name}' (ID: {Customer.Id}). التفاصيل: {auditDetails}"
                };
                _context.AuditLogs.Add(auditLog);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم تعديل بيانات العميل '{Customer.Name}' بنجاح.";
            return RedirectToPage("./Index");
        }
    }
}
