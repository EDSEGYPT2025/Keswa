using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic; // <-- إضافة مهمة
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Keswa.Models
{
    public class MaterialIssuanceNote
    {
        public int Id { get; set; }

        [Display(Name = "رقم الإذن")]
        public string? IssuanceNumber { get; set; }

        [Required]
        [Display(Name = "طلب الصرف")]
        public int MaterialRequisitionId { get; set; }

        [ForeignKey("MaterialRequisitionId")]
        public MaterialRequisition MaterialRequisition { get; set; }

        [Required]
        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        public WorkOrder WorkOrder { get; set; }

        [Required]
        [Display(Name = "المخزن")]
        public int WarehouseId { get; set; }
        [ValidateNever]
        public Warehouse Warehouse { get; set; }

        [Display(Name = "تاريخ الصرف")]
        [DataType(DataType.Date)]
        public DateTime IssuanceDate { get; set; } = DateTime.Today;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [ValidateNever]
        // *** تم التعديل هنا: تغيير النوع من ICollection إلى List ***
        public List<MaterialIssuanceNoteDetail> Details { get; set; } = new List<MaterialIssuanceNoteDetail>();
    }
}
