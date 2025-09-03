using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Keswa.Pages.SalesOrders
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<SalesOrder> SalesOrders { get; set; } = default!;

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
            var query = _context.SalesOrders
                                .Include(s => s.Customer)
                                .AsNoTracking();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(s => s.Id.ToString().Contains(SearchTerm) || s.Customer.Name.Contains(SearchTerm));
            }

            var totalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            SalesOrders = await query.OrderByDescending(s => s.OrderDate)
                                     .Skip((CurrentPage - 1) * PageSize)
                                     .Take(PageSize)
                                     .ToListAsync();
        }
    }
}

