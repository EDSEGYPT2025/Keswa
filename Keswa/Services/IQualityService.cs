using System.Threading.Tasks;
using Keswa.Models;

namespace Keswa.Services
{
    public interface IQualityService
    {
        Task TransferProductToQualityAsync(int productId, int quantity);
        Task AssignTaskToWorkerAsync(int inspectionId, string workerId);
        Task ReceiveInspectionResultAsync(int inspectionId, int quantityGradeA, int quantityGradeB);
    }
}
