using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.MaterialRequisitions
{
    public class ProcessModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProcessModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public MaterialRequisition MaterialRequisition { get; set; } = default!;
        public SelectList WarehouseList { get; set; }

        [BindProperty]
        public List<ProcessDetailInputModel> ProcessDetails { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            MaterialRequisition = await _context.MaterialRequisitions
                .Include(r => r.WorkOrder)
                .Include(r => r.Details)!
                .ThenInclude(d => d.Material)
                .ThenInclude(m => m.Color)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (MaterialRequisition == null || MaterialRequisition.Status == Enums.RequisitionStatus.Processed)
            {
                return NotFound();
            }

            WarehouseList = new SelectList(
                await _context.Warehouses.Where(w => w.Type == Enums.WarehouseType.RawMaterials).OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name");

            ProcessDetails = MaterialRequisition.Details.Select(d => new ProcessDetailInputModel
            {
                DetailId = d.Id,
                MaterialName = d.Material.Name,
                ColorName = d.Material.Color?.Name,
                RequestedQuantity = d.Quantity,
                IssuedQuantity = d.Quantity
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var requisition = await _context.MaterialRequisitions
                .Include(r => r.Details)!
                .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requisition == null) return NotFound();

            var issuanceDetails = new List<MaterialIssuanceNoteDetail>();

            // التحقق من الأرصدة قبل الحفظ
            foreach (var item in ProcessDetails)
            {
                if (item.IssuedQuantity > 0)
                {
                    if (!item.WarehouseId.HasValue)
                    {
                        ModelState.AddModelError("", $"يجب تحديد مخزن للمادة: {item.MaterialName}");
                        continue;
                    }
                    var materialId = requisition.Details.First(d => d.Id == item.DetailId).MaterialId;
                    var stock = await _context.InventoryItems.FirstOrDefaultAsync(i => i.MaterialId == materialId && i.WarehouseId == item.WarehouseId.Value);

                    if (stock == null || stock.StockLevel < item.IssuedQuantity)
                    {
                        var materialName = requisition.Details.First(d => d.Id == item.DetailId).Material.Name;
                        ModelState.AddModelError("", $"الكمية المصروفة من '{materialName}' أكبر من الرصيد المتاح. الرصيد: {stock?.StockLevel ?? 0}");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(id);
                return Page();
            }

            // إنشاء إذن الصرف
            var issuanceNote = new MaterialIssuanceNote
            {
                WorkOrderId = requisition.WorkOrderId,
                IssuanceDate = System.DateTime.Today,
                Notes = Notes,
            };

            // **هذا هو الجزء المسؤول عن خصم الكمية من المخزون**
            foreach (var item in ProcessDetails)
            {
                if (item.IssuedQuantity > 0)
                {
                    var materialId = requisition.Details.First(d => d.Id == item.DetailId).MaterialId;
                    var stockItem = await _context.InventoryItems.FirstAsync(i => i.MaterialId == materialId && i.WarehouseId == item.WarehouseId.Value);

                    // خصم الكمية من الرصيد
                    stockItem.StockLevel -= item.IssuedQuantity;

                    // إضافة التفاصيل لإذن الصرف
                    issuanceNote.Details.Add(new MaterialIssuanceNoteDetail
                    {
                        MaterialId = materialId,
                        Quantity = item.IssuedQuantity
                    });
                }
            }

            if (issuanceNote.Details.Any())
            {
                issuanceNote.WarehouseId = ProcessDetails.First(d => d.WarehouseId.HasValue).WarehouseId.Value;
                _context.MaterialIssuanceNotes.Add(issuanceNote);
            }

            // تحديث حالة طلب الصرف
            requisition.Status = Enums.RequisitionStatus.Processed;

            await _context.SaveChangesAsync();

            // توليد رقم إذن الصرف
            if (issuanceNote.Details.Any())
            {
                issuanceNote.IssuanceNumber = $"ISS-{issuanceNote.IssuanceDate.Year}-{issuanceNote.Id}";
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "تم صرف المواد وتحديث الأرصدة بنجاح.";
            return RedirectToPage("./Index");
        }
    }

    public class ProcessDetailInputModel
    {
        public int DetailId { get; set; }
        public string MaterialName { get; set; }
        public string? ColorName { get; set; }
        public double RequestedQuantity { get; set; }

        [Display(Name = "الكمية المصروفة")]
        public double IssuedQuantity { get; set; }

        [Display(Name = "المخزن")]
        public int? WarehouseId { get; set; }
    }
}

