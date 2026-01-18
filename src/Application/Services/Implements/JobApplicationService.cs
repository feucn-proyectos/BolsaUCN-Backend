using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.Events;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IOfferService _offerService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IReviewService _reviewService;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public JobApplicationService(
            IJobApplicationRepository jobApplicationRepository,
            IOfferService offerService,
            IUserService userService,
            INotificationService notificationService,
            IReviewService reviewService,
            IConfiguration configuration
        )
        {
            _jobApplicationRepository = jobApplicationRepository;
            _offerService = offerService;
            _userService = userService;
            _notificationService = notificationService;
            _reviewService = reviewService;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

        //! ALMOST COMPLETE
        //! MISSING FINAL MAPPING
        public async Task<JobApplicationResponseDto> CreateApplicationAsync(
            int applicantId,
            int offerId
        )
        {
            // Verifica que el usuario exista
            User user = await _userService.GetUserByIdAsync(applicantId);
            // Verificar que la oferta existe y está activa
            Offer? offer = await _offerService.GetByOfferIdAsync(offerId); //! IMPLEMENT GETBYOFFERASYNC IN THE SERVICE IF REQUIRED BY MORE METHODS
            if (offer == null)
            {
                Log.Error("Oferta ID: {OfferId} no encontrada al intentar postular", offerId);
                throw new KeyNotFoundException("La oferta no existe");
            }
            else if (!offer.IsValidated)
            {
                Log.Warning(
                    "Usuario {UserId} intentó postular a oferta inactiva {OfferId}",
                    applicantId,
                    offerId
                );
                throw new InvalidOperationException("No puedes postular a una oferta inactiva");
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

            // Validar elegibilidad del estudiante (incluye validación de CV si es obligatorio)
            if (!await ValidateStudentEligibilityAsync(user.Id, offer.IsCvRequired))
            {
                if (offer.IsCvRequired)
                {
                    throw new UnauthorizedAccessException(
                        "Esta oferta requiere CV. Por favor, sube tu CV en tu perfil antes de postular"
                    );
                }
                else
                {
                    throw new UnauthorizedAccessException(
                        "El estudiante no es elegible para postular"
                    );
                }
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

            // Crear la postulación (CV obligatorio, carta de motivación opcional del perfil)
            var jobApplication = new JobApplication
            {
                StudentId = user.Id,
                JobOfferId = offerId,
                Student = user,
                JobOffer = offer,
                Status = ApplicationStatus.Pendiente,
                CreatedAt = DateTime.UtcNow,
            };

            var createdApplication = await _jobApplicationRepository.AddAsync(jobApplication);

            return new JobApplicationResponseDto
            {
                Id = createdApplication.Id,
                StudentName = $"{user.FirstName} {user.LastName}",
                StudentEmail = user.Email!,
                OfferTitle = offer.Title,
                Status = createdApplication.Status.ToString(),
                ApplicationDate = createdApplication.CreatedAt,
                //CurriculumVitae = student.CurriculumVitae,
                //MotivationLetter = student.MotivationLetter, // Carta opcional del perfil
            };
        }

        public async Task<ApplicationsForApplicantDTO> GetUserApplicationsByIdAsync(
            int applicantId,
            SearchParamsDTO searchParams
        )
        {
            User applicant = await _userService.GetUserByIdAsync(applicantId);
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
                    $"Página solicitada {currentPage} fuera de rango. Total de páginas: {totalPages}. Se ajusta a la página 1."
                );
                currentPage = 1;
            }

            return new ApplicationsForApplicantDTO
            {
                Applications = applications.Adapt<List<ApplicationForApplicantDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = pageSize,
                CurrentPage = currentPage,
            };

            /**
            *? LEGACY RETURN LOGIC
            var applications = await _jobApplicationRepository.GetByStudentIdAsync(userId);
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
            */
        }

        public async Task<JobApplicationDetailDto?> GetApplicationDetailAsync(int applicationId)
        {
            // Obtener la postulación
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);

            if (application == null)
                return null;

            var offer = await _offerService.GetByOfferIdAsync(application.JobOfferId);

            if (offer == null)
                return null;

            var user = await _userService.GetUserByIdAsync(offer.UserId);

            var authorName =
                user?.UserType == UserType.Empresa ? (user.FirstName ?? "Empresa desconocida")
                : user?.UserType == UserType.Particular
                    ? $"{(user.FirstName ?? "").Trim()} {(user.LastName ?? "").Trim()}".Trim()
                : (user?.UserName ?? "Nombre desconocido");
            var statusMessage = application.Status switch
            {
                ApplicationStatus.Pendiente =>
                    "Su solicitud fue enviada con éxito; será contactado a la brevedad.",
                ApplicationStatus.Aceptada => "¡Felicidades! Tu solicitud ha sido aceptada.",
                ApplicationStatus.Rechazada => "Lamentablemente, tu solicitud ha sido rechazada.",
                _ => "",
            };

            return new JobApplicationDetailDto
            {
                Id = application.Id,
                OfferTitle = offer.Title,
                CompanyName = authorName,
                ApplicationDate = application.CreatedAt,
                PublicationDate = offer.CreatedAt,
                EndDate = offer.EndDate,
                Remuneration = offer.Remuneration,
                Description = offer.Description,
                Requirements = offer.Requirements,
                ContactInfo = offer.AdditionalContactInfo,
                Status = application.Status.ToString(),
                StatusMessage = statusMessage,
            };
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
            var offererUser = await _userService.GetUserByIdAsync(OwnnerUserId);
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

        public async Task<bool> ValidateStudentEligibilityAsync(
            int studentId,
            bool isCvRequired = true
        )
        {
            var student = await _userService.GetUserByIdAsync(studentId);

            if (student == null || student.UserType != UserType.Estudiante)
                return false;

            // Verificar que tenga correo institucional
            if (!student.Email!.EndsWith("@alumnos.ucn.cl"))
                return false;

            // Verificar que no esté bloqueado
            if (student.IsBlocked)
                return false;

            // Verificar que tenga CV SOLO si es obligatorio
            /* TODO: Falta revisar la logica de CV
            if (isCvRequired)
            {
                if (
                    student.Student == null
                    || string.IsNullOrEmpty(student.Student.CurriculumVitae)
                )
                    return false;
            }
            */

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
