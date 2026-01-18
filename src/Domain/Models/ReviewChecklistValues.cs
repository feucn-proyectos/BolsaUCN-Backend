namespace backend.src.Domain.Models
{
    public class ReviewChecklistValues
    {
        /// <summary>
        /// Indica si el estudiante llegó a tiempo a su lugar de trabajo.
        /// </summary>
        public bool AtTime { get; set; } = false;

        /// <summary>
        /// Indica si el estudiante tuvo una buena presentación durante la realización del trabajo.
        /// </summary>
        public bool GoodPresentation { get; set; } = false;

        /// <summary>
        /// Booleano que indica si el estudiante mostró respeto hacia el oferente.
        /// </summary>
        /// <value></value>
        public bool StudentHasRespectOfferor { get; set; } = false;
    }
}
