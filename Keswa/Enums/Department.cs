using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums
{
    public enum Department
    {
        [Display(Name = "القص")]
        Cutting,

        [Display(Name = "الحياكة")]
        Sewing,

        [Display(Name = "التشطيب")]
        Finishing,

        [Display(Name = "الجودة")]
        QualityControl,

        [Display(Name = "المكواة")]
        Ironing,

        [Display(Name = "التعبئة")]
        Packaging
    }
}
