using System.Security.Claims;
using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.DTOs.UserDTOs;
using backend.src.Application.DTOs.UserDTOs.UserProfileDTOs;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado.
        /// </summary>
        /// <returns>Datos del perfil del usuario.</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            int parsedUserId = GetUserIdFromToken();

            var result = await _userService.GetUserProfileByIdAsync(parsedUserId);
            return Ok(new GenericResponse<GetUserProfileDTO>("Datos de perfil obtenidos.", result));
        }

        /// <summary>
        /// Actualiza el perfil del usuario autenticado.
        /// </summary>
        /// <param name="updateParamsDTO">Parámetros para actualizar el perfil del usuario.</param>
        /// <returns>Respuesta genérica indicando el resultado de la operación.</returns>
        [HttpPatch("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(
            [FromForm] UpdateUserProfileDTO updateParamsDTO
        )
        {
            var (userId, userType) = GetIdAndTypeFromToken();
            var result = await _userService.UpdateUserProfileByIdAsync(
                updateParamsDTO,
                userId,
                userType
            );
            return Ok(new GenericResponse<string>("Perfil actualizado", result));
        }

        /// <summary>
        /// Obtiene la foto de perfil del usuario autenticado.
        /// </summary>
        /// <returns>URL de la foto de perfil del usuario.</returns>
        [HttpGet("profile/photo")]
        [Authorize]
        public async Task<IActionResult> GetProfilePhoto()
        {
            int userId = GetUserIdFromToken();
            var result = await _userService.GetUserProfilePhotoByIdAsync(userId);
            return Ok(new GenericResponse<GetPhotoDTO>("Foto de perfil obtenida", result));
        }

        /// <summary>
        /// Actualiza la foto de perfil del usuario autenticado.
        /// </summary>
        /// <param name="updatePhotoDTO">Parámetros para actualizar la foto de perfil.</param>
        /// <returns>Respuesta genérica indicando el resultado de la operación.</returns>
        [HttpPatch("profile/photo")]
        [Authorize]
        public async Task<IActionResult> UpdateProfilePhoto(
            [FromForm] UpdatePhotoDTO updatePhotoDTO
        )
        {
            int userId = GetUserIdFromToken();
            var result = await _userService.UpdateUserProfilePhotoByIdAsync(updatePhotoDTO, userId);
            return Ok(new GenericResponse<string>("Foto de perfil actualizada", result));
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado.
        /// </summary>
        /// <param name="changeUserPasswordDTO">Parámetros para cambiar la contraseña del usuario.</param>
        /// <returns>Respuesta genérica indicando el resultado de la operación.</returns>
        [HttpPatch("profile/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangeUserPassword(
            [FromBody] ChangeUserPasswordDTO changeUserPasswordDTO
        )
        {
            int userId = GetUserIdFromToken();
            var result = await _userService.ChangeUserPasswordById(changeUserPasswordDTO, userId);
            return Ok(new GenericResponse<string>("Contraseña actualizada", result));
        }

        /// <summary>
        /// Sube el CV del usuario autenticado.
        /// </summary>
        /// <param name="updateCVDTO">Parámetros para subir el CV del usuario.</param>
        /// <returns>Respuesta genérica indicando el resultado de la operación.</returns>
        [HttpPatch("cv")]
        [Authorize]
        public async Task<IActionResult> UploadCV([FromForm] UploadCVDTO updateCVDTO)
        {
            int userId = GetUserIdFromToken();
            var result = await _userService.UploadCVByIdAsync(updateCVDTO, userId);
            return Ok(new GenericResponse<string>("CV actualizado", result));
        }

        [HttpGet("cv")]
        [Authorize]
        public async Task<IActionResult> GetCV()
        {
            int userId = GetUserIdFromToken();
            var result = await _userService.DownloadCVByIdAsync(userId);
            return Ok(new GenericResponse<GetCVDTO>("CV obtenido", result));
        }
    }
}
