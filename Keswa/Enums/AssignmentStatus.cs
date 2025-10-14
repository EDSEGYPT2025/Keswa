using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum AssignmentStatus
    {
        [Display(Name = "قيد التنفيذ لدى العامل")]
        InProgress,

        [Display(Name = "تم التسليم بالكامل")]
        Completed
    }
}