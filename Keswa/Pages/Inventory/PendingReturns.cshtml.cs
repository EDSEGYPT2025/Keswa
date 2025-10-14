using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.Inventory
{
    public class PendingReturnsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PendingReturnsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MaterialReturnNote> PendingReturnNotes { get; set; }

        public async Task OnGetAsync()
        {
            PendingReturnNotes = await _context.MaterialReturnNotes
                .Include(m => m.WorkOrder)
                .Where(m => m.Status == Enums.RequisitionStatus.Pending)
                .OrderByDescending(m => m.ReturnDate)
                .ToListAsync();
        }
    }
}