// Keswa/Enums/BatchStatus.cs

using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum BatchStatus
    {
        [Display(Name = "جاهز للتحويل")]
        PendingTransfer,

        // -- تعديل: تمت إضافة هذه الحالة الضرورية لتصحيح المنطق --
        [Display(Name = "قيد التنفيذ")]
        InProgress,

        [Display(Name = "تم التحويل")]
        Transferred ,
        [Display(Name = "مكتمل")]
        Completed


    }
}