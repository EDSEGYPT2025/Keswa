using System;

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
}
