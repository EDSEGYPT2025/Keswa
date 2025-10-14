using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum BatchStatus
    {
        [Display(Name = "جاهز للتحويل")]
        PendingTransfer,

        [Display(Name = "تم التحويل")]
        Transferred
    }
}