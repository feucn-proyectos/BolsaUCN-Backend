using System.Security;
using backend.src.Application.DTOs.JobAplicationDTO;
using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.ApplicantsForAdminDTOs;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs.ApplicationsForOfferorDTOs;
using backend.src.Application.Events;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Domain.Options;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class OfferApplicationService : IOfferApplicationService
    {
        private readonly IOfferApplicationRepository _applicationRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IReviewService _reviewService;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageSize;

        public OfferApplicationService(
            IOfferApplicationRepository applicationRepository,
            IPublicationRepository publicationRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IReviewService reviewService,
            IFileService fileService,
            IConfiguration configuration
        )
        {
            _applicationRepository = applicationRepository;
            _userRepository = userRepository;
            _publicationRepository = publicationRepository;
            _notificationService = notificationService;
            _reviewService = reviewService;
            _fileService = fileService;
            _configuration = configuration;
            _defaultPageSize = _configuration.GetValue<int>("Pagination:DefaultPageSize");
        }

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
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(offerId);
            if (offer == null)
            {
                Log.Error("Oferta ID: {OfferId} no encontrada al intentar postular", offerId);
                throw new KeyNotFoundException("La oferta no existe");
            }
            else if (!offer.ApprovalStatus.Equals(ApprovalStatus.Aceptada))
            {
                Log.Warning(
                    "Usuario {UserId} intentó postular a oferta inactiva {OfferId}",
                    applicantId,
                    offerId
                );
                throw new InvalidOperationException("No puedes postular a una oferta inactiva");
            }
            // Verficar que el usuario no postule a su propia oferta
            if (offer.UserId == user.Id)
            {
                Log.Error(
                    "Usuario {UserId} intentó postular a su propia oferta {OfferId}",
                    user.Id,
                    offerId
                );
                throw new InvalidOperationException("No puedes postular a tu propia oferta");
            }
            // Verifiar que la oferta tenga espacios abiertos
            if (offer.AvailableSlots <= 0)
            {
                Log.Error(
                    "Usuario {UserId} intentó postular a oferta {OfferId} sin espacios disponibles",
                    user.Id,
                    offerId
                );
                throw new InvalidOperationException("No hay espacios disponibles en esta oferta");
            }
            // Verificar que el usuario tenga CV si es necesario
            if (offer.IsCvRequired && user.CV == null)
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
            // Validar que el usuario no tenga más de 3 reseñas pendientes como postulante
            var pendingReviewsCount = await _reviewService.GetPendingReviewsCountAsync(
                user,
                RoleNames.Applicant
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

            // Verificar que no haya postulado anteriormente chequeando
            var hasAppliedResult = await _applicationRepository.ExistsByApplicantIdAndOfferId(
                user.Id,
                offerId
            );
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

            bool result = await _applicationRepository.AddAsync(jobApplication);
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

        public async Task<string> CancelApplicationAsync(int userId, int applicationId)
        {
            // Validar al usuario
            bool userExists = await _userRepository.ExistsByIdAsync(userId);
            if (!userExists)
            {
                Log.Error(
                    "Usuario ID: {UserId} no encontrado al intentar cancelar postulación ID: {ApplicationId}",
                    userId,
                    applicationId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            // Obtener la postulación
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no encontrada al intentar cancelar",
                    applicationId
                );
                throw new KeyNotFoundException("La postulación no existe");
            }
            // Verificar que la postulación pertenezca al usuario
            if (application.StudentId != userId)
            {
                Log.Error(
                    "Usuario ID: {UserId} intentó cancelar postulación ID: {ApplicationId} que no le pertenece",
                    userId,
                    applicationId
                );
                throw new UnauthorizedAccessException(
                    "No tienes permiso para cancelar esta postulación"
                );
            }
            // Validar estado de postulacion
            if (application.Status != ApplicationStatus.Pendiente)
            {
                Log.Error(
                    "Usuario ID: {UserId} intentó cancelar postulación ID: {ApplicationId} con estado {Status}",
                    userId,
                    applicationId,
                    application.Status
                );
                throw new InvalidOperationException(
                    "Solo se pueden cancelar las postulaciones en estado Pendiente"
                );
            }

            application.Status = ApplicationStatus.CanceladaPorPostulante;
            bool updateResult = await _applicationRepository.UpdateAsync(application);
            if (!updateResult)
            {
                Log.Error(
                    "Error al cancelar postulación ID: {ApplicationId} para usuario ID: {UserId}",
                    applicationId,
                    userId
                );
                throw new Exception("No se pudo cancelar la postulación");
            }

            Log.Information(
                "Usuario ID: {UserId} canceló postulación ID: {ApplicationId}",
                userId,
                applicationId
            );

            return "Tu postulación ha sido cancelada exitosamente.";
        }

        public async Task<(
            MemoryStream fileStream,
            string contentType,
            string fileName
        )> GetApplicantCVAsync(int offerId, int applicationId, int offerorId)
        {
            // Validar oferente
            bool offerorExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!offerorExists)
            {
                Log.Error(
                    "Usuario ID: {OfferorId} no encontrado al intentar descargar CV de postulación ID: {ApplicationId}",
                    offerorId,
                    applicationId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            bool isOfferor = await _userRepository.CheckRoleAsync(offerorId, RoleNames.Offeror);
            if (!isOfferor)
            {
                Log.Error(
                    "Usuario ID: {OfferorId} sin rol de Offeror intentó descargar CV de postulación ID: {ApplicationId}",
                    offerorId,
                    applicationId
                );
                throw new SecurityException("El usuario no tiene permisos de Oferente");
            }

            // Validar que la oferta existe y pertenece al oferente
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(offerId);
            if (offer == null)
            {
                Log.Error(
                    "Oferta ID: {OfferId} no encontrada al intentar descargar CV de postulación ID: {ApplicationId}",
                    offerId,
                    applicationId
                );
                throw new KeyNotFoundException("La oferta no existe");
            }
            if (offer.UserId != offerorId)
            {
                Log.Error(
                    "Usuario ID: {OfferorId} intentó descargar CV de postulación ID: {ApplicationId} de una oferta que no le pertenece",
                    offerorId,
                    applicationId
                );
                throw new SecurityException(
                    "El usuario no tiene permisos para descargar el CV de esta postulación"
                );
            }

            // Validar que la solicitud existe y pertenece a la oferta
            JobApplication? application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no encontrada al intentar descargar CV",
                    applicationId
                );
                throw new KeyNotFoundException("La postulación no existe");
            }
            if (application.JobOfferId != offerId)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no pertenece a la oferta ID: {OfferId}",
                    applicationId,
                    offerId
                );
                throw new SecurityException("La postulación no pertenece a esta oferta");
            }

            // Revisar el que postulante tenga un CV (No deberia pasar si se validó correctamente al crear la postulación, pero se agrega esta verificación por seguridad adicional antes de intentar acceder al CV)
            bool hasCV = application.Student!.CV != null;
            if (!hasCV)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no tiene CV asociado al intentar descargar",
                    applicationId
                );
                throw new InvalidOperationException("El postulante no ha subido un CV");
            }

            User applicant = application.Student!;
            // Acceso a el archivo por url firmada
            string signedUrl = await _fileService.BuildSignedUrlForCVAsync(applicant.CV!.PublicId);

            // Descarga del archivo desde el servidor de hosting
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(signedUrl);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error(
                    "Error al descargar el CV del estudiante con ID: {UserId} desde el servidor de hosting. StatusCode: {StatusCode}",
                    applicant.Id,
                    response.StatusCode
                );
                throw new Exception(
                    "Error al descargar el CV del estudiante desde el servidor de hosting"
                );
            }
            var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            string fileName = $"CV_{applicant.FirstName}_{applicant.LastName}.pdf"
                .Replace(" ", "_")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");

            Log.Information(
                "CV del estudiante con ID: {UserId} descargado exitosamente desde el servidor de hosting",
                applicant.Id
            );
            return (memoryStream, "application/pdf", fileName);
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
                await _applicationRepository.GetByApplicantIdFilteredAsync(
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

        public async Task<ApplicationsForAdminDTO> GetApplicationsByOfferIdForAdminAsync(
            int offerId,
            int adminId,
            ApplicationsForAdminSearchParamsDTO searchParams
        )
        {
            // Validar usuario
            bool adminExists = await _userRepository.ExistsByIdAsync(adminId);
            if (!adminExists)
            {
                Log.Error(
                    "Usuario ID: {AdminId} no encontrado al obtener postulaciones de oferta ID: {OfferId} para admin",
                    adminId,
                    offerId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            // Validar que la oferta existe
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(offerId);
            if (offer == null)
            {
                Log.Error(
                    "Oferta ID: {OfferId} no encontrada al intentar obtener postulaciones para admin",
                    offerId
                );
                throw new KeyNotFoundException("La oferta no existe");
            }
            // Obtener postulaciones
            var (applications, totalCount) =
                await _applicationRepository.GetAllByOfferIdForAdminAsync(offerId, searchParams);
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
            ApplicationsForAdminDTO applicationsDTO = new ApplicationsForAdminDTO
            {
                Applications = applications.Adapt<List<ApplicationForAdminDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = pageSize,
                CurrentPage = currentPage,
            };
            return applicationsDTO;
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
            var application = await _applicationRepository.GetByIdAsync(applicationId);
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
            var offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(
                application.JobOfferId,
                new PublicationQueryOptions { IncludeUser = true }
            );
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
            var offerorName = user.UserType switch
            {
                UserType.Empresa => user.FirstName ?? "Empresa desconocida",
                UserType.Administrador => "Administrador",
                _ => (user.FirstName + " " + user.LastName) ?? "Nombre desconocido",
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
            detailsDTO.OfferorName = offerorName;
            detailsDTO.StatusMessage = statusMessage;

            return detailsDTO;
        }

        public async Task<string> UpdateApplicationStatusByOfferorAsync(
            int offerorId,
            int applicationId,
            int offerId,
            UpdateApplicationStatusDTO newStatus
        )
        {
            // Validacion de oferente
            bool offerorExists = await _userRepository.ExistsByIdAsync(offerorId);
            if (!offerorExists)
            {
                Log.Error(
                    "Usuario ID: {OfferorId} no encontrado al actualizar estado de postulación",
                    offerorId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            bool isOfferor = await _userRepository.CheckRoleAsync(offerorId, RoleNames.Offeror);
            if (!isOfferor)
            {
                Log.Error(
                    "Usuario ID: {OfferorId} sin rol de Offeror intentó actualizar estado de postulación",
                    offerorId
                );
                throw new SecurityException("El usuario no tiene permisos de Oferente");
            }
            bool isOwner = await _publicationRepository.CheckOwnershipAsync(offerorId, offerId);
            if (!isOwner)
            {
                Log.Error(
                    "Usuario ID: {OfferorId} intentó actualizar postulación de oferta ID: {OfferId} que no le pertenece",
                    offerorId,
                    offerId
                );
                throw new SecurityException("No tienes permiso para actualizar esta postulación");
            }
            // Validacion de la solicitud
            JobApplication? application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no encontrada al actualizar estado",
                    applicationId
                );
                throw new KeyNotFoundException("La postulación no existe");
            }
            if (application.JobOfferId != offerId)
            {
                Log.Error(
                    "Postulación ID: {ApplicationId} no pertenece a oferta ID: {OfferId}",
                    applicationId,
                    offerId
                );
                throw new SecurityException("La postulación no pertenece a la oferta especificada");
            }
            if (application.StudentId == offerorId) // Edge case correspondiente a un estudiante que postula a su propia oferta (Estudiantes tienen rol de Applicant y Offeror)
            {
                Log.Error(
                    "Usuario ID: {OfferorId} intentó actualizar su propia postulación ID: {ApplicationId}",
                    offerorId,
                    applicationId
                );
                throw new SecurityException(
                    "No puedes actualizar el estado de tu propia postulación"
                );
            }
            // Parseo y validacion del nuevo estado
            bool parseSuccess = Enum.TryParse(
                newStatus.NewStatus,
                out ApplicationStatus parsedStatus
            );
            if (!parseSuccess || !Enum.IsDefined(parsedStatus))
            {
                Log.Error(
                    "Estado inválido {NewStatus} proporcionado para postulación ID: {ApplicationId}",
                    newStatus.NewStatus,
                    applicationId
                );
                throw new ArgumentException("El estado proporcionado no es válido");
            }
            // Chequeo de flujo de estados
            if (application.Status == parsedStatus)
            {
                Log.Warning(
                    "El estado de postulación ID: {ApplicationId} ya es {Status}",
                    applicationId,
                    parsedStatus
                );
                return "El estado de la postulación ya es el especificado"; // Edge case: no deberia ocurrir.
            }
            else if (application.Status != ApplicationStatus.Pendiente)
            {
                Log.Error(
                    "Intento de cambiar estado de postulación ID: {ApplicationId} desde {CurrentStatus} a {NewStatus} inválido",
                    applicationId,
                    application.Status,
                    parsedStatus
                );
                throw new InvalidOperationException(
                    "Solo se pueden actualizar postulaciones que están en estado Pendiente"
                );
            }
            // Verificar espacios disponibles en la oferta
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(offerId);
            if (offer == null)
            {
                Log.Error(
                    "Oferta ID: {OfferId} no encontrada al intentar actualizar estado de postulación",
                    offerId
                );
                throw new KeyNotFoundException("La oferta no existe");
            }
            if (offer.AvailableSlots <= 0 && parsedStatus == ApplicationStatus.Aceptada)
            {
                Log.Error(
                    "No hay espacios disponibles en la oferta ID: {OfferId} al intentar actualizar estado de postulación",
                    offerId
                );
                throw new InvalidOperationException("No hay espacios disponibles en la oferta");
            }
            application.Status = parsedStatus;
            if (parsedStatus == ApplicationStatus.Aceptada)
                offer.AvailableSlots -= 1; // Reducir espacios disponibles si se acepta la postulación
            bool applicationUpdateResult = await _applicationRepository.UpdateAsync(application);
            bool offerUpdateResult = await _publicationRepository.UpdateAsync(offer);
            if (!applicationUpdateResult || !offerUpdateResult)
            {
                Log.Error(
                    "Error al actualizar estado de postulación ID: {ApplicationId} para oferente ID: {OfferorId}",
                    applicationId,
                    offerorId
                );
                throw new Exception("No se pudo actualizar el estado de la postulación");
            }
            // Enviar notificación al postulante
            //TODO: Personalizar el mensaje de la notificación según el nuevo estado

            // TODO:Cerrar publicacion si no quedan espacios.
            if (offer.AvailableSlots <= 0)
            {
                offer.StartWork();
                await _publicationRepository.UpdateAsync(offer);
            }
            return "El estado de la postulación ha sido actualizado con éxito.";
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
            var application = await _applicationRepository.GetByIdAsync(applicationId);
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
            bool updateResult = await _applicationRepository.UpdateAsync(application);
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

        public async Task<ApplicationsForOfferorDTO> GetAllApplicationsByOfferIdAsync(
            int offerId,
            int offererId,
            ApplicationsForOfferorSearchParamsDTO searchParams
        )
        {
            // Validar usuario
            bool offererExists = await _userRepository.ExistsByIdAsync(offererId);
            if (!offererExists)
            {
                Log.Error(
                    "Usuario ID: {OffererId} no encontrado al obtener postulaciones de oferta ID: {OfferId}",
                    offererId,
                    offerId
                );
                throw new KeyNotFoundException("El usuario no existe");
            }
            // Validar que la oferta existe y pertenece al usuario
            Offer? offer = await _publicationRepository.GetPublicationByIdAsync<Offer>(offerId);
            if (offer == null)
            {
                Log.Error(
                    "Oferta ID: {OfferId} no encontrada al intentar obtener postulaciones",
                    offerId
                );
                throw new KeyNotFoundException("La oferta no existe");
            }
            else if (offer.UserId != offererId)
            {
                Log.Error(
                    "Usuario ID: {OffererId} intentó acceder a postulaciones de oferta ID: {OfferId} que no le pertenece",
                    offererId,
                    offerId
                );
                throw new SecurityException(
                    "No tienes permiso para ver las postulaciones de esta oferta"
                );
            }
            // Obtener postulaciones
            var (applications, totalCount) = await _applicationRepository.GetAllByOfferIdAsync(
                offerId,
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
            ApplicationsForOfferorDTO applicationsDTO = new ApplicationsForOfferorDTO
            {
                Applications = applications.Adapt<List<ApplicationForOfferorDTO>>(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = pageSize,
                CurrentPage = currentPage,
            };
            return applicationsDTO;
        }
    }
}
