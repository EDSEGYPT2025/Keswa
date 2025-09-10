using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum RequisitionStatus
    {
        [Display(Name = "معلق")]
        Pending,

        [Display(Name = "تم الصرف")]
        Processed
    }
}
