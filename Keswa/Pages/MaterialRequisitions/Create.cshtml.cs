using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.MaterialRequisitions
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MaterialRequisition MaterialRequisition { get; set; } = new();

        public WorkOrder CurrentWorkOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(int workOrderId)
        {
            CurrentWorkOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .ThenInclude(p => p.BillOfMaterialItems)
                .ThenInclude(bom => bom.Material)
                .AsNoTracking()
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);

            if (CurrentWorkOrder == null)
            {
                return NotFound();
            }

            // تعبئة الطلب بالبيانات الأساسية والمواد المطلوبة تلقائياً
            MaterialRequisition.WorkOrderId = workOrderId;
            MaterialRequisition.Details = CurrentWorkOrder.Product.BillOfMaterialItems
                .Select(bom => new MaterialRequisitionDetail
                {
                    MaterialId = bom.MaterialId,
                    Material = bom.Material,
                    Quantity = bom.Quantity * CurrentWorkOrder.QuantityToProduce
                }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // إزالة أي صفوف فارغة
            MaterialRequisition.Details.RemoveAll(d => d.MaterialId == 0 || d.Quantity <= 0);

            if (MaterialRequisition.Details.Count == 0)
            {
                ModelState.AddModelError("", "يجب إضافة مادة خام واحدة على الأقل للطلب.");
            }

            if (!ModelState.IsValid)
            {
                // إعادة تحميل البيانات في حالة الخطأ
                await OnGetAsync(MaterialRequisition.WorkOrderId);
                return Page();
            }

            _context.MaterialRequisitions.Add(MaterialRequisition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إرسال طلب الصرف رقم {MaterialRequisition.Id} بنجاح.";
            return RedirectToPage("/WorkOrders/Details", new { id = MaterialRequisition.WorkOrderId });
        }
    }
}

