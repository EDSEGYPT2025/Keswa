using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.WorkOrders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WorkOrder WorkOrder { get; set; }

        public SelectList ProductList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WorkOrder = await _context.WorkOrders.FirstOrDefaultAsync(m => m.Id == id);

            if (WorkOrder == null)
            {
                return NotFound();
            }

            // تجهيز قائمة الموديلات
            ProductList = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ProductList = new SelectList(_context.Products.OrderBy(p => p.Name), "Id", "Name");
                return Page();
            }

            // لا نسمح بتعديل رقم أمر الشغل أو تاريخ الإنشاء
            _context.Attach(WorkOrder).State = EntityState.Modified;
            _context.Entry(WorkOrder).Property(x => x.WorkOrderNumber).IsModified = false;
            _context.Entry(WorkOrder).Property(x => x.CreationDate).IsModified = false;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkOrderExists(WorkOrder.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool WorkOrderExists(int id)
        {
            return _context.WorkOrders.Any(e => e.Id == id);
        }
    }
}
