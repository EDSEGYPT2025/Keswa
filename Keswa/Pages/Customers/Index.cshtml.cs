using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CustomerViewModel> Customers { get; set; } = default!;

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
                query = query.Where(c => c.Name.Contains(SearchTerm) || c.PhoneNumber.Contains(SearchTerm));
            }

            var totalRecords = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(totalRecords / (double)PageSize);

            var customersFromDb = await query.OrderBy(c => c.Name)
                                 .Skip((CurrentPage - 1) * PageSize)
                                 .Take(PageSize)
                                 .ToListAsync();

            // حساب الأرصدة بكفاءة
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
    }

    // كلاس مساعد لعرض بيانات العميل مع رصيده
    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal Balance { get; set; }
    }
}
