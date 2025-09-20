using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly int PageSize = 10;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<WorkOrder> WorkOrders { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task OnGetAsync()
        {
            var query = _context.WorkOrders
                                .Include(w => w.Product) // لتضمين اسم الموديل
                                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(w => w.WorkOrderNumber.Contains(SearchTerm) || w.Product.Name.Contains(SearchTerm));
            }

            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            WorkOrders = await query.OrderByDescending(w => w.Id)
                                    .Skip((CurrentPage - 1) * PageSize)
                                    .Take(PageSize)
                                    .ToListAsync();
        }
    }
}
