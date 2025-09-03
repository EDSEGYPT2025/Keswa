using Keswa.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class ProductionReceiptLog
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        public WorkOrder WorkOrder { get; set; }

        [Required]
        [Display(Name = "المنتج")]
        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }

        [Required]
        [Display(Name = "المخزن")]
        public int WarehouseId { get; set; }
        [ValidateNever]
        public Warehouse Warehouse { get; set; }

        [Required]
        [Display(Name = "الكمية المستلمة")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "درجة الجودة")]
        public string QualityGrade { get; set; }

        // *** تم إضافة هذا الحقل ***
        [Required]
        [Display(Name = "تكلفة الوحدة")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitCost { get; set; }

        [Display(Name = "تاريخ الاستلام")]
        public DateTime ReceiptDate { get; set; } = DateTime.Now;
    }
}
