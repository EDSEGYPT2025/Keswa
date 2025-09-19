using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum WorkOrderStageStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending,

        [Display(Name = "قيد التنفيذ")]
        InProgress,

        [Display(Name = "مكتمل")]
        Completed
    }
}
