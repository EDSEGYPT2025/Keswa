using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Reports
{
    public class WorkerEarningsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WorkerEarningsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? WorkerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public SelectList WorkersList { get; set; }
        public List<WorkerAssignment> Assignments { get; set; }
        public decimal TotalEarnings { get; set; }

        public async Task OnGetAsync()
        {
            WorkersList = new SelectList(await _context.Workers.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");

            var query = _context.WorkerAssignments
                .Include(wa => wa.Worker)
                .Where(wa => wa.Status == Enums.AssignmentStatus.Completed);

            if (WorkerId.HasValue)
            {
                query = query.Where(wa => wa.WorkerId == WorkerId.Value);
            }
            if (FromDate.HasValue)
            {
                query = query.Where(wa => wa.AssignedDate.Date >= FromDate.Value.Date);
            }
            if (ToDate.HasValue)
            {
                query = query.Where(wa => wa.AssignedDate.Date <= ToDate.Value.Date);
            }

            Assignments = await query.OrderByDescending(wa => wa.AssignedDate).ToListAsync();
            TotalEarnings = Assignments.Sum(a => a.Earnings);
        }
    }
}