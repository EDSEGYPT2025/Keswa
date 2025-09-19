using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class WorkOrderRouting
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        public WorkOrder WorkOrder { get; set; }

        [Required]
        [Display(Name = "القسم")]
        public Department Department { get; set; }

        [Display(Name = "الكمية المستلمة")]
        public int QuantityIn { get; set; } = 0;

        [Display(Name = "الكمية المنجزة")]
        public int QuantityOut { get; set; } = 0;

        [Display(Name = "تاريخ البدء")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاريخ الانتهاء")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // --- تمت إضافة هذا الحقل ---
        [Display(Name = "حالة المرحلة")]
        public WorkOrderStageStatus Status { get; set; } = WorkOrderStageStatus.Pending;
    }
}
