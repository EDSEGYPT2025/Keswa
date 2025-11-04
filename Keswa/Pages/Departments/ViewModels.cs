using System;
using System.ComponentModel.DataAnnotations;

namespace Keswa.Pages.Departments
{
    public class ReadyForTransferViewModel
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public int ReadyQuantity { get; set; }
        public string WorkerNames { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public class QualityAssignmentViewModel
    {
        // For Index page display
        public int AssignmentId { get; set; }
        public string WorkerName { get; set; }
        public string BatchNumber { get; set; }
        public int RemainingQuantity { get; set; }

        // For Assign page form binding
        [Required]
        public int WorkerId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "الكمية المسلمة")]
        public int AssignedQuantity { get; set; }
    }
}
