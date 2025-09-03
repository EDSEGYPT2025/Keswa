using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Administration
{
    public class AuditLogModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AuditLogModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<AuditLog> AuditLogs { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task OnGetAsync()
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a => a.UserName.Contains(SearchTerm) || a.Action.Contains(SearchTerm) || a.ScreenName.Contains(SearchTerm));
            }
            if (StartDate.HasValue)
            {
                query = query.Where(a => a.Timestamp.Date >= StartDate.Value.Date);
            }
            if (EndDate.HasValue)
            {
                query = query.Where(a => a.Timestamp.Date <= EndDate.Value.Date);
            }

            var totalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            AuditLogs = await query.OrderByDescending(a => a.Timestamp)
                                   .Skip((CurrentPage - 1) * PageSize)
                                   .Take(PageSize)
                                   .ToListAsync();
        }
    }
}
