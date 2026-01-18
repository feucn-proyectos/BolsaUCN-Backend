using backend.src.Application.Events;
using backend.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Test endpoint that simulates a postulation status change and triggers notification handling.
        /// </summary>
        /// <param name="evt">Event payload describing the status change.</param>
        /// <returns>HTTP 200 OK when processing completes.</returns>
        [HttpPost("test-status-change")]
        public async Task<IActionResult> TestStatusChange(
            [FromBody] PostulationStatusChangedEvent evt
        )
        {
            await _notificationService.SendPostulationStatusChangeAsync(evt);
            return Ok(new { message = "Notificación procesada correctamente." });
        }
    }
}
