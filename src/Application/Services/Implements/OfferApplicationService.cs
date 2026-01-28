using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.Events;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class OfferApplicationService : IOfferApplicationService
    {
        private readonly IOfferApplicationRepository _jobApplicationRepository;
        private readonly IOfferService _offerService;
        private readonly IOfferRepository _offerRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IReviewService _reviewService;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public OfferApplicationService(
            IOfferApplicationRepository jobApplicationRepository,
            IOfferService offerService,
            IOfferRepository offerRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IReviewService reviewService,
            IConfiguration configuration
        )
        {
            _jobApplicationRepository = jobApplicationRepository;
            _offerService = offerService;
            _offerRepository = offerRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _reviewService = reviewService;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        //! COMPLETE
        public async Task<string> CreateApplicationAsync(
            int applicantId,
            int offerId,
            CoverLetterDTO coverLetter
        )
        {
            // Validar usuario
            User? user = await _userRepository.GetByIdAsync(
                applicantId,
                new UserQueryOptions { IncludeCV = true }
            );
            if (user == null)
            {
                Log.Error(
                    "Usuario ID: {ApplicantId} no encontrado al crear postulación",
                    applicantId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            bool isApplicant = await _userRepository.CheckRoleAsync(user.Id, RoleNames.Applicant);
            if (!isApplicant)
            {
                Log.Error(
                    "Usuario ID: {ApplicantId} no tiene rol de Applicant al crear postulación",
                    applicantId
                );
                throw new UnauthorizedAccessException(
                    "Solo los usuarios con rol de Applicant pueden postular a ofertas"
                );
            }
            // Verificar que la oferta existe y está activa
            Offer? offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
            {
                Log.Error("Oferta ID: {OfferId} no encontrada al intentar postular", offerId);
                throw new KeyNotFoundException("La oferta no existe");
            }
            else if (!offer.IsOpen)
            {
                Log.Warning(
                    "Usuario {UserId} intentó postular a oferta inactiva {OfferId}",
                    applicantId,
                    offerId
                );
                throw new InvalidOperationException("No puedes postular a una oferta inactiva");
            }
            // Verificar que el usuario tenga CV si es necesario
            bool isCVRequired = offer.IsCvRequired;
            if (isCVRequired && user.CV == null)
            {
                Log.Error(
                    "Usuario {UserId} intentó postular a oferta {OfferId} sin CV requerido",
                    user.Id,
                    offerId
                );
                throw new InvalidOperationException(
                    "Se requiere un CV para postular a esta oferta"
                );
            }
            // Validar que el usuario no tenga más de 3 reseñas pendientes
            var pendingReviewsCount = await _reviewService.GetPendingReviewsCountAsync(
                user,
                RoleNames.Applicant // Ya que solo los Applicants pueden postular
            );
            if (pendingReviewsCount >= 3)
            {
                Log.Warning(
                    "Usuario {UserId} intentó postular a oferta con {PendingCount} reseñas pendientes",
                    user.Id,
                    pendingReviewsCount
                );
                throw new UnauthorizedAccessException(
                    "No puedes postular a nuevas ofertas hasta que completes todas tus reseñas pendientes"
                );
            }

            // Validar que la fecha límite no haya expirado
            if (offer.ApplicationDeadline < DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "La fecha límite para postular a esta oferta ha expirado"
                );
            }

            // Validar que la oferta no haya finalizado
            if (offer.EndDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Esta oferta ha finalizado");
            }

            // Verificar que no haya postulado anteriormente
            var hasAppliedResult = await HasAlreadyAppliedAsync(user.Id, offerId);
            if (hasAppliedResult)
            {
                Log.Error(
                    "Usuario {UserId} intentó postular nuevamente a oferta {OfferId}",
                    user.Id,
                    offerId
                );
                throw new InvalidOperationException("Ya has postulado a esta oferta");
            }

            // Crear la postulación
            var jobApplication = new JobApplication
            {
                StudentId = user.Id,
                JobOfferId = offerId,
                Status = ApplicationStatus.Pendiente,
                CoverLetter = coverLetter.CoverLetter,
                CreatedAt = DateTime.UtcNow,
            };

            bool result = await _jobApplicationRepository.AddAsync(jobApplication);
            if (!result)
            {
                Log.Error(
                    "Error al crear postulación para usuario {UserId} a oferta {OfferId}",
                    user.Id,
                    offerId
                );
                throw new Exception("No se pudo crear la postulación");
            }

            return "Tu postulación ha sido creada exitosamente.";
        }

        public async Task<ApplicationsForApplicantDTO> GetApplicationsByUserIdAsync(
            int applicantId,
            SearchParamsDTO searchParams
        )
        {
            // Validar usuario
            bool applicantExists = await _userRepository.ExistsByIdAsync(applicantId);
            if (!applicantExists)
            {
                Log.Error(
                    "Usuario ID: {ApplicantId} no encontrado al obtener sus postulaciones",
                    applicantId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            Log.Information(
                "Obteniendo postulaciones hechas por el usuario ID: {ApplicantId}",
                applicantId
            );

            // Obtener postulaciones con filtros y paginación
            var (applications, totalCount) =
                await _jobApplicationRepository.GetByApplicantIdFilteredAsync(
                    applicantId,
                    searchParams
                );

            int pageSize = searchParams.PageSize ?? _defaultPageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int currentPage = searchParams.PageNumber;
            if (currentPage < 1 || currentPage > totalPages)
            {
                Log.Warning(
                    "Página solicitada {currentPage} fuera de rango. Total de páginas: {totalPages}. Se ajusta a la página 1.",
                    currentPage,
                    totalPages
                );
                currentPage = 1;
            }

            Log.Information(
                "Postulaciones obtenidas: {TotalCount} para usuario ID: {ApplicantId}",
                totalCount,
                applicantId
            );

            return new ApplicationsForApplicantDTO
            {
                Applications = applications.Adapt<List<ApplicationForApplicantDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = pageSize,
                CurrentPage = currentPage,
            };
        }

        public async Task<GetApplicationDetailsDTO> GetApplicationDetailsForApplicantAsync(
            int userId,
            int applicationId
        )
        {
            // Validar al usuario
            bool userExists = await _userRepository.ExistsByIdAsync(userId);
            if (!userExists)
            {
                Log.Error(
                    "Usuario ID: {UserId} no encontrado al obtener detalles de postulación",
                    userId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            // Obtener la postulación
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no encontrada al obtener detalles",
                    applicationId
                );
                throw new KeyNotFoundException("La postulación no existe");
            }
            // Verificar que la postulación pertenezca al usuario
            if (application.StudentId != userId)
            {
                Log.Error(
                    "Usuario ID: {UserId} intentó acceder a postulación ID: {ApplicationId} que no le pertenece",
                    userId,
                    applicationId
                );
                throw new UnauthorizedAccessException(
                    "No tienes permiso para ver los detalles de esta postulación"
                );
            }

            Log.Information(
                "Obteniendo detalles de postulación ID: {ApplicationId} para usuario ID: {UserId}",
                applicationId,
                userId
            );
            // Obtener detalles de la oferta asociada
            var offer = await _offerService.GetByOfferIdAsync(application.JobOfferId);
            if (offer == null)
            {
                Log.Error(
                    "Oferta ID: {OfferId} asociada a postulación ID: {ApplicationId} no encontrada",
                    application.JobOfferId,
                    applicationId
                );
                throw new KeyNotFoundException("La oferta asociada a la postulación no existe");
            }

            // Obtener detalles del autor de la oferta
            User user = offer.User;
            var authorName = user.UserType switch
            {
                UserType.Empresa => user.FirstName ?? "Empresa desconocida",
                UserType.Particular or UserType.Estudiante =>
                    $"{(user.FirstName ?? "").Trim()} {(user.LastName ?? "").Trim()}".Trim(),
                UserType.Administrador => "Administrador",
                _ => user.UserName ?? "Nombre desconocido",
            };

            var statusMessage = application.Status switch
            {
                ApplicationStatus.Pendiente =>
                    "Su solicitud fue enviada con éxito; será contactado a la brevedad.",
                ApplicationStatus.Aceptada => "¡Felicidades! Tu solicitud ha sido aceptada.",
                ApplicationStatus.Rechazada => "Lamentablemente, tu solicitud ha sido rechazada.",
                _ => "Estado de solicitud desconocido.",
            };

            // Mapear a DTO y agregar información configurada
            var detailsDTO = application.Adapt<GetApplicationDetailsDTO>();
            detailsDTO.CompanyName = authorName;
            detailsDTO.StatusMessage = statusMessage;

            return detailsDTO;
        }

        public async Task<string> UpdateApplicationDetailsAsync(
            int userId,
            int applicationId,
            UpdateApplicationDetailsDTO updateDto
        )
        {
            // Validar al usuario
            bool userExists = await _userRepository.ExistsByIdAsync(userId);
            if (!userExists)
            {
                Log.Error(
                    "Usuario ID: {UserId} no encontrado al intentar actualizar detalles de postulación",
                    userId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            // Obtener la postulación
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no encontrada al intentar actualizar detalles",
                    applicationId
                );
                throw new KeyNotFoundException("La postulación no existe");
            }
            // Verificar que la postulación pertenezca al usuario
            if (application.StudentId != userId)
            {
                Log.Error(
                    "Usuario ID: {UserId} intentó actualizar postulación ID: {ApplicationId} que no le pertenece",
                    userId,
                    applicationId
                );
                throw new UnauthorizedAccessException(
                    "No tienes permiso para actualizar los detalles de esta postulación"
                );
            }
            // Validar estado de postulacion
            if (application.Status != ApplicationStatus.Pendiente)
            {
                Log.Error(
                    "Usuario ID: {UserId} intentó actualizar postulación ID: {ApplicationId} con estado {Status}",
                    userId,
                    applicationId,
                    application.Status
                );
                throw new InvalidOperationException(
                    "Solo se pueden actualizar los detalles de postulaciones en estado Pendiente"
                );
            }

            // Actualizar los detalles
            application.CoverLetter = updateDto.CoverLetter;
            bool updateResult = await _jobApplicationRepository.UpdateAsync(application);
            if (!updateResult)
            {
                Log.Error(
                    "Error al actualizar detalles de postulación ID: {ApplicationId} para usuario ID: {UserId}",
                    applicationId,
                    userId
                );
                throw new Exception("No se pudieron actualizar los detalles de la postulación");
            }

            Log.Information(
                "Usuario ID: {UserId} actualizó detalles de postulación ID: {ApplicationId}",
                userId,
                applicationId
            );

            return "Detalles de la postulación actualizados con éxito";
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByOfferIdAsync(
            int offerId
        )
        {
            var applications = await _jobApplicationRepository.GetByOfferIdAsync(offerId);

            return applications.Select(app => new JobApplicationResponseDto
            {
                Id = app.Id,
                StudentName = $"{app.Student.FirstName} {app.Student.LastName}",
                StudentEmail = app.Student.Email!,
                OfferTitle = app.JobOffer.Title,
                Status = app.Status.ToString(),
                ApplicationDate = app.CreatedAt,
                //CurriculumVitae = app.Student.CurriculumVitae,
                //MotivationLetter = app.Student.MotivationLetter,
            });
        }

        /**
        *? UNUSED METHOD
        public async Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByCompanyIdAsync(
            int companyId
        )
        {
            // Obtener todas las ofertas de la empresa
            var companyOffers = await _offerService.GetOffersByUserIdAsync(companyId);
            var offerIds = companyOffers.Select(o => o.Id).ToList();

            // Obtener todas las postulaciones de esas ofertas
            var allApplications = new List<JobApplicationResponseDto>();

            foreach (var offerId in offerIds)
            {
                var applications = await GetApplicationsByOfferIdAsync(offerId);
                allApplications.AddRange(applications);
            }

            return allApplications.OrderByDescending(a => a.ApplicationDate);
        }
        */

        public async Task<bool> UpdateApplicationStatusAsync(
            int applicationId,
            ApplicationStatus newStatus,
            int OwnnerUserId
        )
        {
            // La validación del enum se hace automáticamente al recibir el parámetro
            if (!Enum.IsDefined(typeof(ApplicationStatus), newStatus))
            {
                throw new ArgumentException(
                    $"Estado inválido. Debe ser uno de: {string.Join(", ", Enum.GetNames(typeof(ApplicationStatus)))}"
                );
            }

            // Obtener la postulación
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no encontrada para actualizar estado",
                    applicationId
                );
                throw new KeyNotFoundException("Postulación no encontrada");
            }

            // Verificar que la oferta pertenece a la empresa
            var offer = await _offerService.GetByOfferIdAsync(application.JobOfferId);
            if (offer == null || offer.UserId != OwnnerUserId)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permiso para modificar esta postulación"
                );
            }

            // Actualizar el estado
            application.Status = newStatus;
            await _jobApplicationRepository.UpdateAsync(application);

            // Obtener información del oferente para el email
            var offererUser = await _userRepository.GetByIdAsync(OwnnerUserId);
            var companyName =
                offererUser?.UserType == UserType.Empresa
                    ? (offererUser.FirstName ?? "Empresa desconocida")
                : offererUser?.UserType == UserType.Particular
                    ? $"{offererUser.FirstName ?? ""} {offererUser.LastName ?? ""}".Trim()
                : "Nombre desconocido";

            // Enviar notificación y email al estudiante
            var statusEvent = new PostulationStatusChangedEvent
            {
                PostulationId = applicationId,
                NewStatus = newStatus,
                OfferName = offer.Title,
                CompanyName = companyName,
                StudentEmail = application.Student.Email!,
            };

            await _notificationService.SendPostulationStatusChangeAsync(statusEvent);

            return true;
        }

        public async Task<IEnumerable<ViewApplicantsDto>> GetApplicantsForAdminManagement(
            int offerId
        )
        {
            var applicant = await _jobApplicationRepository.GetByOfferIdAsync(offerId);
            return applicant
                .Select(app => new ViewApplicantsDto
                {
                    Id = app.Id,
                    Applicant = $"{app.Student.FirstName} {app.Student.LastName}",
                    Status = app.Status.ToString(),
                    Rating = app.Student.Rating,
                })
                .ToList();
        }

        public async Task<ViewApplicantDetailAdminDto> GetApplicantDetailForAdmin(int studentId)
        {
            var applicant = await _jobApplicationRepository.GetByIdAsync(studentId); // Esto no tiene sentido. GetbyIdAsync recibe applicationId, no studentId y devuelve una solicitud, no un estudiante (usuario).
            return new ViewApplicantDetailAdminDto
            {
                Id = applicant.Id,
                StudentName = $"{applicant.Student.FirstName} {applicant.Student.LastName}",
                Email = applicant.Student.Email,
                PhoneNumber = applicant.Student.PhoneNumber,
                Status = applicant.Status.ToString(),
                CurriculumVitae = string.Empty,
                //CurriculumVitae = applicant.Student.CurriculumVitae,
                Rating = (float?)applicant.Student.Rating,
                MotivationLetter = string.Empty,
                //MotivationLetter = applicant.Student.MotivationLetter,
                Disability = applicant.Student.Disability.ToString(),
                ProfilePicture = applicant.Student.ProfilePhoto?.Url,
                // TODO: falta descripcion
            };
        }

        public async Task<IEnumerable<OffererApplicantViewDto>> GetApplicantsForOffererAsync(
            int offerId,
            int offererUserId
        )
        {
            var offer = await _offerService.GetByOfferIdAsync(offerId);
            if (offer == null)
            {
                throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
            }

            // Comprueba que el ID del usuario de la oferta (offer.UserId)
            // sea el mismo que el ID del usuario logueado (offererUserId).
            if (offer.UserId != offererUserId)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permiso para ver los postulantes de esta oferta, ya que no eres el propietario."
                );
            }

            // Tu repositorio GetByOfferIdAsync ya incluye Student y Student.Student (según tu código)
            var applications = await _jobApplicationRepository.GetByOfferIdAsync(offerId);

            // 4. Mapear al nuevo DTO que creamos
            var applicantDtos = applications
                .Select(app => new OffererApplicantViewDto
                {
                    ApplicationId = app.Id,
                    StudentId = app.StudentId,
                    ApplicantName = $"{app.Student.FirstName} {app.Student.LastName}",
                    Status = app.Status.ToString(),
                    ApplicationDate = app.CreatedAt,
                    // Enviamos el link del CV directamente
                    //CurriculumVitaeUrl = app.Student.CurriculumVitae,
                    Rating = app.Student.Rating,
                })
                .ToList();

            return applicantDtos;
        }

        public async Task<ViewApplicantUserDetailDto> GetApplicantDetailForOfferer(
            int studentId,
            int offerId,
            int offererUserId
        )
        {
            var offer = await _offerService.GetByOfferIdAsync(offerId);
            if (offer == null)
                throw new KeyNotFoundException($"Oferta {offerId} no encontrada.");

            if (offer.UserId != offererUserId)
                throw new UnauthorizedAccessException("No eres el dueño de esta oferta.");

            var applicationsList = await _jobApplicationRepository.GetByStudentIdAsync(studentId);

            var applicant = applicationsList.FirstOrDefault(app => app.JobOfferId == offerId);

            if (applicant == null)
            {
                throw new KeyNotFoundException(
                    "Este estudiante no ha postulado a esta oferta específica."
                );
            }

            return new ViewApplicantUserDetailDto
            {
                Id = applicant.Id,
                StudentName = $"{applicant.Student.FirstName} {applicant.Student.LastName}",
                Email = applicant.Student.Email,
                PhoneNumber = applicant.Student.PhoneNumber,
                Status = applicant.Status.ToString(),
                //CurriculumVitae = applicant.Student.CurriculumVitae,
                CurriculumVitae = string.Empty,
                Rating = (float?)applicant.Student.Rating,
                //MotivationLetter = applicant.Student.Student?.MotivationLetter,
                MotivationLetter = string.Empty,
                Disability = applicant.Student.Disability.ToString(),
                ProfilePicture = applicant.Student.ProfilePhoto?.Url,
            };
        }

        /// <summary>
        /// Verifica si un estudiante ya ha postulado a una oferta específica.
        /// </summary>
        /// <param name="applicantId">ID del estudiante</param>
        /// <param name="offerId">ID de la oferta</param>
        /// <returns>Indica si el estudiante ya ha postulado a la oferta</returns>
        private async Task<bool> HasAlreadyAppliedAsync(int applicantId, int offerId)
        {
            return await _jobApplicationRepository.ExistsByApplicantIdAndOfferId(
                applicantId,
                offerId
            );
        }

        /**
        * ? LEGACY METHOD FOR COMPATIBILTY
        */
        public async Task<IEnumerable<JobApplicationResponseDto>> GetStudentApplicationsAsync(
            int studentId
        )
        {
            var applications = await _jobApplicationRepository.GetByStudentIdAsync(studentId);
            return applications.Select(app => new JobApplicationResponseDto
            {
                Id = app.Id,
                StudentName = $"{app.Student.FirstName} {app.Student.LastName}",
                StudentEmail = app.Student.Email!,
                OfferTitle = app.JobOffer.Title,
                Status = app.Status.ToString(),
                ApplicationDate = app.CreatedAt,
                //CurriculumVitae = app.Student.CurriculumVitae,
                //MotivationLetter = app.Student.MotivationLetter,
            });
        }
    }
}
