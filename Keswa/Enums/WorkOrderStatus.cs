using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum WorkOrderStatus
    {
        [Display(Name = "جديد")]
        New,

        [Display(Name = "قيد التنفيذ")]
        InProgress,

        [Display(Name = "مكتمل")]
        Completed,

        [Display(Name = "ملغي")]
        Cancelled
    }
}
