using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Keswa.Models
{
    public class SalesOrderDetail
    {
        public int Id { get; set; }

        public int SalesOrderId { get; set; }

        [ForeignKey("SalesOrderId")]
        [ValidateNever]
        public SalesOrder SalesOrder { get; set; }

        [Required(ErrorMessage = "يجب اختيار الموديل.")]
        [Display(Name = "الموديل")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [Required(ErrorMessage = "يجب إدخال الكمية.")]
        [Display(Name = "الكمية المطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية أكبر من صفر.")]
        public int Quantity { get; set; }

        [Display(Name = "حالة البند")]
        public Enums.SalesOrderDetailStatus Status { get; set; }

        public int? WorkOrderId { get; set; }

        [ForeignKey("WorkOrderId")]
        [ValidateNever]
        public WorkOrder? WorkOrder { get; set; }
    }
}
