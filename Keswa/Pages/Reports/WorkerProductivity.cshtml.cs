using Keswa.Data;
using Keswa.Enums;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Reports
{
    public class WorkerProductivityModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WorkerProductivityModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // خصائص الفلاتر
        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedWorkerId { get; set; }

        // قوائم الفلاتر المنسدلة
        public SelectList WorkerList { get; set; }

        // نتائج التقرير
        public List<ProductivityViewModel> ProductivityReport { get; set; }

        public async Task OnGetAsync()
        {
            // تجهيز قائمة العمال للفلتر
            WorkerList = new SelectList(await _context.Workers.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");

            var query = _context.ProductionLogs.AsQueryable();

            // تطبيق الفلاتر
            if (StartDate.HasValue)
            {
                query = query.Where(p => p.LogDate.Date >= StartDate.Value.Date);
            }
            if (EndDate.HasValue)
            {
                query = query.Where(p => p.LogDate.Date <= EndDate.Value.Date);
            }
            if (SelectedWorkerId.HasValue)
            {
                query = query.Where(p => p.WorkerId == SelectedWorkerId.Value);
            }

            // تجميع البيانات وحساب الإنتاجية
            ProductivityReport = await query
                .Include(p => p.Worker)
                .GroupBy(p => new { p.WorkerId, p.Worker.Name, p.Worker.Department })
                .Select(g => new ProductivityViewModel
                {
                    WorkerName = g.Key.Name,
                    Department = g.Key.Department,
                    TotalQuantityProduced = g.Sum(p => p.QuantityProduced)
                })
                .OrderByDescending(r => r.TotalQuantityProduced)
                .ToListAsync();
        }
    }

    // كلاس مساعد لعرض نتائج التقرير
    public class ProductivityViewModel
    {
        public string WorkerName { get; set; }
        public Department Department { get; set; }
        public int TotalQuantityProduced { get; set; }
    }
}
