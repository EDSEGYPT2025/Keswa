using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class MaterialRequisition
    {
        public int Id { get; set; }

        [Required]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "تاريخ الطلب")]
        public System.DateTime RequestDate { get; set; } = System.DateTime.Today;

        [Display(Name = "الحالة")]
        public RequisitionStatus Status { get; set; } = RequisitionStatus.Pending;

        [Display(Name = "ملاحظات الطلب")]
        public string? Notes { get; set; }

        [ValidateNever]
        public List<MaterialRequisitionDetail> Details { get; set; } = new List<MaterialRequisitionDetail>();
    }
}
