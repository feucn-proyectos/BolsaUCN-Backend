using backend.src.Application.DTOs.AuthDTOs;
using backend.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs;
using backend.src.Application.DTOs.BaseResponse;
using backend.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.src.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IUserService _service;

        public AuthController(IUserService userService)
        {
            _service = userService;
        }

        [HttpPost("register/student")]
        public async Task<IActionResult> Register([FromBody] RegisterStudentDTO registerStudentDTO)
        {
            Log.Information(
                "Intentando registrar estudiante con email: {Email}",
                registerStudentDTO.Email
            );
            var message = await _service.RegisterStudentAsync(registerStudentDTO);
            return Ok(new GenericResponse<string>("Registro de estudiante exitoso", message));
        }

        [HttpPost("register/individual")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterIndividualDTO registerIndividualDTO
        )
        {
            Log.Information(
                "Intentando registrar particular con email: {Email}",
                registerIndividualDTO.Email
            );
            var message = await _service.RegisterIndividualAsync(registerIndividualDTO);
            return Ok(new GenericResponse<string>("Registro de particular exitoso", message));
        }

        [HttpPost("register/company")]
        public async Task<IActionResult> Register([FromBody] RegisterCompanyDTO registerCompanyDTO)
        {
            Log.Information(
                "Intentando registrar empresa con email: {Email}",
                registerCompanyDTO.Email
            );
            var message = await _service.RegisterCompanyAsync(registerCompanyDTO);
            return Ok(new GenericResponse<string>("Registro de empresa exitoso", message));
        }

        [HttpPost("register/admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Register([FromBody] RegisterAdminDTO registerAdminDTO)
        {
            var adminId = GetUserIdFromToken();
            Log.Information(
                "Intentando registrar admin con email: {Email}",
                registerAdminDTO.Email
            );
            var message = await _service.RegisterAdminAsync(adminId, registerAdminDTO);
            return Ok(new GenericResponse<string>("Registro de admin exitoso", message));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            Log.Information("Intentando verificar email: {Email}", verifyEmailDTO.Email);
            var message = await _service.VerifyEmailAsync(verifyEmailDTO);
            return Ok(new GenericResponse<string>("Verificación de email exitosa", message));
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail(
            [FromBody] ResendVerificationDTO resendVerificationDTO
        )
        {
            Log.Information(
                "Intentando reenviar código de verificación al email: {Email}",
                resendVerificationDTO.Email
            );
            var message = await _service.ResendVerificationEmailAsync(resendVerificationDTO);
            return Ok(new GenericResponse<string>("Código de verificación reenviado", message));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            Log.Information("Intentando hacer login para: {Email}", loginDTO.Email);
            var token = await _service.LoginAsync(loginDTO);
            return Ok(new GenericResponse<string>("Login exitoso", token));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> SendResetPasswordVerificationCodeEmail(
            [FromBody] RequestResetPasswordCodeDTO requestResetPasswordCodeDTO
        )
        {
            Log.Information(
                "Intentando hacer reseteo de contraseña para: {Email}",
                requestResetPasswordCodeDTO.Email
            );
            var message = await _service.SendResetPasswordVerificationCodeEmailAsync(
                requestResetPasswordCodeDTO
            );
            return Ok(
                new GenericResponse<string>("Correo de reseteo de contraseña enviado", message)
            );
        }

        [HttpPost("reset-password/verify")]
        public async Task<IActionResult> VerifyResetPasswordCode(
            [FromBody] VerifyResetPasswordCodeDTO verifyResetPasswordCodeDTO
        )
        {
            Log.Information(
                "Intentando verificar código de reseteo de contraseña para: {Email}",
                verifyResetPasswordCodeDTO.Email
            );
            var message = await _service.VerifyResetPasswordCodeAsync(verifyResetPasswordCodeDTO);
            return Ok(
                new GenericResponse<string>(
                    "Verificación de código de reseteo de contraseña exitosa",
                    message
                )
            );
        }
    }
}
