namespace WorkScheduleApp.Models
{
    public class ErrorSharedView
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}