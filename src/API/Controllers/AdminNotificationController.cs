using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminNotificationController : ControllerBase
    {
        private readonly IAdminNotificationRepository _adminNotificationRepository;

        public AdminNotificationController(IAdminNotificationRepository adminNotificationRepository)
        {
            _adminNotificationRepository = adminNotificationRepository;
        }

        /// <summary>
        /// Obtiene todas las notificaciones administrativas.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _adminNotificationRepository.GetAllAsync();
            return Ok(notifications);
        }

        /// <summary>
        /// Marca una notificación como leída.
        /// </summary>
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _adminNotificationRepository.GetByIdAsync(id);
            if (notification == null)
                return NotFound($"No se encontró una notificación con ID {id}.");

            notification.IsRead = true;
            await _adminNotificationRepository.UpdateAsync(notification);

            return Ok("Notificación marcada como leída.");
        }
    }
}
