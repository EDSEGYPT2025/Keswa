using System.ComponentModel.DataAnnotations;

namespace Keswa.Enums;

public enum UnitOfMeasure
{
    [Display(Name = "متر")]
    Meter,

    [Display(Name = "كيلو")]
    Kilogram,

    [Display(Name = "قطعة")]
    Piece,

    [Display(Name = "بكرة")]
    Spool,

    [Display(Name = "دستة")]
    Dozen,

    [Display(Name = "فرخ")]
    Sheet
}