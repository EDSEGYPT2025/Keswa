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
                .ThenInclude(m => m.Color) // *** تم التعديل هنا: جلب بيانات اللون المرتبطة ***
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
                    Material = bom.Material, // الكائن الآن يحتوي على اللون
                    Quantity = bom.Quantity * CurrentWorkOrder.QuantityToProduce
                }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // إزالة أي صفوف فارغة
            MaterialRequisition.Details.RemoveAll(d => d.MaterialId == 0 || d.Quantity <= 0);

            // --- بداية الحل القاطع ---

            // 1. نقوم بإزالة خطأ التحقق الخاص برقم طلب الصرف من ModelState
            //    لأننا سنقوم بإنشائه هنا في الكود
            ModelState.Remove("MaterialRequisition.MaterialRequisitionNumber");

            // 2. نقوم بإنشاء وتعيين رقم فريد لطلب الصرف (يمكنك استخدام أي نظام ترقيم تفضله)
            //    هنا سنستخدم نظامًا يعتمد على تاريخ ووقت الإنشاء لضمان تفرده
            MaterialRequisition.MaterialRequisitionNumber = $"REQ-{DateTime.Now:yyyyMMddHHmmss}";

            // --- نهاية الحل القاطع ---

            if (MaterialRequisition.Details.Count == 0)
            {
                ModelState.AddModelError("", "يجب إضافة مادة خام واحدة على الأقل للطلب.");
            }

            // التحقق النهائي من صحة باقي البيانات
            if (!ModelState.IsValid)
            {
                // إعادة تحميل البيانات في حالة الخطأ
                await OnGetAsync(MaterialRequisition.WorkOrderId);
                return Page();
            }

            _context.MaterialRequisitions.Add(MaterialRequisition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إرسال طلب الصرف رقم {MaterialRequisition.MaterialRequisitionNumber} بنجاح.";
            return RedirectToPage("/WorkOrders/Details", new { id = MaterialRequisition.WorkOrderId });
        }
    }
}
