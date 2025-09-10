using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.MaterialRequisitions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MaterialRequisition> MaterialRequisitions { get; set; } = default!;

        public async Task OnGetAsync()
        {
            MaterialRequisitions = await _context.MaterialRequisitions
                .Include(m => m.WorkOrder)
                .Where(m => m.Status == Enums.RequisitionStatus.Pending)
                .OrderByDescending(m => m.RequestDate)
                .ToListAsync();
        }
    }
}
