using System.Security;
using backend.src.Application.DTOs.AuthDTOs;
using backend.src.Application.DTOs.AuthDTOs.ResetPasswordDTOs;
using backend.src.Application.DTOs.UserDTOs;
using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Application.DTOs.UserDTOs.UserProfileDTOs;
using backend.src.Application.Jobs.Interfaces;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Infrastructure.Exceptions;
using backend.src.Infrastructure.Repositories.Interfaces;
using CloudinaryDotNet;
using Hangfire;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IOfferApplicationRepository _applicationRepository;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IFileService _fileService;
        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly int _daysOfUnconfirmedUserRetention;
        private readonly int _daysOfUnconfirmedPendingEmailChangeRetention;

        public UserService(
            IConfiguration configuration,
            IUserRepository userRepository,
            IFileRepository fileRepository,
            IOfferApplicationRepository applicationRepository,
            IVerificationCodeRepository verificationCodeRepository,
            IEmailService emailService,
            ITokenService tokenService,
            IFileService fileService
        )
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _fileRepository = fileRepository;
            _applicationRepository = applicationRepository;
            _emailService = emailService;
            _verificationCodeRepository = verificationCodeRepository;
            _tokenService = tokenService;
            _fileService = fileService;
            _daysOfUnconfirmedUserRetention = _configuration.GetValue<int>(
                "JobsConfiguration:DaysOfUnconfirmedUserRetention"
            );
            _daysOfUnconfirmedPendingEmailChangeRetention = _configuration.GetValue<int>(
                "JobsConfiguration:DaysOfUnconfirmedPendingEmailChangeRetention"
            );
        }

        /// <summary>
        /// Registra un nuevo estudiante en el sistema.
        /// </summary>
        /// <param name="registerStudentDTO">DTO con la información del estudiante</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterStudentAsync(RegisterStudentDTO registerStudentDTO)
        {
            Log.Information(
                "Iniciando registro de estudiante con email: {Email}",
                registerStudentDTO.Email
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerStudentDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro con email duplicado: {Email}",
                    registerStudentDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerStudentDTO.Rut);
            if (registrado)
            {
                Log.Warning("Intento de registro con RUT duplicado: {Rut}", registerStudentDTO.Rut);
                throw new InvalidOperationException("El RUT ya está en uso.");
            }

            var user = registerStudentDTO.Adapt<User>();
            user.PhoneNumber = NormalizePhoneNumber(registerStudentDTO.PhoneNumber);

            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
            };
            await _fileRepository.CreateUserImageAsync(profile);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;

            var result = await _userRepository.CreateUserAsync(
                user,
                registerStudentDTO.Password,
                RoleNames.Applicant
            );
            if (result == false)
            {
                Log.Error(
                    "Error al crear usuario estudiante con email: {Email}",
                    registerStudentDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }

            //Envio email de verificacion
            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(
                user.Email!,
                newCode.Code
            );
            if (emailResult)
            {
                Log.Information(
                    "Estudiante registrado exitosamente con ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Registra un nuevo usuario particular en el sistema.
        /// </summary>
        /// <param name="registerIndividualDTO">Dto de registro del usuario particular</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterIndividualAsync(
            RegisterIndividualDTO registerIndividualDTO
        )
        {
            Log.Information(
                "Iniciando registro de particular con email: {Email}",
                registerIndividualDTO.Email
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerIndividualDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro particular con email duplicado: {Email}",
                    registerIndividualDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerIndividualDTO.Rut);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro particular con RUT duplicado: {Rut}",
                    registerIndividualDTO.Rut
                );
                throw new InvalidOperationException("El RUT ya está en uso.");
            }
            var user = registerIndividualDTO.Adapt<User>();
            user.PhoneNumber = NormalizePhoneNumber(registerIndividualDTO.PhoneNumber);
            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
            };
            await _fileRepository.CreateUserImageAsync(profile);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;
            var result = await _userRepository.CreateUserAsync(
                user,
                registerIndividualDTO.Password,
                RoleNames.Offeror
            );
            if (!result)
            {
                Log.Error(
                    "Error al crear usuario particular con email: {Email}",
                    registerIndividualDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }
            //Envio email de verificacion
            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(
                user.Email!,
                newCode.Code
            );
            if (emailResult)
            {
                Log.Information(
                    "Particular registrado exitosamente con ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Registra una nueva empresa en el sistema.
        /// </summary>
        /// <param name="registerCompanyDTO">Dto de registro de la empresa</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterCompanyAsync(RegisterCompanyDTO registerCompanyDTO)
        {
            Log.Information(
                "Iniciando registro de empresa con email: {Email}",
                registerCompanyDTO.Email
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerCompanyDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de empresa con email duplicado: {Email}",
                    registerCompanyDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerCompanyDTO.Rut);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de empresa con RUT duplicado: {Rut}",
                    registerCompanyDTO.Rut
                );
                throw new InvalidOperationException("El RUT ya está en uso.");
            }
            var user = registerCompanyDTO.Adapt<User>();
            user.PhoneNumber = NormalizePhoneNumber(registerCompanyDTO.PhoneNumber);
            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
            };
            await _fileRepository.CreateUserImageAsync(profile);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;
            var result = await _userRepository.CreateUserAsync(
                user,
                registerCompanyDTO.Password,
                RoleNames.Offeror
            );
            if (!result)
            {
                Log.Error(
                    "Error al crear usuario empresa con email: {Email}",
                    registerCompanyDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }
            //Envio email de verificacion
            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(
                user.Email!,
                newCode.Code
            );
            if (emailResult)
            {
                Log.Information(
                    "Empresa registrada exitosamente con ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Registra un nuevo administrador en el sistema.
        /// </summary>
        /// <param name="registerAdminDTO">Dto de registro del administrador</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> RegisterAdminAsync(int adminId, RegisterAdminDTO registerAdminDTO)
        {
            Log.Information("Verificando permisos del admin con ID: {AdminId}", adminId);
            var requestingAdmin = await _userRepository.GetByIdAsync(adminId);
            if (requestingAdmin == null)
            {
                Log.Error("No se encontro usuario con ID: {AdminId}", adminId);
                throw new UnauthorizedAccessException("No se encontro usuario.");
            }
            var requestingAdminRoleResult = await _userRepository.CheckRoleAsync(
                requestingAdmin.Id,
                RoleNames.SuperAdmin
            );
            if (!requestingAdminRoleResult)
            {
                Log.Error(
                    "El usuario con ID: {AdminId} no tiene permisos para registrar administradores.",
                    adminId
                );
                throw new UnauthorizedAccessException("El usuario no es superadmin.");
            }
            var existingAdminsCount = await _userRepository.GetCountByTypeAsync(
                UserType.Administrador
            );
            var maxAdminsAllowed = _configuration.GetValue<int>("AdminSettings:MaxAdminsAllowed");
            if (existingAdminsCount >= maxAdminsAllowed)
            {
                Log.Error(
                    "Se ha alcanzado el número máximo de administradores permitidos: {MaxAdminsAllowed}.",
                    maxAdminsAllowed
                );
                throw new InvalidOperationException(
                    "No se pueden registrar más administradores. Se ha alcanzado el límite."
                );
            }

            Log.Information(
                "Iniciando registro de admin con email: {Email}",
                registerAdminDTO.Email
            );

            bool registrado = await _userRepository.ExistsByEmailAsync(registerAdminDTO.Email);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de admin con email duplicado: {Email}",
                    registerAdminDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            registrado = await _userRepository.ExistsByRutAsync(registerAdminDTO.Rut);
            if (registrado)
            {
                Log.Warning(
                    "Intento de registro de admin con RUT duplicado: {Rut}",
                    registerAdminDTO.Rut
                );
                throw new InvalidOperationException("El RUT ya está en uso.");
            }
            var user = registerAdminDTO.Adapt<User>();
            user.PhoneNumber = NormalizePhoneNumber(registerAdminDTO.PhoneNumber);
            var profile = new UserImage()
            {
                Url = _configuration.GetValue<string>("Images:DefaultUserImageUrl")!,
                PublicId = _configuration.GetValue<string>("Images:DefaultUserImagePublicId")!,
            };

            await _fileRepository.CreateUserImageAsync(profile);
            user.ProfilePhoto = profile;
            user.ProfilePhotoId = profile.Id;

            string role = RoleNames.Admin;

            var result = await _userRepository.CreateUserAsync(
                user,
                registerAdminDTO.Password,
                role
            );
            if (!result)
            {
                Log.Error(
                    "Error al crear usuario admin con email: {Email}",
                    registerAdminDTO.Email
                );
                throw new Exception("Error al crear el usuario.");
            }
            //Envio email de verificacion
            string code = new Random().Next(100000, 999999).ToString();
            VerificationCode verificationCode = new VerificationCode
            {
                Code = code,
                CodeType = CodeType.EmailConfirmation,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            var emailResult = await _emailService.SendVerificationEmailAsync(
                user.Email!,
                newCode.Code
            );
            if (emailResult)
            {
                Log.Information(
                    "Admin registrado exitosamente con ID: {UserId}, Email: {Email}, Rol: {Role}",
                    user.Id,
                    user.Email,
                    role
                );
                return "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico.";
            }
            return "El usuario fue registrado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Verifica el correo electrónico de un usuario.
        /// </summary>
        /// <param name="verifyEmailDTO">Dto de verificación del correo electrónico</param>
        /// <returns>Mensaje de éxito o error</returns>
        public async Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
        {
            Log.Information("Intentando verificar email: {Email}", verifyEmailDTO.Email);
            var user = await _userRepository.GetByEmailAsync(verifyEmailDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de verificación para email no existente: {Email}",
                    verifyEmailDTO.Email
                );
                throw new KeyNotFoundException("El usuario no esta registrado.");
            }
            if (user.EmailConfirmed)
            {
                Log.Information("Email ya verificado: {Email}", verifyEmailDTO.Email);
                return "El correo electrónico ya ha sido verificado.";
            }
            //Variable de Testing
            bool testing = _configuration.GetValue<bool>("Testing:IsTesting");
            CodeType type = CodeType.EmailConfirmation;

            var verificationCode = testing
                ? new VerificationCode
                {
                    Code =
                        _configuration.GetValue<string>("Testing:FixedVerificationCode")
                        ?? "000000",
                    CodeType = type,
                    UserId = user.Id,
                    Expiration = DateTime.UtcNow.AddHours(1),
                }
                : await _verificationCodeRepository.GetByLatestUserIdAsync(user.Id, type);

            if (
                verificationCode.Code != verifyEmailDTO.VerificationCode
                || DateTime.UtcNow >= verificationCode.Expiration
            )
            {
                int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                    user.Id,
                    type
                );
                Log.Warning(
                    "Intento de verificación fallido para usuario ID: {UserId}, Intentos: {Attempts}",
                    user.Id,
                    attempsCountUpdated
                );

                if (attempsCountUpdated >= 5)
                {
                    bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                        user.Id,
                        type
                    );
                    if (codeDeleteResult)
                    {
                        bool userDeleteResult = await _userRepository.DeleteUserAsync(user);
                        if (userDeleteResult)
                        {
                            Log.Warning(
                                "Usuario eliminado por exceder intentos de verificación. Email: {Email}, ID: {UserId}",
                                user.Email,
                                user.Id
                            );
                            throw new Exception(
                                "Se ha alcanzado el límite de intentos. El usuario ha sido eliminado."
                            );
                        }
                    }
                }
                if (DateTime.UtcNow >= verificationCode.Expiration)
                {
                    Log.Warning(
                        "Código de verificación expirado para usuario ID: {UserId}",
                        user.Id
                    );
                    throw new Exception("El código de verificación ha expirado.");
                }
                else
                {
                    throw new Exception(
                        $"El código de verificación es incorrecto, quedan {5 - attempsCountUpdated} intentos."
                    );
                }
            }
            bool emailConfirmed = await _userRepository.ConfirmEmailAsync(user.Email!);
            if (emailConfirmed)
            {
                bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                    user.Id,
                    type
                );
                if (codeDeleteResult)
                {
                    Log.Information(
                        "Email verificado exitosamente para usuario ID: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email
                    );
                    var emailResult = await _emailService.SendWelcomeEmailAsync(user.Email!);
                    if (emailResult)
                    {
                        Log.Information(
                            "Email de bienvenida enviado exitosamente a: {Email}",
                            user.Email
                        );
                        return "!Ya puedes iniciar sesión!";
                    }
                    else
                    {
                        Log.Error("Error al enviar email de bienvenida a: {Email}", user.Email);
                        return "Correo verificado, pero hubo un error al enviar el email de bienvenida.";
                    }
                }
                Log.Error(
                    "Error al eliminar código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al confirmar el correo electrónico.");
            }
            Log.Error("Error al confirmar email para usuario ID: {UserId}", user.Id);
            throw new Exception("Error al verificar el correo electrónico.");
        }

        /// <summary>
        /// Reenvia el mensaje de con el codigo de verificacion.
        /// </summary>
        /// <param name="resendVerificationDTO">Dto con los datos del usuatio</param>
        /// <returns>Mensaje de exito o error</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> ResendVerificationEmailAsync(
            ResendVerificationDTO resendVerificationDTO
        )
        {
            Log.Information(
                "Reenviando código de verificación al email: {Email}",
                resendVerificationDTO.Email
            );

            var user = await _userRepository.GetByEmailAsync(resendVerificationDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de reenvío de verificación para email no existente: {Email}",
                    resendVerificationDTO.Email
                );
                throw new KeyNotFoundException("El usuario no esta registrado.");
            }
            if (user.EmailConfirmed)
            {
                Log.Information("Email ya verificado: {Email}", resendVerificationDTO.Email);
                throw new InvalidOperationException("El correo electrónico ya ha sido verificado.");
            }
            var existingCode = await _verificationCodeRepository.GetByLatestUserIdAsync(
                user.Id,
                CodeType.EmailConfirmation
            );
            bool emailResult;
            if (existingCode != null && DateTime.UtcNow < existingCode.Expiration)
            {
                Log.Information(
                    "Código de verificación aún válido para usuario ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                emailResult = await _emailService.SendVerificationEmailAsync(
                    user.Email!,
                    existingCode.Code
                );
                if (!emailResult)
                {
                    Log.Error(
                        "Error al reenviar código de verificación para usuario ID: {UserId}, Email: {Email}",
                        user.Id,
                        user.Email
                    );
                    throw new Exception("Error al enviar el correo de verificación.");
                }
                return "El código de verificación anterior aún es válido. Por favor, revisa tu correo electrónico.";
            }
            //Generar nuevo código
            string newCode = new Random().Next(100000, 999999).ToString();
            VerificationCode newVerificationCode = new VerificationCode
            {
                Code = newCode,
                CodeType = CodeType.EmailConfirmation,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var createdCode = await _verificationCodeRepository.CreateCodeAsync(
                newVerificationCode
            );
            if (createdCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para usuario ID: {UserId}",
                    user.Id
                );
                throw new InvalidOperationException("Error al crear el código de verificación.");
            }

            Log.Information("Enviando email de verificación a: {Email}", user.Email);
            emailResult = await _emailService.SendVerificationEmailAsync(
                user.Email!,
                createdCode.Code
            );
            if (!emailResult)
            {
                Log.Error(
                    "Error al enviar código de verificación para usuario ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                throw new Exception("Error al enviar el correo de verificación.");
            }
            return "Código de verificación reenviado exitosamente.";
        }

        /// <summary>
        /// Inicia sesión en el sistema.
        /// </summary>
        /// <param name="loginDTO">Dto de inicio de sesión</param>
        /// <returns>Token de acceso</returns>
        public async Task<string> LoginAsync(LoginDTO loginDTO)
        {
            Log.Information("Intento de login para email: {Email}", loginDTO.Email);

            User? user = await _userRepository.GetByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                Log.Warning("Intento de login con email no registrado: {Email}", loginDTO.Email);
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }
            if (!user.EmailConfirmed)
            {
                Log.Warning(
                    "Intento de login con email no verificado: {Email}, UserId: {UserId}",
                    user.Email,
                    user.Id
                );
                throw new EmailNotVerifiedException(user.Email!);
            }
            var result = await _userRepository.CheckPasswordAsync(user, loginDTO.Password);
            if (!result)
            {
                Log.Warning(
                    "Intento de login con contraseña incorrecta para usuario: {Email}, UserId: {UserId}",
                    user.Email,
                    user.Id
                );
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }
            if (user.IsBlocked)
            {
                Log.Warning(
                    "Intento de login para usuario bloqueado: {Email}, UserId: {UserId}",
                    user.Email,
                    user.Id
                );
                throw new UnauthorizedAccessException(
                    "Tu cuenta ha sido bloqueada. Por favor, contacta al soporte para más información."
                );
            }
            var roles = await _userRepository.GetRolesAsync(user);
            Log.Information(
                "Login exitoso para usuario: {Email}, UserId: {UserId}, Roles: {Roles}",
                user.Email,
                user.Id,
                roles
            );
            var newToken = _tokenService.CreateToken(user, roles, loginDTO.RememberMe);
            var whitelist = new Whitelist
            {
                UserId = user.Id,
                Email = user.Email!,
                Token = newToken,
                Expiration = loginDTO.RememberMe
                    ? DateTime.UtcNow.AddDays(24)
                    : DateTime.UtcNow.AddHours(1),
            };
            var whitelistResult = await _tokenService.AddToWhitelistAsync(whitelist);

            if (!whitelistResult)
            {
                Log.Error(
                    "Error al agregar token a la whitelist para usuario: {Email}, UserId: {UserId}",
                    user.Email,
                    user.Id
                );
                throw new Exception("Error al iniciar sesión.");
            }
            var updateLoginTimeResult = await _userRepository.UpdateLastLoginAsync(user);
            if (!updateLoginTimeResult)
            {
                Log.Error(
                    "Error al actualizar la última hora de login para usuario: {Email}, UserId: {UserId}",
                    user.Email,
                    user.Id
                );
                throw new Exception("Error al iniciar sesión.");
            }
            return newToken;
        }

        /// <summary>
        /// Envía un código de verificación para el reseteo de contraseña al correo electrónico del usuario.
        /// </summary>
        /// <param name="requestResetPasswordCodeDTO">Dto que contiene el email para enviar el código de verificación</param>
        /// <returns>Mensaje indicando el resultado del envío del código de verificación</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> SendResetPasswordVerificationCodeEmailAsync(
            RequestResetPasswordCodeDTO requestResetPasswordCodeDTO
        )
        {
            Log.Information(
                "Enviando código de verificación de reseteo de contraseña al email: {Email}",
                requestResetPasswordCodeDTO.Email
            );
            var user = await _userRepository.GetByEmailAsync(requestResetPasswordCodeDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de reseteo de contraseña para email no registrado: {Email}",
                    requestResetPasswordCodeDTO.Email
                );
                throw new KeyNotFoundException("El usuario no existe no esta registrado.");
            }
            if (!user.EmailConfirmed)
            {
                Log.Warning(
                    "Intento de reseteo de contraseña para email no confirmado: {Email}",
                    requestResetPasswordCodeDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico no ha sido confirmado.");
            }
            //Variable de Testing
            bool testing = _configuration.GetValue<bool>("Testing:IsTesting");
            string code = new Random().Next(100000, 999999).ToString();

            VerificationCode verificationCode = new VerificationCode
            {
                Code = testing
                    ? _configuration.GetValue<string>("Testing:FixedVerificationCode")!
                    : code,
                CodeType = CodeType.PasswordReset,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddHours(1),
            };
            var newCode = await _verificationCodeRepository.CreateCodeAsync(verificationCode);
            if (newCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación de reseteo de contraseña para usuario ID: {UserId}",
                    user.Id
                );
                throw new InvalidOperationException("Error al crear el código de verificación.");
            }
            var emailResult = await _emailService.SendResetPasswordVerificationEmailAsync(
                user.Email!,
                newCode.Code
            );
            if (emailResult)
            {
                Log.Information(
                    "Código de verificación de reseteo de contraseña enviado exitosamente para usuario ID:{UserId}, Email: {Email}",
                    user.Id,
                    user.Email
                );
                return "Correo de reseteo de contraseña enviado exitosamente.";
            }
            return "El codigo fue creado pero ocurrió un error al enviar el correo de verificación. Por favor, solicita un nuevo código de verificación.";
        }

        /// <summary>
        /// Verifica el codigo de reseteo de contraseña ingresado.
        /// </summary>
        /// <param name="verifyResetPasswordCodeDTO">Dto con los el codigo y contraseña</param>
        /// <returns>Mensaje de exito o error</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> VerifyResetPasswordCodeAsync(
            VerifyResetPasswordCodeDTO verifyResetPasswordCodeDTO
        )
        {
            Log.Information(
                "Verificando código de reseteo de contraseña para email: {Email}",
                verifyResetPasswordCodeDTO.Email
            );
            var user = await _userRepository.GetByEmailAsync(verifyResetPasswordCodeDTO.Email);
            if (user == null)
            {
                Log.Warning(
                    "Intento de verificación de código de reseteo para email no registrado: {Email}",
                    verifyResetPasswordCodeDTO.Email
                );
                throw new KeyNotFoundException("El usuario no esta registrado.");
            }
            if (!user.EmailConfirmed)
            {
                Log.Warning(
                    "Intento de verificación de código de reseteo para email no confirmado: {Email}",
                    verifyResetPasswordCodeDTO.Email
                );
                throw new InvalidOperationException("El correo electrónico no ha sido confirmado.");
            }
            //Variable de Testing
            bool testing = _configuration.GetValue<bool>("Testing:IsTesting");
            var verificationCode = testing
                ? new VerificationCode
                {
                    Code =
                        _configuration.GetValue<string>("Testing:FixedVerificationCode")
                        ?? "000000",
                    CodeType = CodeType.PasswordReset,
                    UserId = user.Id,
                    Expiration = DateTime.UtcNow.AddHours(1),
                }
                : await _verificationCodeRepository.GetByLatestUserIdAsync(
                    user.Id,
                    CodeType.PasswordReset
                );
            if (
                verificationCode.Code != verifyResetPasswordCodeDTO.VerificationCode
                || DateTime.UtcNow >= verificationCode.Expiration
            )
            {
                int attempsCountUpdated = await _verificationCodeRepository.IncreaseAttemptsAsync(
                    user.Id,
                    verificationCode.CodeType
                );
                Log.Warning(
                    "Intento de verificación fallido para usuario ID: {UserId}, Intentos: {Attempts}",
                    user.Id,
                    attempsCountUpdated
                );

                if (attempsCountUpdated >= 5)
                {
                    bool codeDeleteResult = await _verificationCodeRepository.DeleteByUserIdAsync(
                        user.Id,
                        verificationCode.CodeType
                    );
                    if (codeDeleteResult)
                    {
                        Log.Warning(
                            "Código de verificación eliminado por exceder intentos. Email: {Email}, ID: {UserId}",
                            user.Email,
                            user.Id
                        );
                        throw new Exception(
                            "Se ha alcanzado el límite de intentos. El código de verificación ha sido eliminado."
                        );
                    }
                }
                if (DateTime.UtcNow >= verificationCode.Expiration)
                {
                    Log.Warning(
                        "Código de verificación expirado para usuario ID: {UserId}",
                        user.Id
                    );
                    throw new Exception("El código de verificación ha expirado.");
                }
                else
                {
                    throw new Exception(
                        $"El código de verificación es incorrecto, quedan {5 - attempsCountUpdated} intentos."
                    );
                }
            }
            Log.Information(
                "Código de verificación de reseteo de contraseña válido para usuario ID: {UserId}",
                user.Id
            );
            var newPasswordResult = await _userRepository.UpdatePasswordAsync(
                user,
                verifyResetPasswordCodeDTO.Password
            );
            if (!newPasswordResult)
            {
                Log.Error("Error al actualizar la contraseña para usuario ID: {UserId}", user.Id);
                throw new Exception("Error al actualizar la contraseña.");
            }
            return "Contraseña actualizada exitosamente.";
        }

        #region User Profiles

        /// <summary>
        /// Obtiene el perfil de un estudiante por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Perfil del usuario</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<GetUserProfileDTO> GetUserProfileByIdAsync(int userId)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions
                {
                    IncludePhoto = true,
                    IncludeCV = true,
                    TrackChanges = false,
                }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            Log.Information("Buscando detalles relevantes");
            return user.Adapt<GetUserProfileDTO>();
        }

        /// <summary>
        /// Actualiza el perfil de un usuario.
        /// </summary>
        /// <param name="updateParamsDTO">Parámetros de actualización</param>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userType">Tipo de usuario</param>
        /// <returns>Mensaje de exito</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> UpdateUserProfileByIdAsync(
            UpdateUserProfileDTO updateParamsDTO,
            int userId,
            UserType userType
        )
        {
            Log.Information(
                "Validando parametros de actualización para usuario ID: {UserId}",
                userId
            );
            // Validar Email
            if (updateParamsDTO.Email != null)
            {
                bool emailExists = await _userRepository.ExistsByEmailAsync(updateParamsDTO.Email);
                if (emailExists)
                {
                    Log.Error(
                        "El email proporcionado ya está en uso por otro usuario: {Email}",
                        updateParamsDTO.Email
                    );
                    throw new InvalidOperationException("El correo electrónico ya está en uso.");
                }
                // Validar dominio del email según el tipo de usuario
                if (
                    userType == UserType.Estudiante
                    && !updateParamsDTO.Email.EndsWith(
                        _configuration.GetValue<string>("StudentSettings:AllowedDomain")!
                    )
                )
                {
                    Log.Error(
                        "El email proporcionado no es válido para un estudiante: {Email}",
                        updateParamsDTO.Email
                    );
                    throw new InvalidOperationException(
                        "El email proporcionado no es válido para un estudiante."
                    );
                }
                else if (
                    userType == UserType.Administrador
                    && !updateParamsDTO.Email.EndsWith(
                        _configuration.GetValue<string>("AdminSettings:AllowedDomain")!
                    )
                )
                {
                    Log.Error(
                        "El email proporcionado no es válido para un administrador: {Email}",
                        updateParamsDTO.Email
                    );
                    throw new InvalidOperationException(
                        "El email proporcionado no es válido para un administrador."
                    );
                }
            }
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions
                {
                    IncludePhoto = true,
                    IncludeCV = true,
                    TrackChanges = true,
                }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            updateParamsDTO.Adapt(user);

            // Validar que los campos requeridos por el sistema estén presentes después de aplicar los cambios
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Rut))
            {
                Log.Error(
                    "El usuario actualizado no tiene todos los campos requeridos. Id: {UserId}",
                    userId
                );
                throw new ArgumentNullException(
                    "El usuario actualizado no tiene todos los campos requeridos."
                );
            }

            await _userRepository.UpdateAsync(user);

            return "Datos del usuario actualizados correctamente";
        }

        /// <summary>
        /// Obtiene la foto de perfil de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Foto de perfil del usuario</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<GetPhotoDTO> GetUserProfilePhotoByIdAsync(int userId)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { IncludePhoto = true }
            );

            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            return user.Adapt<GetPhotoDTO>();
        }

        /// <summary>
        /// Actualiza la foto de perfil de un usuario por su ID.
        /// </summary>
        /// <param name="updatePhotoDTO">Parámetros para actualizar la foto de perfil del usuario.</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de éxito</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> UpdateUserProfilePhotoByIdAsync(
            UpdatePhotoDTO updatePhotoDTO,
            int userId
        )
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { IncludePhoto = true, TrackChanges = true }
            );

            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            await _fileService.UploadUserImageAsync(updatePhotoDTO.Photo, user);

            await _userRepository.UpdateAsync(user);

            return "Foto de perfil del usuario actualizada correctamente";
        }

        public async Task<string> ChangeUserEmailByIdAsync(
            ChangeUserEmailDTO changeUserEmailDTO,
            int userId
        )
        {
            // Validar disponibilidad del nuevo correo
            bool emailExists = await _userRepository.ExistsByEmailAsync(
                changeUserEmailDTO.NewEmail
            );
            if (emailExists)
            {
                Log.Error(
                    "El email proporcionado ya está en uso por otro usuario: {Email}",
                    changeUserEmailDTO.NewEmail
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            // Validar usuario
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { TrackChanges = true }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            Log.Information("Validando contraseña actual para usuario ID: {UserId}", userId);
            bool isPasswordValid = await _userRepository.CheckPasswordAsync(
                user,
                changeUserEmailDTO.CurrentPassword
            );
            if (!isPasswordValid)
            {
                Log.Warning(
                    "Intento de cambio de correo fallido para usuario ID: {UserId} debido a contraseña incorrecta",
                    userId
                );
                throw new SecurityException("La contraseña actual es incorrecta.");
            }
            Log.Information(
                "Actualizando correo electrónico para usuario ID: {UserId}, Correo: {NewEmail}",
                userId,
                changeUserEmailDTO.NewEmail
            );

            // Enviar correo de verificación al nuevo email
            string verificationCode = new Random().Next(100000, 999999).ToString();
            VerificationCode newVerificationCode = new VerificationCode
            {
                Code = verificationCode,
                CodeType = CodeType.EmailChange,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddDays(_daysOfUnconfirmedPendingEmailChangeRetention),
            };
            var createdCode = await _verificationCodeRepository.CreateCodeAsync(
                newVerificationCode
            );
            if (createdCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para cambio de correo del usuario ID: {UserId}",
                    user.Id
                );
                throw new InvalidOperationException("Error al crear el código de verificación.");
            }
            Log.Information("Enviando email de verificación a: {Email}", user.PendingEmail);
            var emailResult = await _emailService.SendChangeEmailVerificationEmailAsync(
                user.PendingEmail!,
                createdCode.Code
            );
            if (!emailResult)
            {
                Log.Error(
                    "Error al enviar código de verificación para cambio de correo del usuario ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.PendingEmail
                );
                throw new Exception("Error al enviar el correo de verificación.");
            }
            // Programar trabajo para eliminar el correo pendiente y código de verificación si no se confirma el cambio en el tiempo establecido
            string jobId = BackgroundJob.Schedule<IUserJobs>(
                nameof(IUserJobs.ClearExpiredPendingEmailChangeRequestAsync).ToLower() + user.Id,
                job => job.ClearExpiredPendingEmailChangeRequestAsync(user.Id),
                DateTime
                    .UtcNow.AddDays(_daysOfUnconfirmedPendingEmailChangeRetention)
                    .AddMinutes(10)
                    .ToUniversalTime()
            );

            // Guardar el correo pendiente y el ID del trabajo programado en el usuario para futuras referencias
            user.PendingEmail = changeUserEmailDTO.NewEmail;
            user.PendingEmailJobId = jobId;
            bool result = await _userRepository.UpdateAsync(user);
            if (!result)
            {
                Log.Error(
                    "Error al actualizar el correo electrónico para usuario ID: {UserId}",
                    userId
                );
                throw new Exception("Error al actualizar el correo electrónico.");
            }
            return "Correo de verificación enviado al nuevo correo electrónico.";
        }

        public async Task<string> ResendChangeEmailVerificationByIdAsync(int userId)
        {
            // Validar usuario
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { TrackChanges = true }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            if (user.PendingEmail == null)
            {
                Log.Error(
                    "Intento de reenvío de verificación de cambio de correo sin una solicitud previa. Usuario ID: {UserId}",
                    userId
                );
                throw new InvalidOperationException(
                    "No hay una solicitud de cambio de correo pendiente o la solicitud ha expirado."
                );
            }

            //Validar codigo de verificacion existente
            VerificationCode? existingCode =
                await _verificationCodeRepository.GetByLatestUserIdAsync(
                    user.Id,
                    CodeType.EmailChange
                );
            if (existingCode != null && DateTime.UtcNow < existingCode.Expiration)
            {
                Log.Information(
                    "Código de verificación aún válido para usuario ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.PendingEmail
                );
            }

            VerificationCode newCode = new VerificationCode
            {
                Code = new Random().Next(100000, 999999).ToString(),
                CodeType = CodeType.EmailChange,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddDays(_daysOfUnconfirmedPendingEmailChangeRetention),
            };

            var createdCode = await _verificationCodeRepository.CreateCodeAsync(newCode);
            if (createdCode == null)
            {
                Log.Error(
                    "Error al crear código de verificación para cambio de correo del usuario ID: {UserId}",
                    user.Id
                );
                throw new InvalidOperationException("Error al crear el código de verificación.");
            }

            bool emailResult = await _emailService.SendChangeEmailVerificationEmailAsync(
                user.PendingEmail!,
                newCode.Code
            );
            if (!emailResult)
            {
                Log.Error(
                    "Error al enviar código de verificación para cambio de correo del usuario ID: {UserId}, Email: {Email}",
                    user.Id,
                    user.PendingEmail
                );
                throw new Exception("Error al enviar el correo de verificación.");
            }

            // Reprogramar trabajo para eliminar el correo pendiente y código de verificación si no se confirma el cambio en el tiempo establecido
            BackgroundJob.Reschedule(
                user.PendingEmailJobId,
                DateTime
                    .UtcNow.AddDays(_daysOfUnconfirmedPendingEmailChangeRetention) // Resetea el contador para el usuario
                    .AddMinutes(10)
                    .ToUniversalTime()
            );
            return "Código de verificación reenviado exitosamente al nuevo correo electrónico.";
        }

        public async Task<string> VerifyNewEmailByIdAsync(
            VerifyNewEmailDTO verifyNewEmailDTO,
            int userId
        )
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool isEmailAvailable = !await _userRepository.ExistsByEmailAsync(user.PendingEmail!);
            if (!isEmailAvailable)
            {
                Log.Error(
                    "El email pendiente ya está en uso por otro usuario: {Email}",
                    user.PendingEmail
                );
                throw new InvalidOperationException("El correo electrónico ya está en uso.");
            }
            var verificationCode = await _verificationCodeRepository.GetByLatestUserIdAsync(
                user.Id,
                CodeType.EmailChange
            );
            if (
                verificationCode == null
                || verificationCode.Code != verifyNewEmailDTO.VerificationCode
                || DateTime.UtcNow >= verificationCode.Expiration
            )
            {
                Log.Warning(
                    "Código de verificación inválido o expirado para usuario ID: {UserId}",
                    user.Id
                );
                throw new Exception("El código de verificación es incorrecto o ha expirado.");
            }
            // Cancela el trabajo programado para eliminar el correo pendiente ya que se ha confirmado el cambio de correo
            BackgroundJob.Delete(user.PendingEmailJobId);

            user.Email = user.PendingEmail;
            user.PendingEmail = null;
            user.PendingEmailJobId = null;

            bool updateResult = await _userRepository.UpdateAsync(user);
            if (!updateResult)
            {
                Log.Error(
                    "Error al actualizar el correo electrónico para usuario ID: {UserId}",
                    userId
                );
                throw new Exception("Error al actualizar el correo electrónico.");
            }

            return "Correo electrónico actualizado correctamente.";
        }

        /// <summary>
        /// Cambia la contraseña de un usuario por su ID.
        /// </summary>
        /// <param name="changeUserPasswordDTO">Parámetros para cambiar la contraseña del usuario.</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Mensaje de éxito</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> ChangeUserPasswordById(
            ChangeUserPasswordDTO changeUserPasswordDTO,
            int userId
        )
        {
            Log.Information("Buscando usuario con la Id: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { TrackChanges = true }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            var isPasswordValid = await _userRepository.CheckPasswordAsync(
                user,
                changeUserPasswordDTO.CurrentPassword
            );
            if (!isPasswordValid)
            {
                Log.Warning(
                    "Intento de cambio de contraseña fallido para usuario ID: {UserId} debido a contraseña actual incorrecta",
                    userId
                );
                throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");
            }
            var result = await _userRepository.UpdatePasswordAsync(
                user,
                changeUserPasswordDTO.NewPassword
            );
            if (!result)
            {
                Log.Error("Error al actualizar la contraseña para usuario ID: {UserId}", userId);
                throw new Exception("Error al actualizar la contraseña.");
            }
            return "Contraseña actualizada exitosamente.";
        }

        #endregion
        #region Documents Management

        /// <summary>
        /// Sube el CV de un usuario por su ID.
        /// </summary>
        /// <param name="uploadCVDTO">Datos para subir el CV.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Mensaje de éxito.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> UploadCVByIdAsync(UploadCVDTO uploadCVDTO, int userId)
        {
            // Validar usuario
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { IncludeCV = true, TrackChanges = true }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            // Validar que el CV pueda ser actualizado
            bool hasApplicationsPending =
                await _applicationRepository.HasPendingCvRequiredApplication(user.Id);
            if (hasApplicationsPending)
            {
                Log.Error(
                    "El usuario con ID: {UserId} tiene postulaciones pendientes que requieren CV. No se puede actualizar el CV.",
                    userId
                );
                throw new InvalidOperationException(
                    "No se puede actualizar el CV porque tienes postulaciones pendientes que requieren un CV."
                );
            }
            if (user.CV != null && user.UserType == UserType.Estudiante)
            {
                Log.Information(
                    "El usuario con ID: {UserId} ya tiene un CV, se reemplazará el existente.",
                    user.Id
                );
                var deleteResult = await _fileRepository.DeleteCVAsync(user.CV.PublicId);
                if (!deleteResult)
                {
                    Log.Error(
                        "Error al eliminar el CV existente del estudiante con ID: {UserId}",
                        user.Id
                    );
                    throw new Exception("Error al eliminar el CV existente del estudiante");
                }
                await _applicationRepository.MarkCvAsInvalidAsync(user.Id);
            }
            var uploadResult = await _fileService.UploadPDFAsync(uploadCVDTO.CVFile, user);
            if (!uploadResult)
            {
                Log.Error("Error al subir el CV del estudiante con ID: {UserId}", user.Id);
                throw new Exception("Error al subir el CV del estudiante");
            }

            return "CV del usuario actualizado correctamente";
        }

        public async Task<(
            MemoryStream FileStream,
            string FileName,
            string ContentType
        )> DownloadCVByIdAsync(int userId)
        {
            // Validacion de usuario
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { IncludeCV = true }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            if (user.CV == null)
            {
                Log.Warning("El usuario con ID: {UserId} no tiene un CV para descargar.", userId);
                throw new KeyNotFoundException("El usuario no tiene un CV para descargar");
            }

            // Acceso a el archivo por url firmada
            string signedUrl = await _fileService.BuildSignedUrlForCVAsync(user.CV.PublicId);

            // Descarga del archivo desde el servidor de hosting
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(signedUrl);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error(
                    "Error al descargar el CV del estudiante con ID: {UserId} desde el servidor de hosting. StatusCode: {StatusCode}",
                    user.Id,
                    response.StatusCode
                );
                throw new Exception(
                    "Error al descargar el CV del estudiante desde el servidor de hosting"
                );
            }
            var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            string fileName = $"CV_{user.FirstName}_{user.LastName}.pdf"
                .Replace(" ", "_")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");

            Log.Information(
                "CV del estudiante con ID: {UserId} descargado exitosamente desde el servidor de hosting",
                user.Id
            );
            return (memoryStream, fileName, "application/pdf");
        }

        public async Task<HasCVDTO> CheckCVExistsByIdAsync(int userId)
        {
            Log.Information("Buscando usuario con la ID: {UserId}", userId);

            // Validacion de usuarios
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { IncludeCV = true }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            if (user.CV == null)
            {
                Log.Warning("El usuario con ID: {UserId} no tiene un CV para descargar.", userId);
            }
            Log.Information("CV encontrado para el usuario con ID: {UserId}", userId);
            HasCVDTO hasCVDTO = new() { HasCV = user.CV != null };
            return hasCVDTO;
        }

        public async Task<string> DeleteCVByIdAsync(int userId)
        {
            // Validar usuario
            Log.Information("Buscando usuario con la ID: {UserId}", userId);
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { IncludeCV = true, TrackChanges = true }
            );
            if (user == null)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            if (user.CV == null || user.UserType != UserType.Estudiante)
            {
                Log.Warning("El usuario con ID: {UserId} no tiene un CV para eliminar.", userId);
                throw new KeyNotFoundException("El usuario no tiene un CV para eliminar");
            }
            // Validar capacidad de eliminar el CV
            bool hasApplicationsPending =
                await _applicationRepository.HasPendingCvRequiredApplication(user.Id);
            if (hasApplicationsPending)
            {
                Log.Warning(
                    "El usuario con ID: {UserId} tiene postulaciones pendientes que requieren CV. No se puede eliminar el CV.",
                    userId
                );
                throw new InvalidOperationException(
                    "No se puede eliminar el CV porque tiene postulaciones pendientes que requieren CV."
                );
            }
            // Eliminar CV
            var deleteResult = await _fileRepository.DeleteCVAsync(user.CV.PublicId);
            if (!deleteResult)
            {
                Log.Error("Error al eliminar el CV del estudiante con ID: {UserId}", user.Id);
                throw new Exception("Error al eliminar el CV del estudiante");
            }
            user.CVId = null;
            bool result = await _userRepository.UpdateAsync(user);
            if (!result)
            {
                Log.Error(
                    "Error al actualizar el usuario después de eliminar el CV. ID: {UserId}",
                    user.Id
                );
                throw new Exception("Error al actualizar el usuario después de eliminar el CV");
            }
            // Update closed applications with the outdated cv flag.
            await _applicationRepository.MarkCvAsInvalidAsync(user.Id);

            return "CV eliminado exitosamente";
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Funcion helper para normalizar el numero de telefono.
        /// </summary>
        /// <param name="phoneNumber">Numero de telefono del usuario</param>
        /// <returns>Numero de telefono normalizado</returns>
        private string NormalizePhoneNumber(string phoneNumber)
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return "+56" + digits;
        }

        #endregion

        #region Background Jobs

        public async Task DeleteUnconfirmedUserAccountsAsync()
        {
            Log.Information(
                "Iniciando proceso de eliminación de cuentas de usuario no confirmadas."
            );
            // La fecha de corte se calcula restando el número de días de retención a la fecha actual. Si el número de días es negativo, se suman en su lugar, lo que permite flexibilidad en la configuración.
            DateTime cutoffDate =
                _daysOfUnconfirmedUserRetention > 0
                    ? DateTime.UtcNow.AddDays(_daysOfUnconfirmedUserRetention * -1)
                    : DateTime.UtcNow.AddDays(_daysOfUnconfirmedUserRetention);

            DateTime.UtcNow.AddDays(_daysOfUnconfirmedUserRetention * -1);
            (int deletedUsersCount, int deletedVerificationCodesCount) =
                await _userRepository.DeleteUnconfirmedUsersByCutoffDateAsync(cutoffDate);
            Log.Information(
                "Proceso de eliminación de cuentas de usuario no confirmadas completado. Cantidad de usuarios eliminados: {Count}, Cantidad de códigos de verificación eliminados: {VerificationCodesCount}",
                deletedUsersCount,
                deletedVerificationCodesCount
            );
        }

        public async Task ClearExpiredPendingEmailChangeRequestAsync(int userId)
        {
            Log.Information(
                "Iniciando proceso de limpieza de solicitudes de cambio de correo electrónico no confirmadas."
            );
            bool userExists = await _userRepository.ExistsByIdAsync(userId);
            if (!userExists)
            {
                Log.Error("Usuario no encontrado con la ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            bool pendingEmailCleared =
                await _userRepository.ClearUnconfirmedEmailChangeRequestsAsync(userId);
            if (!pendingEmailCleared)
            {
                Log.Error(
                    "Error al limpiar solicitudes de cambio de correo electrónico no confirmadas para el usuario ID: {UserId}",
                    userId
                );
                throw new Exception(
                    "Error al limpiar solicitudes de cambio de correo electrónico no confirmadas."
                );
            }
            Log.Information(
                "Proceso de limpieza de solicitudes de cambio de correo electrónico no confirmadas completado para el usuario ID: {UserId}",
                userId
            );
        }
        #endregion
    }
}
