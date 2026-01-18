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
        #region Métodos auxiliares

        /// <summary>
        /// Obtiene un usuario por su ID con opciones de consulta.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="options">
        /// Opciones de consulta para incluir relaciones y controlar el seguimiento de cambios.
        /// Si es <c>null</c>, se utilizan las opciones predeterminadas.
        /// <list type="bullet">
        /// <item><description><see cref="UserQueryOptions.TrackChanges"/>: Indica si se debe realizar un seguimiento de los cambios en la entidad.</description></item>
        /// <item><description><see cref="UserQueryOptions.IncludePhoto"/>: Indica si se debe incluir la foto del usuario.</description></item>
        /// <item><description><see cref="UserQueryOptions.IncludeCV"/>: Indica si se debe incluir el CV del usuario.</description></item>
        /// <item><description><see cref="UserQueryOptions.IncludeApplications"/>: Indica si se deben incluir las aplicaciones del usuario.</description></item>
        /// <item><description><see cref="UserQueryOptions.IncludePublications"/>: Indica si se deben incluir las publicaciones del usuario.</description></item>
        /// </list>
        /// </param>
        /// <returns>El usuario encontrado o lanza una excepción si no existe</returns>
        /// <exception cref="KeyNotFoundException">Si el usuario no existe</exception>
        Task<User> GetUserByIdAsync(int userId, UserQueryOptions? options = null);

        /// <summary>
        /// Verifica si un usuario tiene un rol específico.
        /// </summary>
        /// <param name="user">El usuario a verificar</param>
        /// <param name="role">El nombre del rol a verificar</param>
        /// <returns>True si el usuario tiene el rol, de lo contrario False</returns>
        Task<bool> HasRoleAsync(User user, string role);

        /// <summary>
        /// Obtiene el número de usuarios por tipo.
        /// </summary>
        /// <param name="userType">El tipo de usuario</param>
        /// <returns>El número de usuarios del tipo especificado</returns>
        Task<int> GetNumberOfUsersByTypeAsync(UserType userType);

        /// <summary>
        /// Actualiza un usuario en el sistema.
        /// </summary>
        /// <param name="user">El usuario a actualizar</param>
        /// <returns>True si la actualización fue exitosa, de lo contrario False</returns>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Obtiene usuarios filtrados para un administrador.
        /// </summary>
        /// <param name="adminId">ID del administrador</param>
        /// <param name="searchParams">Parámetros de búsqueda para filtrar usuarios</param>
        /// <returns>Una tupla que contiene la lista de usuarios y el conteo total</returns>
        Task<(IEnumerable<User>, int TotalCount)> GetFilteredForAdminAsync(
            int adminId,
            SearchParamsDTO searchParams
        );

        #endregion

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
        Task<GetCVDTO> DownloadCVByIdAsync(int userId);

        /// <summary>
        /// Elimina el CV de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Indica si la eliminación fue exitosa</returns>
        Task<string> DeleteCVByIdAsync(int userId);
        #endregion
    }
}
