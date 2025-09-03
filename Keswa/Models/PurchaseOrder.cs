using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        // *** تمت إضافة هذا الحقل ***
        [Display(Name = "رقم أمر الشراء")]
        public string? OrderNumber { get; set; }


        [Display(Name = "تاريخ الطلب")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Display(Name = "الحالة")]
        public PurchaseOrderStatus Status { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // *** تمت إضافة هذه العلاقة ***
        [Display(Name = "مرتبط بأمر الشغل")]
        public int? WorkOrderId { get; set; }
        [ValidateNever]
        [ForeignKey("WorkOrderId")]
        public WorkOrder? WorkOrder { get; set; }


        [ValidateNever]
        public ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    }
}