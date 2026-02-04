using backend.src.Application.DTOs.OfferDTOs;
using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Serilog;

namespace backend.src.Application.Services.Implements;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public OfferService(
        IOfferRepository offerRepository,
        AppDbContext context,
        IEmailService emailService
    )
    {
        _offerRepository = offerRepository;
        _context = context;
        _emailService = emailService;
    }

    public async Task<Offer> GetByOfferIdAsync(int offerId)
    {
        Offer? offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            Log.Warning("Oferta con ID {OfferId} no encontrada", offerId);
            throw new KeyNotFoundException($"Offer with id {offerId} not found");
        }
        return offer;
    }

    public async Task<IEnumerable<OfferSummaryDto>> GetActiveOffersAsync()
    {
        Log.Information("Obteniendo todas las ofertas activas");
        // Debe traer User + Company/Individual (el repo debería hacer Include).
        var offers = await _offerRepository.GetAllActiveAsync();
        var list = offers.ToList();

        Log.Information("Se encontraron {Count} ofertas activas", list.Count);

        var result = list.Select(o =>
            {
                // Nombre de oferente
                var ownerName =
                    o.User?.UserType == UserType.Empresa
                        ? (o.User.FirstName ?? "Empresa desconocida")
                    : o.User?.UserType == UserType.Particular
                        ? $"{(o.User.FirstName ?? "").Trim()} {(o.User.LastName ?? "").Trim()}".Trim()
                    : (o.User?.UserName ?? "Nombre desconocido");

                var ownerRating = o.User?.Rating ?? 0.0f;
                return new OfferSummaryDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    CompanyName = ownerName, // si lo sigues usando en otros lados
                    OwnerName = ownerName, // lo que consume el front para “oferente”
                    OwnerRating = ownerRating,

                    Location = "Campus Antofagasta",

                    // 💰 y fechas para la tarjeta
                    Remuneration = o.Remuneration ?? 0,
                    DeadlineDate = o.ApplicationDeadline,
                    PublicationDate = o.CreatedAt,
                    OfferType = o.OfferType, // Trabajo / Voluntariado (enum)
                };
            })
            .ToList();

        return result;
    }

    public async Task<OfferDetailDto?> GetOfferDetailsAsync(int offerId)
    {
        Log.Information("Obteniendo detalles de la oferta ID: {OfferId}", offerId);

        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            Log.Warning("Oferta con ID {OfferId} no encontrada", offerId);
            throw new KeyNotFoundException($"Offer with id {offerId} not found");
        }

        // Nombre de oferente para detalles
        var ownerName =
            offer.User?.UserType == UserType.Empresa
                ? (offer.User.FirstName ?? "Empresa desconocida")
            : offer.User?.UserType == UserType.Particular
                ? $"{(offer.User.FirstName ?? "").Trim()} {(offer.User.LastName ?? "").Trim()}".Trim()
            : (offer.User?.UserName ?? "Nombre desconocido");

        var result = new OfferDetailDto
        {
            Id = offer.Id,
            Title = offer.Title,
            Description = offer.Description,
            CompanyName = ownerName,
            // si también quieres forzar aquí Antofagasta:
            Location = "Campus Antofagasta",

            PostDate = offer.CreatedAt,
            EndDate = offer.EndDate,
            Remuneration = (int)offer.Remuneration, // tu DTO usa int
            OfferType = offer.OfferType.ToString(),
            statusValidation = offer.ApprovalStatus,
        };

        Log.Information("Detalles de oferta ID: {OfferId} obtenidos exitosamente", offerId);
        return result;
    }

    public async Task PublishOfferAsync(int id)
    {
        var offer = await _context.Offers.FindAsync(id);
        if (offer == null)
            throw new KeyNotFoundException("Offer not found.");

        offer.IsOpen = true; // o Published / Active, según tu modelo
        _context.Offers.Update(offer);
        await _context.SaveChangesAsync();
    }

    public async Task RejectOfferAsync(int id)
    {
        var offer = await _context.Offers.FindAsync(id);
        if (offer == null)
            throw new KeyNotFoundException("Offer not found.");

        offer.IsOpen = false;
        _context.Offers.Update(offer);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PendingOffersForAdminDto>> GetPendingOffersAsync()
    {
        var offer = await _offerRepository.GetAllPendingOffersAsync();
        return offer
            .Select(o => new PendingOffersForAdminDto
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                Location = o.Location,
                PostDate = o.CreatedAt,
                Remuneration = o.Remuneration ?? 0,
                OfferType = o.OfferType,
            })
            .ToList();
    }

    public async Task<IEnumerable<OfferBasicAdminDto>> GetPublishedOffersAsync()
    {
        var offer = await _offerRepository.PublishedOffersAsync();
        var list = offer.ToList();
        var result = list.Select(o =>
            {
                var ownerName =
                    o.User?.UserType == UserType.Empresa
                        ? (o.User.FirstName ?? "Empresa desconocida")
                    : o.User?.UserType == UserType.Particular
                        ? $"{(o.User.FirstName ?? "").Trim()} {(o.User.LastName ?? "").Trim()}".Trim()
                    : (o.User?.UserName ?? "Nombre desconocido");
                return new OfferBasicAdminDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    CompanyName = ownerName,
                    PublicationDate = o.CreatedAt,
                    OfferType = o.OfferType,
                    Activa = o.IsOpen,
                    Remuneration = o.Remuneration ?? 0,
                };
            })
            .ToList();
        return result;
    }

    public async Task<OfferDetailsAdminDto> GetOfferDetailsForAdminManagement(int offerId)
    {
        Log.Information("Obteniendo detalles de la oferta ID: {OfferId}", offerId);
        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            Log.Warning("Oferta con ID {OfferId} no encontrada", offerId);
            throw new KeyNotFoundException($"Offer with id {offerId} not found");
        }
        var ownerName =
            offer.User?.UserType == UserType.Empresa
                ? (offer.User.FirstName ?? "Empresa desconocida")
            : offer.User?.UserType == UserType.Particular
                ? $"{(offer.User.FirstName ?? "").Trim()} {(offer.User.LastName ?? "").Trim()}".Trim()
            : (offer.User?.UserName ?? "Nombre desconocido");
        var imageForDTO = offer.Images.Select(i => i.Url).ToList();
        var result = new OfferDetailsAdminDto
        {
            Title = offer.Title,
            Description = offer.Description,
            Images = imageForDTO,
            CompanyName = ownerName,
            PublicationDate = offer.CreatedAt,
            EndDate = offer.EndDate,
            DeadlineDate = offer.ApplicationDeadline,
            Type = offer.PublicationType,
            Active = offer.IsOpen,
            statusValidation = offer.ApprovalStatus,
            Remuneration = offer.Remuneration ?? 0,
            OfferType = offer.OfferType,
            Location = offer.Location,
            ContactInfo = offer.AdditionalContactEmail,
            AboutMe = offer.User?.AboutMe,
            Rating = offer.User.Rating,
        };
        Log.Information("Detalles de oferta ID: {OfferId} obtenidos exitosamente", offerId);
        return result;
    }

    public async Task GetOfferForAdminToClose(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
        }
        if (!offer.IsOpen)
        {
            throw new InvalidOperationException($"La oferta con id {offerId} ya ha sido cerrada.");
        }
        offer.IsOpen = false;
        await _offerRepository.UpdateOfferAsync(offer);

        Log.Information(
            "Oferta ID: {OfferId} cerrada. Reviews se crearán automáticamente.",
            offerId
        );

        if (offer.User?.Email != null)
        {
            await _emailService.SendPublicationStatusChangeEmailAsync(
                offer.Id,
                offer.User.Email,
                offer.Title,
                "Cerrada (Finalizada)" // Estado a mostrar en el email
            );
        }
    }

    public async Task<OfferDetailValidationDto> GetOfferDetailForOfferValidationAsync(int id)
    {
        Log.Information("Obteniendo detalles de la oferta ID: {OfferId}", id);
        var offer = await _offerRepository.GetByIdAsync(id);
        if (offer == null)
        {
            Log.Warning("Oferta con ID {OfferId} no encontrada", id);
            throw new KeyNotFoundException($"Offer with id {id} not found");
        }
        var ownerName =
            offer.User?.UserType == UserType.Empresa
                ? (offer.User.FirstName ?? "Empresa desconocida")
            : offer.User?.UserType == UserType.Particular
                ? $"{(offer.User.FirstName ?? "").Trim()} {(offer.User.LastName ?? "").Trim()}".Trim()
            : (offer.User?.UserName ?? "Nombre desconocido");
        var imageForDTO = offer.Images.Select(i => i.Url).ToList();
        return new OfferDetailValidationDto
        {
            Title = offer.Title,
            Images = imageForDTO,
            Description = offer.Description,
            CompanyName = ownerName,
            CorreoContacto = offer.User?.Email,
            TelefonoContacto = offer.User?.PhoneNumber,
            PublicationDate = offer.CreatedAt,
            DeadlineDate = offer.ApplicationDeadline,
            EndDate = offer.EndDate,
            Remuneration = offer.Remuneration ?? 0,
            OfferType = offer.OfferType,
            ContactInfo = offer.AdditionalContactEmail,
            AboutMe = offer.User?.AboutMe,
            Rating = offer.User.Rating,
        };
    }

    public async Task GetOfferForAdminToPublish(int id)
    {
        var offer = await _offerRepository.GetByIdAsync(id);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");
        }
        if (offer.ApprovalStatus != ApprovalStatus.EnProceso)
        {
            throw new InvalidOperationException(
                $"La oferta con ID {id} ya fue {offer.ApprovalStatus}. No se puede publicar."
            );
        }
        offer.IsOpen = true;
        offer.ApprovalStatus = ApprovalStatus.Aceptado;
        await _offerRepository.UpdateOfferAsync(offer);

        if (offer.User?.Email != null)
        {
            await _emailService.SendPublicationStatusChangeEmailAsync(
                offer.Id,
                offer.User.Email,
                offer.Title,
                "Publicada"
            );
        }
    }

    public async Task GetOfferForAdminToReject(int id)
    {
        var offer = await _offerRepository.GetByIdAsync(id);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");
        }
        if (offer.ApprovalStatus != ApprovalStatus.EnProceso)
        {
            throw new InvalidOperationException(
                $"La oferta con ID {id} ya fue {offer.ApprovalStatus}. No se puede rechazar."
            );
        }
        offer.IsOpen = false;
        offer.ApprovalStatus = ApprovalStatus.Rechazado;
        await _offerRepository.UpdateOfferAsync(offer);

        if (offer.User?.Email != null)
        {
            await _emailService.SendPublicationStatusChangeEmailAsync(
                offer.Id,
                offer.User.Email,
                offer.Title,
                "Rechazada"
            );
        }
    }

    public async Task<OfferDetailDto> GetOfferDetailForOfferer(int id, string userId)
    {
        var offer = await _offerRepository.GetByIdAsync(id);

        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");
        }

        if (!int.TryParse(userId, out int parsedUserId))
        {
            throw new UnauthorizedAccessException("El ID de usuario es inválido.");
        }

        if (offer.UserId != parsedUserId)
        {
            // Lanza 404 para no revelar que la oferta existe pero no es suya
            throw new KeyNotFoundException($"La oferta con id {id} no fue encontrada.");

            // throw new UnauthorizedAccessException("No tienes permiso para ver esta oferta.");
        }
        var offerDetailDto = offer.Adapt<OfferDetailDto>();

        return offerDetailDto;
    }

    public async Task ClosePublishedOfferAsync(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
        }
        if (!offer.IsOpen)
        {
            throw new InvalidOperationException($"La oferta con id {offerId} ya ha sido cerrada.");
        }
        offer.IsOpen = false;
        await _offerRepository.UpdateOfferAsync(offer);

        Log.Information(
            "Oferta ID: {OfferId} cerrada por el oferente. Reviews se crearán automáticamente.",
            offerId
        );

        if (offer.User?.Email != null)
        {
            await _emailService.SendPublicationStatusChangeEmailAsync(
                offer.Id,
                offer.User.Email,
                offer.Title,
                "Cerrada (Finalizada)" // Estado a mostrar en el email
            );
        }
    }

    public async Task ClosePublishedOfferForOffererAsync(int offerId, int offererUserId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId);
        if (offer == null)
        {
            throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
        }
        // Validar propiedad: lanzar 404 para no revelar la existencia
        if (offer.UserId != offererUserId)
        {
            throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
        }

        if (!offer.IsOpen)
        {
            throw new InvalidOperationException($"La oferta con id {offerId} ya ha sido cerrada.");
        }

        // El estado de publicación debe ser Publicado para poder cerrarse
        if (offer.ApprovalStatus != ApprovalStatus.Aceptado)
        {
            throw new InvalidOperationException(
                $"La oferta con ID {offerId} está {offer.ApprovalStatus}. Solo las publicaciones activas pueden ser cerradas."
            );
        }

        offer.IsOpen = false;
        offer.ApprovalStatus = ApprovalStatus.Cerrado;
        await _offerRepository.UpdateOfferAsync(offer);

        Log.Information(
            "Oferta ID: {OfferId} cerrada por el oferente {OffererId}. Reviews se crearán automáticamente.",
            offerId,
            offererUserId
        );

        if (offer.User?.Email != null)
        {
            await _emailService.SendPublicationStatusChangeEmailAsync(
                offer.Id,
                offer.User.Email,
                offer.Title,
                "Cerrada (Finalizada)" // Estado a mostrar en el email
            );
        }
    }
}
