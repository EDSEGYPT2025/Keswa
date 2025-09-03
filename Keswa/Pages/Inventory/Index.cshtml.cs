using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly int PageSize = 10; // تحديد عدد العناصر في كل صفحة

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Warehouse> Warehouses { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task OnGetAsync()
        {
            // البدء ببناء الاستعلام دون تنفيذه
            var query = _context.Warehouses
                                .Include(w => w.InventoryItems)
                                .AsQueryable();

            // إذا كان هناك مصطلح بحث، قم بتطبيقه على الاستعلام
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(w => w.Name.Contains(SearchTerm));
            }

            // حساب العدد الإجمالي للصفحات
            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            // جلب البيانات الخاصة بالصفحة الحالية فقط
            Warehouses = await query.OrderBy(w => w.Name)
                                    .Skip((CurrentPage - 1) * PageSize)
                                    .Take(PageSize)
                                    .ToListAsync();
        }
    }
}
