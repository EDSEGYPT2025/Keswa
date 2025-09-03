using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class ProductionLog
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }

        [ValidateNever]
        public WorkOrder WorkOrder { get; set; }

        [Required]
        [Display(Name = "العامل")]
        public int WorkerId { get; set; }

        [ValidateNever]
        public Worker Worker { get; set; }

        [Required]
        [Display(Name = "القسم")]
        public Department Department { get; set; }

        [Required(ErrorMessage = "يجب إدخال الكمية المنتجة")]
        [Display(Name = "الكمية المنتجة")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر")]
        public int QuantityProduced { get; set; }

        // *** تم التعديل هنا ***
        [Display(Name = "تاريخ ووقت التسجيل")]
        public DateTime LogDate { get; set; } = DateTime.Now;

    }
}
