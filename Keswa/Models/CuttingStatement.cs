using Keswa.Enums;
using Keswa.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class CuttingStatement
    {
        public int Id { get; set; }

        [Required]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        public WorkOrder WorkOrder { get; set; }

        [Required(ErrorMessage = "يجب اختيار الخامة")]
        [Display(Name = "الخامة")]
        public int MaterialId { get; set; }
        [ValidateNever]
        public Material Material { get; set; }

        [Required(ErrorMessage = "يجب إدخال المتراج")]
        [Display(Name = "المتراج")]
        public double Meterage { get; set; }

        [Display(Name = "رقم التشغيل")]
        public string? RunNumber { get; set; }

        [Required(ErrorMessage = "يجب اختيار الموديل")]
        [Display(Name = "الموديل")]
        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }

        [Required(ErrorMessage = "يجب إدخال اللون")]
        [Display(Name = "اللون")]
        public string Color { get; set; }

        [Required(ErrorMessage = "يجب إدخال العدد")]
        [Display(Name = "العدد")]
        [Range(1, int.MaxValue)]
        public int Count { get; set; }

        [Display(Name = "الكمية المنصرفة")]
        [Range(0, double.MaxValue, ErrorMessage = "الكمية المنصرفة يجب أن تكون رقمًا موجبًا")]
        public double IssuedQuantity { get; set; }   // 👈 الإضافة الجديدة

        [Required(ErrorMessage = "يجب اختيار العامل")]
        [Display(Name = "اسم العامل")]
        public int WorkerId { get; set; }
        [ValidateNever]
        public Worker Worker { get; set; }

        [Required(ErrorMessage = "يجب اختيار العميل")]
        [Display(Name = "اسم العميل")]
        public int CustomerId { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; }

        //public DateTime StatementDate { get; set; } = DateTime.Now;
        public DateTime StatementDate { get; set; } = DateTimeHelper.EgyptNow;

        // أضف هذا السطر داخل كلاس CuttingStatement
        [Display(Name = "حالة التشغيلة")]
        public BatchStatus Status { get; set; } = BatchStatus.PendingTransfer;
    }
}

