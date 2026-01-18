using backend.src.Application.Events;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Service responsible for handling notifications: persistence and dispatch.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Processes a postulation status change event by creating a notification and sending any required emails.
        /// </summary>
        /// <param name="evt">Event describing the postulation status change.</param>
        Task SendPostulationStatusChangeAsync(PostulationStatusChangedEvent evt);
    }
}
