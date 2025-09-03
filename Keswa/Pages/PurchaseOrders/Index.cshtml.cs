using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.PurchaseOrders
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly int PageSize = 10;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<PurchaseOrder> PurchaseOrders { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task OnGetAsync()
        {
            var query = _context.PurchaseOrders
                                .Include(p => p.WorkOrder) // لتضمين رقم أمر الشغل المرتبط
                                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(p => p.OrderNumber.Contains(SearchTerm) || (p.WorkOrder != null && p.WorkOrder.WorkOrderNumber.Contains(SearchTerm)));
            }

            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            PurchaseOrders = await query.OrderByDescending(p => p.OrderDate)
                                        .Skip((CurrentPage - 1) * PageSize)
                                        .Take(PageSize)
                                        .ToListAsync();
        }
    }
}
