using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums;

public enum SalesOrderStatus
{
    [Display(Name = "جديدة")]
    New,

    [Display(Name = "جاري تنفيذها")]
    InProgress,

    [Display(Name = "مكتملة")]
    Completed
}
