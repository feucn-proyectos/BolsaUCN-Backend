using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.src.Application.DTOs.JobAplicationDTO
{
    /// <summary>
    /// Dto para obtener una lista de postulantes mostrando solamente el nombre del postulante y el status de la publicacion
    /// </summary>
    public class ViewApplicantsDto
    {
        public int Id { get; set; }
        public string Applicant { get; set; }
        public string Status { get; set; } = string.Empty;
        public double Rating { get; set; }
    }
}
