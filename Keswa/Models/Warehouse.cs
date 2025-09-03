using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المخزن مطلوب")]
        [Display(Name = "اسم المخزن")]
        public string Name { get; set; }

        [Display(Name = "الموقع")]
        public string? Location { get; set; }

        // *** تمت إضافة هذا الحقل ***
        [Required]
        [Display(Name = "نوع المخزن")]
        public WarehouseType Type { get; set; }

        [ValidateNever]
        public List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}
