using Keswa.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Models
{
    public class MaterialReturnNote
    {
        public int Id { get; set; }

        [Display(Name = "رقم السند")]
        public string ReturnNoteNumber { get; set; }

        [Display(Name = "أمر الشغل")]
        public int WorkOrderId { get; set; }
        [ValidateNever]
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "القسم")]
        public Department Department { get; set; }

        [Display(Name = "تاريخ الإرجاع")]
        public DateTime ReturnDate { get; set; }

        [Display(Name = "الحالة")]
        public RequisitionStatus Status { get; set; } // سنعيد استخدام نفس الـ Enum (Pending, Approved, Rejected)

        public string? Notes { get; set; }

        [ValidateNever]
        public ICollection<MaterialReturnNoteDetail> Details { get; set; } = new List<MaterialReturnNoteDetail>();
    }
}