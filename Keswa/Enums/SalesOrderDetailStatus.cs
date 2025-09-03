using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums;

public enum SalesOrderDetailStatus
{
    [Display(Name = "بانتظار التحويل")]
    PendingConversion,

    [Display(Name = "تم التحويل للإنتاج")]
    ConvertedToWorkOrder
}
