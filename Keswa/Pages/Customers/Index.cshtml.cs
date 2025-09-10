using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Customers
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CustomerViewModel> Customers { get; set; } = new List<CustomerViewModel>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;


        public async Task OnGetAsync()
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(SearchTerm) || (c.PhoneNumber != null && c.PhoneNumber.Contains(SearchTerm)));
            }

            var totalRecords = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalRecords / (double)PageSize);

            var customersFromDb = await query.OrderBy(c => c.Name)
                                 .Skip((CurrentPage - 1) * PageSize)
                                 .Take(PageSize)
                                 .AsNoTracking()
                                 .ToListAsync();

            var customerIds = customersFromDb.Select(c => c.Id).ToList();
            var balances = await _context.CustomerTransactions
                .Where(t => customerIds.Contains(t.CustomerId))
                .GroupBy(t => t.CustomerId)
                .Select(g => new {
                    CustomerId = g.Key,
                    Balance = g.Sum(t => t.Debit - t.Credit)
                })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Balance);

            Customers = customersFromDb.Select(c => new CustomerViewModel
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                Balance = balances.GetValueOrDefault(c.Id, 0)
            }).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // *** تم التعديل هنا: التحقق من وجود طلبيات مرتبطة قبل الحذف ***
            var hasSalesOrders = await _context.SalesOrders.AnyAsync(so => so.CustomerId == id);
            if (hasSalesOrders)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف هذا العميل لأنه مرتبط بطلبيات مسجلة في النظام.";
                return RedirectToPage();
            }

            var customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                // حذف المعاملات المالية المرتبطة بالعميل أولاً
                var transactions = await _context.CustomerTransactions.Where(t => t.CustomerId == id).ToListAsync();
                _context.CustomerTransactions.RemoveRange(transactions);

                // ثم حذف العميل نفسه
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"تم حذف العميل '{customer.Name}' وجميع معاملاته بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = "لم يتم العثور على العميل المطلوب.";
            }

            return RedirectToPage();
        }
    }

    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal Balance { get; set; }
    }
}
