using Microsoft.AspNetCore.Identity;

namespace backend.src.Domain.Models
{
    /// <summary>
    /// Tipo de usuario en el sistema.
    /// </summary>
    public enum UserType
    {
        Estudiante,
        Empresa,
        Particular,
        Administrador,
    }

    /// <summary>
    /// Tipo de discapacidad del usuario.
    /// </summary>
    public enum Disability
    {
        Ninguna,
        Visual,
        Auditiva,
        Motriz,
        Cognitiva,
        Otra,
    }

    public class User : IdentityUser<int>
    {
        // === PROPIEDADES GENERALES ===
        /// <summary>
        /// Nombre del usuario.
        /// Estudiante/Particular/Administrador: Nombre de pila.
        /// Empresa: Nombre de la empresa.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// Estudiante/Particular/Administrador: Apellido paterno.
        /// Empresa: Razon social de la empresa.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Tipo de usuario en el sistema.
        /// </summary>
        public required UserType UserType { get; set; }

        /// <summary>
        /// RUT del usuario (sin puntos, con guion).
        /// </summary>
        public required string Rut { get; set; }

        /// <summary>
        /// Breve descripción o biografía del usuario.
        /// </summary>
        public string? AboutMe { get; set; }

        /// <summary>
        /// Calificación promedio del usuario (de 1.0 a 6.0).
        /// </summary>
        public double Rating { get; set; } = 6.0;

        // === PROPIEDADES DE ESTUDIANTES ===

        /// <summary>
        /// Identificador del currículum vitae (CV) asociado al usuario estudiante.
        /// </summary>
        public int? CVId { get; set; }

        /// <summary>
        /// Currículum vitae (CV) asociado al usuario estudiante.
        /// </summary>
        public Curriculum? CV { get; set; }

        /// <summary>
        /// Discapacidad del usuario estudiante.
        /// </summary>
        public Disability? Disability { get; set; }

        // === PROPIEDADES DE AUDITORIA ===

        /// <summary>
        /// Indica si el usuario está bloqueado.
        /// Un usuario bloqueado no puede iniciar sesión ni realizar acciones en el sistema.
        /// true = bloqueado; false = activo.
        /// Por defecto es false.
        /// </summary>
        public required bool IsBlocked { get; set; } = false;

        /// <summary>
        /// Correo electrónico pendiente de verificación.
        /// Este atributo es exclusivo para usuarios que han cambiado su correo electrónico y aún no lo han verificado.
        /// Para nuevos usuarios, el correo electrónico se verifica al momento de la creación de la cuenta.
        /// </summary>
        public string? PendingEmail { get; set; }

        /// <summary>
        /// Fecha y hora de cuando el email pendiente de verificación expira.
        /// </summary>
        public DateTime? PendingEmailExpiration { get; set; }

        /// <summary>
        /// Fecha y hora de creación del usuario (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora de la última actualización del usuario (UTC).
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora del último inicio de sesión del usuario (UTC).
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        // === PUBLICACIONES ===

        /// <summary>
        /// Colección de publicaciones creadas por el usuario.
        /// </summary>
        public ICollection<Publication> Publications { get; set; } = new List<Publication>();

        /// <summary>
        /// Colección de postulaciones realizadas por el usuario (si es estudiante).
        /// </summary>
        public ICollection<JobApplication>? Applications { get; set; }

        // === IMÁGENES ===

        /// <summary>
        /// Identificador de la foto de perfil del usuario.
        /// </summary>
        public int? ProfilePhotoId { get; set; }

        /// <summary>
        /// Foto de perfil del usuario.
        /// </summary>
        public UserImage? ProfilePhoto { get; set; } = null;
    }
}
