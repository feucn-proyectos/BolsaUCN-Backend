namespace backend.src.Application.Jobs.Interfaces
{
    public interface INotificationJobs
    {
        Task SendUserDailyNotificationsAsync();
        Task SendAdminDailyNotificationsAsync();
    }
}
