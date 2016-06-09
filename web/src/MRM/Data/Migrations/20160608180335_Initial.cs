using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MRM.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalRecord",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdmittingDoctor = table.Column<string>(nullable: true),
                    AttendingDoctor = table.Column<string>(nullable: true),
                    Contrast = table.Column<string>(nullable: true),
                    DirectorName = table.Column<string>(nullable: true),
                    Dob = table.Column<DateTime>(nullable: false),
                    ExamDate = table.Column<DateTime>(nullable: false),
                    FacilityId = table.Column<int>(nullable: false),
                    FacilityLocation = table.Column<string>(nullable: true),
                    FacilityName = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    Laterality = table.Column<string>(nullable: true),
                    NumberOfFilms = table.Column<int>(nullable: false),
                    OrderingPh = table.Column<string>(nullable: true),
                    Patient = table.Column<string>(nullable: true),
                    PatientId = table.Column<int>(nullable: false),
                    Pos = table.Column<string>(nullable: true),
                    Procedure = table.Column<string>(nullable: true),
                    Radiologist = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    ReportStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecord", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalRecord");
        }
    }
}
