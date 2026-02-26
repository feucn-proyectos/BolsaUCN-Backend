using backend.src.Application.DTOs.ReviewDTO.ReviewReportDTO;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio para la generación de documentos PDF con información de reviews
    /// </summary>
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly IUserRepository _userRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IReviewService _reviewService;

        public PdfGeneratorService(
            IUserRepository userRepository,
            IReviewRepository reviewRepository,
            IReviewService reviewService
        )
        {
            _userRepository = userRepository;
            _reviewRepository = reviewRepository;
            _reviewService = reviewService;

            // Configuración de licencia QuestPDF (Community - gratuita para uso educativo)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Genera un PDF con todas las reviews del usuario
        /// </summary>
        public async Task<byte[]> GenerateUserReviewsPdfAsync(
            int requestingUserId,
            int? targetUserId = null
        )
        {
            // Validar usuario solicitante
            bool requestingUserExists = await _userRepository.ExistsByIdAsync(requestingUserId);
            if (!requestingUserExists)
            {
                Log.Error(
                    "Usuario solicitante con ID {RequestingUserId} no encontrado para generación de PDF",
                    requestingUserId
                );
                throw new KeyNotFoundException(
                    $"Usuario solicitante con ID {requestingUserId} no encontrado"
                );
            }
            if (targetUserId != null)
            {
                bool isAdmin = await _userRepository.CheckRoleAsync(
                    requestingUserId,
                    RoleNames.Admin
                );
                if (!isAdmin)
                {
                    Log.Error(
                        "Usuario con ID {RequestingUserId} no tiene permisos de administrador para generar PDF de otro usuario",
                        requestingUserId
                    );
                    throw new UnauthorizedAccessException(
                        $"Usuario con ID {requestingUserId} no tiene permisos de administrador para generar PDF de otro usuario"
                    );
                }
            }
            targetUserId ??= requestingUserId;
            // 1. Obtener usuario
            User? user = await _userRepository.GetByIdAsync((int)targetUserId);
            if (user == null)
            {
                Log.Error(
                    "Usuario con ID {UserId} no encontrado para generación de PDF",
                    targetUserId
                );
                throw new KeyNotFoundException($"Usuario con ID {targetUserId} no encontrado");
            }

            List<Review> reviews = await _reviewRepository.GetAllByUserIdAsync((int)targetUserId);

            // 5. Construir DTO para el reporte
            ReviewReportDTO reportData = new()
            {
                UserName = user.FullName ?? "Usuario",
                UserEmail = user.Email ?? "N/A",
                AverageRating = user.Rating, // Obtenido directamente del usuario
                TotalReviews = reviews.Count,
                GeneratedAt = DateTime.UtcNow,
                Reviews = reviews
                    .Select(r => new ReviewDetailDTO
                    {
                        ReviewId = r.Id,
                        IsApplicant = r.ApplicantId == user.Id,
                        PublicationTitle =
                            r.Application?.JobOffer!.Title ?? "Publicación no disponible",
                        Rating =
                            r.ApplicantId == user.Id
                                ? r.OfferorRatingOfApplicant
                                : r.ApplicantRatingOfOfferor,
                        Comment =
                            r.ApplicantId == user.Id
                                ? r.OfferorCommentForApplicant
                                : r.ApplicantCommentForOfferor,
                        ReviewDate = r.CreatedAt,
                        ReviewerName =
                            r.ApplicantId == user.Id
                                ? (r.Offeror?.FullName ?? "Oferente")
                                : (r.Applicant?.FullName ?? "Estudiante"),
                        IsOnTime = r.ApplicantId == user.Id ? r.IsOnTime : null,
                        IsPresentable = r.ApplicantId == user.Id ? r.IsPresentable : null,
                        IsRespectful = r.ApplicantId == user.Id ? r.IsRespectful : null,
                    })
                    .ToList(),
            };

            // 6. Generar PDF
            return GeneratePdfDocument(reportData);
        }

        /// <summary>
        /// Genera el documento PDF con QuestPDF
        /// </summary>
        private byte[] GeneratePdfDocument(ReviewReportDTO data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header
                    page.Header().Element(c => ComposeHeader(c, data));

                    // Content
                    page.Content().Element(c => ComposeContent(c, data));

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text($"Generado el: {data.GeneratedAt:dd/MM/yyyy HH:mm} | BolsaFEUCN")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Compone el header del PDF
        /// </summary>
        private void ComposeHeader(IContainer container, ReviewReportDTO data)
        {
            container.Column(column =>
            {
                column
                    .Item()
                    .Text("Reporte de Calificaciones")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Text(data.UserName).FontSize(14).SemiBold();

                column.Item().Text(data.UserEmail).FontSize(10).FontColor(Colors.Grey.Darken1);

                column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
            });
        }

        /// <summary>
        /// Compone el contenido principal del PDF
        /// </summary>
        private void ComposeContent(IContainer container, ReviewReportDTO data)
        {
            container
                .PaddingVertical(20)
                .Column(column =>
                {
                    // Sección de resumen
                    column
                        .Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(col =>
                                {
                                    col.Item().Text("Promedio General").FontSize(12).SemiBold();
                                    col.Item()
                                        .Text($"{data.AverageRating?.ToString("F2") ?? "n/a "}/6.0")
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(GetRatingColor(data.AverageRating));
                                });

                            row.RelativeItem()
                                .Column(col =>
                                {
                                    col.Item().Text("Total de Reviews").FontSize(12).SemiBold();
                                    col.Item()
                                        .Text(data.TotalReviews.ToString())
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(Colors.Blue.Medium);
                                });
                        });

                    column
                        .Item()
                        .PaddingTop(20)
                        .Text("Detalle de Calificaciones")
                        .FontSize(14)
                        .SemiBold();

                    // Verificar si hay reviews
                    if (!data.Reviews.Any())
                    {
                        column
                            .Item()
                            .PaddingTop(20)
                            .Text("No hay calificaciones registradas.")
                            .FontSize(12)
                            .Italic()
                            .FontColor(Colors.Grey.Medium);
                    }
                    else
                    {
                        // Lista de reviews
                        foreach (var review in data.Reviews)
                        {
                            column.Item().PaddingTop(15).Element(c => ComposeReviewCard(c, review));
                        }
                    }
                });
        }

        /// <summary>
        /// Compone una tarjeta individual de review
        /// </summary>
        private void ComposeReviewCard(IContainer container, ReviewDetailDTO review)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(15)
                .Column(column =>
                {
                    // Título publicación
                    column.Item().Text(review.PublicationTitle).FontSize(12).SemiBold();

                    // Rating
                    if (review.Rating.HasValue)
                    {
                        column
                            .Item()
                            .PaddingTop(5)
                            .Row(row =>
                            {
                                row.AutoItem().Text("Calificación: ");
                                row.AutoItem()
                                    .Text($"{review.Rating.Value}/6")
                                    .Bold()
                                    .FontColor(GetRatingColor(review.Rating.Value));
                            });
                    }
                    else
                    {
                        column
                            .Item()
                            .PaddingTop(5)
                            .Text("Calificación: Sin calificar")
                            .FontColor(Colors.Grey.Medium)
                            .Italic();
                    }

                    // Comentario
                    if (!string.IsNullOrEmpty(review.Comment))
                    {
                        column
                            .Item()
                            .PaddingTop(5)
                            .Row(row =>
                            {
                                row.AutoItem().Text("Comentario: ").FontSize(10);
                                row.AutoItem().Text(review.Comment).FontSize(10).Italic();
                            });
                    }

                    // Campos específicos para estudiantes
                    if (review.IsApplicant)
                    {
                        column
                            .Item()
                            .PaddingTop(5)
                            .Row(row =>
                            {
                                if (review.IsOnTime.HasValue)
                                {
                                    row.AutoItem().Text("• Puntualidad: ").FontSize(9);
                                    row.AutoItem()
                                        .Text(review.IsOnTime.Value ? "Sí" : "No")
                                        .FontSize(9)
                                        .FontColor(
                                            review.IsOnTime.Value
                                                ? Colors.Green.Medium
                                                : Colors.Red.Medium
                                        );
                                    row.AutoItem().PaddingLeft(15);
                                }

                                if (review.IsPresentable.HasValue)
                                {
                                    row.AutoItem().Text("• Presentación: ").FontSize(9);
                                    row.AutoItem()
                                        .Text(review.IsPresentable.Value ? "Buena" : "Regular")
                                        .FontSize(9)
                                        .FontColor(
                                            review.IsPresentable.Value
                                                ? Colors.Green.Medium
                                                : Colors.Orange.Medium
                                        );
                                }
                            });

                        if (review.IsRespectful.HasValue)
                        {
                            column
                                .Item()
                                .PaddingTop(3)
                                .Row(row =>
                                {
                                    row.AutoItem().Text("• Respeto al oferente: ").FontSize(9);
                                    row.AutoItem()
                                        .Text(review.IsRespectful.Value ? "Sí" : "No")
                                        .FontSize(9)
                                        .FontColor(
                                            review.IsRespectful.Value
                                                ? Colors.Green.Medium
                                                : Colors.Red.Medium
                                        );
                                });
                        }
                    }

                    // Revisor y fecha
                    column
                        .Item()
                        .PaddingTop(5)
                        .Text($"Por: {review.ReviewerName} | {review.ReviewDate:dd/MM/yyyy}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
        }

        /// <summary>
        /// Obtiene el color según el rating
        /// </summary>
        private static string GetRatingColor(float? rating)
        {
            if (rating >= 5.5f)
                return Colors.Green.Darken2;
            if (rating >= 4.5f)
                return Colors.Green.Medium;
            if (rating >= 4.0f)
                return Colors.Blue.Medium;
            if (rating >= 3.0f)
                return Colors.Orange.Medium;
            if (rating == null)
                return Colors.Grey.Medium;
            return Colors.Red.Medium;
        }

        /// <summary>
        /// Genera un PDF con todas las reviews del sistema
        /// </summary>
        public async Task<byte[]> GenerateSystemReviewsPdfAsync(int requestingUserId)
        {
            // Validar administrador
            bool adminExists = await _userRepository.ExistsByIdAsync(requestingUserId);
            if (!adminExists)
            {
                Log.Error(
                    "Administrador con ID {AdminId} no encontrado para generación de PDF",
                    requestingUserId
                );
                throw new KeyNotFoundException(
                    $"Administrador con ID {requestingUserId} no encontrado"
                );
            }
            bool isAdmin = await _userRepository.CheckRoleAsync(requestingUserId, RoleNames.Admin);
            if (!isAdmin)
            {
                Log.Error(
                    "Usuario con ID {AdminId} no tiene permisos de administrador para generación de PDF",
                    requestingUserId
                );
                throw new UnauthorizedAccessException(
                    $"Usuario con ID {requestingUserId} no tiene permisos de administrador"
                );
            }

            // Obtener todas las reviews del sistema con includes
            List<Review> reviews = await _reviewRepository.GetAllForAdminAsync();

            // 2. Calcular estadísticas del sistema
            int totalReviews = reviews.Count;

            // Contar usuarios únicos que tienen reviews
            var usersWithReviews = reviews
                .SelectMany(r => new[] { r.ApplicantId, r.OfferorId })
                .Distinct()
                .Count();

            // 3. Construir DTO para el reporte
            SystemReviewReportDTO reportData = new()
            {
                Reviews = reviews.Adapt<List<SystemReviewDetailDTO>>(),
                TotalReviews = totalReviews,
                TotalUsersWithReviews = usersWithReviews,
                GeneratedAt = DateTime.UtcNow,
            };

            // 4. Generar PDF
            return GenerateSystemPdfDocument(reportData);
        }

        /// <summary>
        /// Genera el documento PDF del sistema con QuestPDF
        /// </summary>
        private byte[] GenerateSystemPdfDocument(SystemReviewReportDTO data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header
                    page.Header().Element(c => ComposeSystemHeader(c, data));

                    // Content
                    page.Content().Element(c => ComposeSystemContent(c, data));

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(
                            $"Generado el: {data.GeneratedAt:dd/MM/yyyy HH:mm} | BolsaFEUCN - Reporte del Sistema"
                        )
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Compone el header del PDF del sistema
        /// </summary>
        private void ComposeSystemHeader(IContainer container, SystemReviewReportDTO data)
        {
            container.Column(column =>
            {
                column
                    .Item()
                    .Text("Reporte General del Sistema")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column
                    .Item()
                    .PaddingTop(5)
                    .Text("BolsaFEUCN - Todas las Calificaciones")
                    .FontSize(14)
                    .SemiBold();

                column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
            });
        }

        /// <summary>
        /// Compone el contenido principal del PDF del sistema
        /// </summary>
        private void ComposeSystemContent(IContainer container, SystemReviewReportDTO data)
        {
            container
                .PaddingVertical(20)
                .Column(column =>
                {
                    // Sección de resumen general
                    column
                        .Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(col =>
                                {
                                    col.Item().Text("Total de Reviews").FontSize(12).SemiBold();
                                    col.Item()
                                        .Text(data.TotalReviews.ToString())
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(Colors.Blue.Medium);
                                });

                            row.RelativeItem()
                                .Column(col =>
                                {
                                    col.Item().Text("Usuarios con Reviews").FontSize(12).SemiBold();
                                    col.Item()
                                        .Text(data.TotalUsersWithReviews.ToString())
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(Colors.Green.Medium);
                                });
                        });

                    column
                        .Item()
                        .PaddingTop(20)
                        .Text("Detalle de Todas las Calificaciones (Más Recientes Primero)")
                        .FontSize(14)
                        .SemiBold();

                    // Verificar si hay reviews
                    if (!data.Reviews.Any())
                    {
                        column
                            .Item()
                            .PaddingTop(20)
                            .Text("No hay calificaciones registradas en el sistema.")
                            .FontSize(12)
                            .Italic()
                            .FontColor(Colors.Grey.Medium);
                    }
                    else
                    {
                        // Lista de reviews
                        foreach (var review in data.Reviews)
                        {
                            column
                                .Item()
                                .PaddingTop(15)
                                .Element(c => ComposeSystemReviewCard(c, review));
                        }
                    }
                });
        }

        /// <summary>
        /// Compone una tarjeta individual de review del sistema
        /// </summary>
        private void ComposeSystemReviewCard(IContainer container, SystemReviewDetailDTO review)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(15)
                .Column(column =>
                {
                    // Header con título y estado
                    column
                        .Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text(review.PublicationTitle)
                                .FontSize(12)
                                .SemiBold();

                            row.AutoItem()
                                .Text(review.IsCompleted ? "Completa" : "Incompleta")
                                .FontSize(9)
                                .FontColor(
                                    review.IsCompleted ? Colors.Green.Medium : Colors.Orange.Medium
                                )
                                .Italic();

                            if (review.IsClosed)
                            {
                                row.AutoItem()
                                    .PaddingLeft(5)
                                    .Text("Cerrada")
                                    .FontSize(9)
                                    .FontColor(Colors.Red.Medium)
                                    .Italic();
                            }
                        });

                    // Información de participantes
                    column
                        .Item()
                        .PaddingTop(5)
                        .Row(row =>
                        {
                            row.AutoItem()
                                .Text($"Estudiante: {review.ApplicantName}")
                                .FontSize(10)
                                .SemiBold();
                            row.AutoItem()
                                .PaddingLeft(15)
                                .Text($"Oferente: {review.OfferorName}")
                                .FontSize(10)
                                .SemiBold();
                        });

                    // Calificación para el Estudiante
                    column
                        .Item()
                        .PaddingTop(8)
                        .Border(1)
                        .BorderColor(Colors.Blue.Lighten3)
                        .Padding(8)
                        .Column(col =>
                        {
                            col.Item()
                                .Text("Calificación para el Estudiante")
                                .FontSize(10)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken1);

                            if (review.RatingForStudent.HasValue)
                            {
                                col.Item()
                                    .PaddingTop(3)
                                    .Row(r =>
                                    {
                                        r.AutoItem().Text("Rating: ");
                                        r.AutoItem()
                                            .Text($"{review.RatingForStudent.Value}/6")
                                            .Bold()
                                            .FontColor(
                                                GetRatingColor(review.RatingForStudent.Value)
                                            );
                                    });
                            }
                            else
                            {
                                col.Item()
                                    .PaddingTop(3)
                                    .Text("Sin calificar")
                                    .FontSize(9)
                                    .Italic()
                                    .FontColor(Colors.Grey.Medium);
                            }

                            if (!string.IsNullOrEmpty(review.CommentForStudent))
                            {
                                col.Item()
                                    .PaddingTop(3)
                                    .Text($"Comentario: {review.CommentForStudent}")
                                    .FontSize(9)
                                    .Italic();
                            }

                            // Campos específicos
                            if (
                                review.IsOnTime.HasValue
                                || review.IsPresentable.HasValue
                                || review.IsRespectful.HasValue
                            )
                            {
                                col.Item()
                                    .PaddingTop(3)
                                    .Row(r =>
                                    {
                                        if (review.IsOnTime.HasValue)
                                        {
                                            r.AutoItem().Text("Puntualidad: ");
                                            r.AutoItem()
                                                .Text(review.IsOnTime.Value ? "Sí" : "No")
                                                .FontColor(
                                                    review.IsOnTime.Value
                                                        ? Colors.Green.Medium
                                                        : Colors.Red.Medium
                                                );
                                            r.AutoItem().PaddingLeft(10);
                                        }

                                        if (review.IsPresentable.HasValue)
                                        {
                                            r.AutoItem().Text("Presentación: ");
                                            r.AutoItem()
                                                .Text(
                                                    review.IsPresentable.Value ? "Buena" : "Regular"
                                                )
                                                .FontColor(
                                                    review.IsPresentable.Value
                                                        ? Colors.Green.Medium
                                                        : Colors.Orange.Medium
                                                );
                                        }
                                    });

                                if (review.IsRespectful.HasValue)
                                {
                                    col.Item()
                                        .PaddingTop(2)
                                        .Row(r =>
                                        {
                                            r.AutoItem().Text("Respeto al oferente: ");
                                            r.AutoItem()
                                                .Text(review.IsRespectful.Value ? "Sí" : "No")
                                                .FontColor(
                                                    review.IsRespectful.Value
                                                        ? Colors.Green.Medium
                                                        : Colors.Red.Medium
                                                );
                                        });
                                }
                            }
                        });

                    // Calificación para el Oferente
                    column
                        .Item()
                        .PaddingTop(5)
                        .Border(1)
                        .BorderColor(Colors.Green.Lighten3)
                        .Padding(8)
                        .Column(col =>
                        {
                            col.Item()
                                .Text("Calificación para el Oferente")
                                .FontSize(10)
                                .SemiBold()
                                .FontColor(Colors.Green.Darken1);

                            if (review.RatingForOfferor.HasValue)
                            {
                                col.Item()
                                    .PaddingTop(3)
                                    .Row(r =>
                                    {
                                        r.AutoItem().Text("Rating: ");
                                        r.AutoItem()
                                            .Text($"{review.RatingForOfferor.Value}/6")
                                            .Bold()
                                            .FontColor(
                                                GetRatingColor(review.RatingForOfferor.Value)
                                            );
                                    });
                            }
                            else
                            {
                                col.Item()
                                    .PaddingTop(3)
                                    .Text("Sin calificar")
                                    .FontSize(9)
                                    .Italic()
                                    .FontColor(Colors.Grey.Medium);
                            }

                            if (!string.IsNullOrEmpty(review.CommentForOfferor))
                            {
                                col.Item()
                                    .PaddingTop(3)
                                    .Text($"Comentario: {review.CommentForOfferor}")
                                    .FontSize(9)
                                    .Italic();
                            }
                        });

                    // Fecha
                    column
                        .Item()
                        .PaddingTop(5)
                        .Text($"Fecha: {review.ReviewDate:dd/MM/yyyy HH:mm}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
        }
    }
}
