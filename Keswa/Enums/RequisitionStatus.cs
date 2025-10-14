using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum RequisitionStatus
    {
        [Display(Name = "معلق")]
        Pending,

        [Display(Name = "تمت الموافقة")] // <-- تم التعديل
        Approved,

        [Display(Name = "مرفوض")] // <-- تم الإضافة
        Rejected,

        [Display(Name = "تم الصرف")]
        Processed
    }
}