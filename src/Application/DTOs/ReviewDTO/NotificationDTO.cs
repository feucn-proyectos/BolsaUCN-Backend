/// <summary>
/// Data Transfer Object representing a user notification.
/// Used to transport notification data between layers of the application.
/// </summary>
public class NotificationDTO
{
    /// <summary>
    /// Notification identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Recipient user email address.
    /// </summary>
    public required string UserEmail { get; set; }

    /// <summary>
    /// Notification message content.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Timestamp when the notification was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Flag indicating whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; } = false;
}
