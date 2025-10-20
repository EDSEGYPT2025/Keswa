using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum BatchStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending,

        [Display(Name = "جاهز للتحويل")]
        PendingTransfer,

        [Display(Name = "تم التحويل")]
        Transferred,

        [Display(Name = "مكتمل")]
        Completed,

        // --- تمت إضافة هذه الحالة ---
        [Display(Name = "مؤرشف")]
        Archived
    }
}
