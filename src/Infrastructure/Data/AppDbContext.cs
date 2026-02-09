using backend.src.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace backend.src.Infrastructure.Data
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación
    /// Hereda de IdentityDbContext para incluir las funcionalidades de autenticación
    /// </summary>
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets - Representan las tablas en la base de datos
        public DbSet<Image> Images { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Curriculum> CVs { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Whitelist> Whitelists { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<BuySell> BuySells { get; set; }
        public DbSet<NotificationDTO> Notifications { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewChecklistValues> ReviewChecklistValues { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }

        // public DbSet<Review> Reviews { get; set; } // Desactivado temporalmente

        /// <summary>
        /// Configura las relaciones entre entidades y otras configuraciones de EF Core
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relaciones de JobApplication (Postulación a oferta)
            // Un estudiante puede hacer muchas postulaciones
            builder
                .Entity<JobApplication>()
                .HasOne(ja => ja.Student)
                .WithMany()
                .HasForeignKey(ja => ja.StudentId);

            // Una oferta puede tener muchas postulaciones
            builder
                .Entity<JobApplication>()
                .HasOne(ja => ja.JobOffer)
                .WithMany(o => o.Applications)
                .HasForeignKey(ja => ja.JobOfferId);

            // Relaciones de Publication (clase base para ofertas y compra/venta)
            // Un usuario puede crear muchas publicaciones
            builder
                .Entity<Publication>()
                .HasOne(p => p.User)
                .WithMany(u => u.Publications)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones de Review
            // TODO: Implementar eliminacion de filas despues de probar que funciona todo.
            // Review - Publication (1:N)
            // Una publicación puede tener múltiples reviews (una por cada estudiante aceptado)
            builder
                .Entity<Review>()
                .HasOne(r => r.Publication)
                .WithMany()
                .HasForeignKey(r => r.PublicationId);
            // .OnDelete(DeleteBehavior.Cascade);

            // Review - Student (Many:1)
            // Un estudiante puede tener muchas reviews (como estudiante evaluado)
            builder
                .Entity<Review>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId);
            // .OnDelete(DeleteBehavior.Restrict);

            // Review - Offeror (Many:1)
            // Un proveedor puede tener muchas reviews (como oferente evaluado)
            builder
                .Entity<Review>()
                .HasOne(r => r.Offeror)
                .WithMany()
                .HasForeignKey(r => r.OfferorId)
                .OnDelete(DeleteBehavior.Restrict); // TODO: Revisar luego logica de Delete

            // Review - ReviewChecklistValues (1:1)
            // Cada review tiene exactamente un conjunto de valores de checklist
            builder.Entity<Review>().OwnsOne(r => r.ReviewChecklistValues);

            // Relaciones de UserImage (Imágenes de usuario)
            // Relación uno a uno entre GeneralUser y sus imágenes de perfil y banner
            builder
                .Entity<User>()
                .HasOne(gu => gu.ProfilePhoto)
                .WithOne()
                .HasForeignKey<User>(gu => gu.ProfilePhotoId)
                .OnDelete(DeleteBehavior.SetNull);
            // Relaciones de Documentos
            builder
                .Entity<User>()
                .HasOne(gu => gu.CV)
                .WithOne()
                .HasForeignKey<User>(gu => gu.CVId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- Fix: almacenar ApplicationStatus como string en la base de datos ---
            builder.Entity<JobApplication>().Property(j => j.Status).HasConversion<string>();
        }

        #region Override de SaveChangesAsync
        /// <summary>
        /// Override de SaveChangesAsync para:
        /// 1. Actualizar automáticamente UpdatedAt en entidades modificadas
        /// 2. Detectar publicaciones cerradas y crear reviews iniciales
        /// </summary>
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default
        )
        {
            Log.Information("Iniciando SaveChangesAsync con lógica extendida");
            // Actualizar UpdatedAt automáticamente
            UpdateTimestamps();

            // Logica de creación de reviews al cerrar publicaciones
            // Detectar publicaciones cerradas antes de guardar
            var closedPublications = DetectClosedPublications();
            // Guardar cambios
            var result = await base.SaveChangesAsync(cancellationToken);
            // Procesar publicaciones cerradas (crear reviews)
            await ProcessClosedPublicationsAsync(closedPublications, cancellationToken);
            return result;
        }

        #endregion
        #region Metodos auxiliares SaveChangesAsync
        /// <summary>
        /// Actualiza automáticamente el campo UpdatedAt de todas las entidades
        /// modificadas o agregadas que tengan esta propiedad.
        /// </summary>
        private void UpdateTimestamps()
        {
            var now = DateTime.UtcNow;
            // Obtener todas las entidades modificadas o agregadas que tengan la propiedad UpdatedAt
            var modifiedEntries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            foreach (var entry in modifiedEntries)
            {
                var updatedAtProperty = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.CanWrite)
                {
                    updatedAtProperty.SetValue(entry.Entity, now);
                }
            }
        }

        /// <summary>
        /// Detecta publicaciones que están siendo cerradas (IsOpen: true -> false).
        /// </summary>
        /// <returns>Lista de publicaciones cerradas</returns>
        private List<Publication> DetectClosedPublications()
        {
            return
            [
                .. ChangeTracker
                    .Entries<Publication>()
                    .Where(e =>
                        e.State == EntityState.Modified
                    )
                    .Select(e => e.Entity),
            ];
        }

        /// <summary>
        /// Procesa las publicaciones cerradas, creando reviews iniciales
        /// para todas las postulaciones aceptadas.
        /// </summary>
        private async Task ProcessClosedPublicationsAsync(
            List<Publication> closedPublications,
            CancellationToken cancellationToken
        )
        {
            if (closedPublications.Count == 0)
                return;
            await CreateReviewsForClosedPublicationsAsync(closedPublications, cancellationToken);
        }
        #endregion

        #region Creación de Reviews
        /// <summary>
        /// Crea reviews iniciales para todas las postulaciones aceptadas
        /// de las publicaciones que acaban de cerrarse.
        /// </summary>
        private async Task CreateReviewsForClosedPublicationsAsync(
            List<Publication> closedPublications,
            CancellationToken cancellationToken
        )
        {
            Log.Information(
                closedPublications.Count == 0
                    ? "No hay publicaciones cerradas para procesar reviews"
                    : $"Procesando {closedPublications.Count} publicaciones cerradas para crear reviews"
            );
            foreach (var publication in closedPublications)
            {
                try
                {
                    Log.Information(
                        "Procesando cierre de publicación ID: {PublicationId} - {Title}",
                        publication.Id,
                        publication.Title
                    );
                    // TODO: Preguntar logica para BuySell
                    // Por ahora solo se procesa Offers que tienen postulaciones
                    if (publication is not Offer offer)
                    {
                        Log.Information(
                            "Publicación ID: {PublicationId} es tipo {Type}, no tiene postulaciones para crear reviews",
                            publication.Id,
                            publication.GetType().Name
                        );
                        continue;
                    }
                    // Obtener postulaciones aceptadas para esta Offer
                    var acceptedPostulations = await JobApplications
                        .Where(ja =>
                            ja.JobOfferId == offer.Id && ja.Status == ApplicationStatus.Aceptada
                        )
                        .ToListAsync(cancellationToken);
                    if (acceptedPostulations.Count == 0)
                    {
                        Log.Information(
                            "No hay postulaciones aceptadas para la oferta ID: {OfferId}",
                            offer.Id
                        );
                        continue;
                    }
                    Log.Information(
                        "Encontradas {Count} postulaciones aceptadas para crear reviews",
                        acceptedPostulations.Count
                    );
                    // Crear review por cada postulación aceptada
                    foreach (var postulation in acceptedPostulations)
                    {
                        // Evitar duplicados
                        var existingReview = await Reviews.AnyAsync(
                            r =>
                                r.PublicationId == offer.Id
                                && r.StudentId == postulation.StudentId
                                && r.OfferorId == offer.UserId,
                            cancellationToken
                        );
                        if (existingReview)
                        {
                            Log.Warning(
                                "Ya existe una review para Oferta: {OfferId}, Estudiante: {StudentId}, Oferente: {OfferorId}",
                                offer.Id,
                                postulation.StudentId,
                                offer.UserId
                            );
                            continue;
                        }
                        var review = new Review
                        {
                            PublicationId = offer.Id,
                            StudentId = postulation.StudentId,
                            OfferorId = offer.UserId,
                            IsReviewForStudentCompleted = false,
                            IsReviewForOfferorCompleted = false,
                            IsCompleted = false,
                            IsClosed = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                        };
                        Reviews.Add(review);
                        Log.Information(
                            "Review creada para Estudiante ID: {StudentId}, y Oferente ID: {OfferorId} en Oferta ID: {OfferId}",
                            postulation.StudentId,
                            offer.UserId,
                            offer.Id
                        );
                    }
                    // Guardar las reviews creadas
                    await base.SaveChangesAsync(cancellationToken);
                    Log.Information(
                        "Se crearon {Count} reviews iniciales para la oferta ID: {OfferId}",
                        acceptedPostulations.Count,
                        offer.Id
                    );
                }
                catch (Exception ex)
                {
                    Log.Error(
                        ex,
                        "Error al crear reviews para publicación ID: {PublicationId}",
                        publication.Id
                    );
                    // No lanzar la excepción para no romper el flujo principal
                    // Las reviews se pueden crear manualmente si falla
                }
            }
        }
        #endregion
    }
}
