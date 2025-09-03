using Keswa.Data;
using Keswa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keswa.Pages.MaterialIssuances
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MaterialIssuanceNote MaterialIssuanceNote { get; set; } = new();

        public SelectList WarehouseList { get; set; }
        public SelectList MaterialList { get; set; }
        public WorkOrder CurrentWorkOrder { get; set; }

        // *** تمت إضافة هذه الخاصية ***
        public List<RequiredMaterialViewModel> RequiredMaterials { get; set; }

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

            // *** تم إضافة هذا الجزء لحساب الكميات المطلوبة ***
            RequiredMaterials = CurrentWorkOrder.Product.BillOfMaterialItems
                .Select(bom => new RequiredMaterialViewModel
                {
                    MaterialName = bom.Material.Name,
                    TotalRequiredQuantity = bom.Quantity * CurrentWorkOrder.QuantityToProduce,
                    Unit = bom.Material.Unit
                }).ToList();


            MaterialIssuanceNote.WorkOrderId = workOrderId;

            WarehouseList = new SelectList(await _context.Warehouses.OrderBy(w => w.Name).ToListAsync(), "Id", "Name");
            MaterialList = new SelectList(await _context.Materials.OrderBy(m => m.Name).ToListAsync(), "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            MaterialIssuanceNote.Details.RemoveAll(d => d.MaterialId == 0 || d.Quantity <= 0);

            if (MaterialIssuanceNote.Details.Count == 0)
            {
                ModelState.AddModelError("MaterialIssuanceNote.Details", "يجب إضافة صنف واحد على الأقل للإذن.");
            }

            foreach (var detail in MaterialIssuanceNote.Details)
            {
                var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(i =>
                    i.WarehouseId == MaterialIssuanceNote.WarehouseId && i.MaterialId == detail.MaterialId);

                if (inventoryItem == null || inventoryItem.StockLevel < detail.Quantity)
                {
                    var materialName = (await _context.Materials.FindAsync(detail.MaterialId))?.Name;
                    ModelState.AddModelError("", $"الكمية المطلوبة من '{materialName}' غير متوفرة في المخزن المحدد. الرصيد المتاح: {inventoryItem?.StockLevel ?? 0}");
                }
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(MaterialIssuanceNote.WorkOrderId); // إعادة تحميل البيانات
                return Page();
            }

            foreach (var detail in MaterialIssuanceNote.Details)
            {
                var inventoryItem = await _context.InventoryItems.FirstAsync(i =>
                    i.WarehouseId == MaterialIssuanceNote.WarehouseId && i.MaterialId == detail.MaterialId);

                inventoryItem.StockLevel -= detail.Quantity;
            }

            _context.MaterialIssuanceNotes.Add(MaterialIssuanceNote);
            await _context.SaveChangesAsync();

            MaterialIssuanceNote.IssuanceNumber = $"ISS-{MaterialIssuanceNote.IssuanceDate.Year}-{MaterialIssuanceNote.Id}";
            await _context.SaveChangesAsync();

            return RedirectToPage("/WorkOrders/Details", new { id = MaterialIssuanceNote.WorkOrderId });
        }
    }

    // كلاس مساعد لعرض الكميات المطلوبة
    public class RequiredMaterialViewModel
    {
        public string MaterialName { get; set; }
        public double TotalRequiredQuantity { get; set; }
        public Enums.UnitOfMeasure Unit { get; set; }
    }
}
