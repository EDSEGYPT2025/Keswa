using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }

        [Display(Name = "رقم أمر الشغل")]
        public string? WorkOrderNumber { get; set; } // <-- تم التعديل هنا

        [Required(ErrorMessage = "يجب اختيار الموديل")]
        [Display(Name = "الموديل")]
        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Display(Name = "الكمية المطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر")]
        public int QuantityToProduce { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "تاريخ البدء المخطط")]
        [DataType(DataType.Date)]
        public DateTime? PlannedStartDate { get; set; }

        [Display(Name = "تاريخ الانتهاء المخطط")]
        [DataType(DataType.Date)]
        public DateTime? PlannedCompletionDate { get; set; }

        [Display(Name = "الحالة")]
        public WorkOrderStatus Status { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // أضف هذا السطر داخل كلاس WorkOrder
        public ProductionLine? ProductionLine { get; set; }
    }
}
