using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum AssignmentType
    {
        [Display(Name = "داخلي (داخل المصنع)")]
        Internal,

        [Display(Name = "خارجي (خارج المصنع)")]
        External
    }
}