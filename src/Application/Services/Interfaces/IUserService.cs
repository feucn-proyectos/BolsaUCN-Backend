using backend.src.Application.DTOs.AuthDTOs;
using backend.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs;
using backend.src.Application.DTOs.UserDTOs;
using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Application.DTOs.UserDTOs.UserProfileDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;

namespace backend.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        #region Registro de usuarios

        /// <summary>
        /// Funcion de registro de estudiante
        /// </summary>
        /// <param name="registerStudentDTO">Datos para registrar un estudiante</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterStudentAsync(RegisterStudentDTO registerStudentDTO);

        /// <summary>
        /// Funcion de registro de persona particular
        /// </summary>
        /// <param name="registerIndividualDTO">Datos para registrar una persona particular</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterIndividualAsync(RegisterIndividualDTO registerIndividualDTO);

        /// <summary>
        /// Funcion de registro de empresa
        /// </summary>
        /// <param name="registerCompanyDTO">Datos para registrar una empresa</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterCompanyAsync(RegisterCompanyDTO registerCompanyDTO);

        /// <summary>
        /// Funcion de registro de administrador
        /// </summary>
        /// <param name="registerAdminDTO">Datos para registrar un administrador</param>
        /// <returns>Mensaje de resultado del registro</returns>
        Task<string> RegisterAdminAsync(int adminId, RegisterAdminDTO registerAdminDTO);

        /// <summary>
        /// Verifica el correo electrónico de un usuario.
        /// </summary>
        /// <param name="verifyEmailDTO">Datos para verificar el correo electrónico</param>
        /// <returns>Mensaje de resultado de la verificación</returns>
        Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO);

        /// <summary>
        /// Reenvía el correo de verificación a un usuario.
        /// </summary>
        /// <param name="resendVerificationDTO">Datos para reenviar el correo de verificación</param>
        /// <returns>Mensaje de resultado del reenvío</returns>
        Task<string> ResendVerificationEmailAsync(ResendVerificationDTO resendVerificationDTO);

        /// <summary>
        /// Cambia el correo electrónico de un usuario por su ID.
        /// </summary>
        /// <param name="changeUserEmailDTO">Datos para cambiar el correo electrónico del usuario</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado del cambio de correo electrónico</returns>
        Task<string> ChangeUserEmailByIdAsync(ChangeUserEmailDTO changeUserEmailDTO, int userId);

        /// <summary>
        /// Reenvía el correo de verificación del nuevo correo electrónico de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado del reenvío</returns>
        Task<string> ResendChangeEmailVerificationByIdAsync(int userId);

        /// <summary>
        /// Verifica el nuevo correo electrónico de un usuario por su ID.
        /// </summary>
        /// <param name="verifyNewEmailDTO">Datos para verificar el nuevo correo electrónico del usuario</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado de la verificación</returns>
        Task<string> VerifyNewEmailByIdAsync(VerifyNewEmailDTO verifyNewEmailDTO, int userId);
        #endregion

        #region Login and Password Management
        /// <summary>
        /// Inicia sesión de un usuario.
        /// </summary>
        /// <param name="loginDTO">Datos para iniciar sesión</param>
        /// <returns>Mensaje de resultado del inicio de sesión</returns>
        Task<string> LoginAsync(LoginDTO loginDTO);

        /// <summary>
        /// Envía un correo electrónico con un código de verificación para restablecer la contraseña.
        /// </summary>
        /// <param name="requestResetPasswordCodeDTO">Datos para solicitar el código de verificación</param>
        /// <returns>Mensaje de resultado del envío</returns>
        Task<string> SendResetPasswordVerificationCodeEmailAsync(
            RequestResetPasswordCodeDTO requestResetPasswordCodeDTO
        );

        /// <summary>
        /// Verifica el código de restablecimiento de contraseña.
        /// </summary>
        /// <param name="verifyResetPasswordCodeDTO">Datos para verificar el código de restablecimiento</param>
        /// <returns>Mensaje de resultado de la verificación</returns>
        Task<string> VerifyResetPasswordCodeAsync(
            VerifyResetPasswordCodeDTO verifyResetPasswordCodeDTO
        );

        /// <summary>
        /// Restablece la contraseña de un usuario.
        /// </summary>
        /// <param name="changeUserPasswordDTO">Datos para cambiar la contraseña del usuario</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado del cambio de contraseña</returns>
        Task<string> ChangeUserPasswordById(
            ChangeUserPasswordDTO changeUserPasswordDTO,
            int userId
        );
        #endregion
        #region Profile Management
        /// <summary>
        /// Obtiene el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Perfil del usuario</returns>
        Task<GetUserProfileDTO> GetUserProfileByIdAsync(int userId);

        /// <summary>
        /// Actualiza el perfil de un usuario por su ID.
        /// </summary>
        /// <param name="updateParamsDTO">Datos para actualizar el perfil del usuario</param>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Mensaje de resultado de la actualización</returns>
        Task<string> UpdateUserProfileByIdAsync(
            UpdateUserProfileDTO updateParamsDTO,
            int userId,
            UserType userType
        );

        /// <summary>
        /// Obtiene la foto de perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Foto de perfil del usuario</returns>
        Task<GetPhotoDTO> GetUserProfilePhotoByIdAsync(int userId);

        /// <summary>
        /// Actualiza la foto de perfil de un usuario por su ID.
        /// </summary>
        /// <param name="updatePhotoDTO">Datos para actualizar la foto de perfil</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado de la actualización</returns>
        Task<string> UpdateUserProfilePhotoByIdAsync(UpdatePhotoDTO updatePhotoDTO, int userId);
        #endregion
        #region Documents Management
        /// <summary>
        /// Sube el CV de un usuario por su ID.
        /// </summary>
        /// <param name="uploadCVDTO">Datos para subir el CV</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de resultado de la subida</returns>
        Task<string> UploadCVByIdAsync(UploadCVDTO uploadCVDTO, int userId);

        /// <summary>
        /// Descarga el CV de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Archivo del CV</returns>
        Task<HasCVDTO> CheckCVExistsByIdAsync(int userId);

        /// <summary>
        /// Descarga el CV de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Archivo del CV</returns>
        Task<(MemoryStream FileStream, string FileName, string ContentType)> DownloadCVByIdAsync(
            int userId
        );

        /// <summary>
        /// Elimina el CV de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Indica si la eliminación fue exitosa</returns>
        Task<string> DeleteCVByIdAsync(int userId);
        #endregion

        #region Background Jobs

        /// <summary>
        /// Elimina las cuentas de usuario que no han sido confirmadas dentro de un cierto período de tiempo.
        /// </summary>
        Task DeleteUnconfirmedUserAccountsAsync();

        /// <summary>
        /// Elimina las solicitudes de cambio de correo electrónico pendientes que han expirado según la configuración de tiempo de expiración.
        /// </summary>
        Task ClearExpiredPendingEmailChangeRequestAsync(int userId);
        #endregion
    }
}
