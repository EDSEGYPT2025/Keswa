using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "المخزن")]
        public int WarehouseId { get; set; }
        [ValidateNever]
        public Warehouse Warehouse { get; set; }

        // *** تمت إضافة هذا الحقل ***
        [Required]
        [Display(Name = "نوع الصنف")]
        public InventoryItemType ItemType { get; set; }

        // *** تم تعديل هذا الحقل ليصبح اختيارياً ***
        [Display(Name = "المادة الخام")]
        public int? MaterialId { get; set; } // Nullable, as it might be a finished product
        [ValidateNever]
        public Material? Material { get; set; }

        // *** تمت إضافة هذا الحقل ***
        [Display(Name = "المنتج النهائي")]
        public int? ProductId { get; set; } // Nullable, as it might be a raw material
        [ValidateNever]
        public Product? Product { get; set; }

        [Required]
        [Display(Name = "الرصيد الحالي")]
        [Range(0, double.MaxValue, ErrorMessage = "لا يمكن أن يكون الرصيد سالباً")]
        public double StockLevel { get; set; }
    }
}
