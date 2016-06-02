using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDi
{
    using System.Data.Entity;
    class MedicalRecordContext : DbContext
    {
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
    }
}
