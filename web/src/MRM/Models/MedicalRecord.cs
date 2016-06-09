using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRM.Models
{
    using System.ComponentModel.DataAnnotations;

    public class MedicalRecord
    {
        public int ID { get; set; }

        [Display(Name = "Facility ID")]
        public int FacilityId { get; set; }

        [Display(Name = "Facility Name")]
        public string FacilityName { get; set; }

        [Display(Name = "Facility Name")]
        public string FacilityLocation { get; set; }

        public string Patient { get; set; }

        public string Gender { get; set; }

        [Display(Name = "Birthdate")]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Display(Name = "Patient ID")]
        public int PatientId { get; set; }

        public string Procedure { get; set; }

        [Display(Name = "Number of Films")]
        public int NumberOfFilms { get; set; }

        public string Laterality { get; set; }

        public string Contrast { get; set; }

        public string Reason { get; set; }

        [Display(Name = "Exam Date")]
        public DateTime ExamDate { get; set; }

        public string Radiologist { get; set; }

        [Display(Name = "Ordering Physician")]
        public string OrderingPh { get; set; }

        public string Pos { get; set; }

        [Display(Name = "Report Status")]
        public int ReportStatus { get; set; }

        [Display(Name = "Attending Doctor")]
        public string AttendingDoctor { get; set; }

        [Display(Name = "Admitting Doctor")]
        public string AdmittingDoctor { get; set; }

        [Display(Name = "Director Name")]
        public string DirectorName { get; set; }
    }
}
