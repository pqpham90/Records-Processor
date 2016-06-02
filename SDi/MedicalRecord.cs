// ------------------------------
// MedicalRecordProcessor.CS
// Copyright (C) 2016
// Phillip Pham
// ------------------------------

namespace SDi
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    class MedicalRecord
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string FacilityLocation { get; set; }

        public string Patient { get; set; }
        public string Gender { get; set; }
        public DateTime Dob { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PatientId { get; set; }

        public string Procedure { get; set; }
        public int NumberOfFilms { get; set; }
        public string Laterality { get; set; }
        public string Contrast { get; set; }
        public string Reason { get; set; }
        public DateTime ExamDate { get; set; }
        public string Radiologist { get; set; }
        public string OrderingPh { get; set; }
        public string Pos { get; set; }
        public int ReportStatus { get; set; }

        public string AttendingDoctor { get; set; }
        public string AdmittingDoctor { get; set; }
        public string DirectorName { get; set; }
    }
}
