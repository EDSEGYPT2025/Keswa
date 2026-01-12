namespace Keswa.Enums
{
    public enum PackagingBatchStatus
    {
        Pending,        // بانتظار التوزيع
        InProgress,     // قيد التشغيل
        Completed,      // مكتملة
        TransferredToStore // تم توريدها للمخزن النهائي
    }
}