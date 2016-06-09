using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MRM.Data;
using MRM.Models;

namespace MRM.Controllers
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;

    public class MedicalRecordsController : Controller
    {
        private static readonly Dictionary<string, string> fieldDictionary = new Dictionary<string, string>();
        private readonly ApplicationDbContext _context;
        private IHostingEnvironment hostingEnv;

        public MedicalRecordsController(ApplicationDbContext context, IHostingEnvironment env)
        {
            _context = context;
            hostingEnv = env;
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index()
        {
            return View(await _context.MedicalRecord.ToListAsync());
        }

        // GET: MedicalRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecord.SingleOrDefaultAsync(m => m.ID == id);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // GET: MedicalRecords/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MedicalRecords/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,AdmittingDoctor,AttendingDoctor,Contrast,DirectorName,Dob,ExamDate,FacilityId,FacilityLocation,FacilityName,Gender,Laterality,NumberOfFilms,OrderingPh,Patient,PatientId,Pos,Procedure,Radiologist,Reason,ReportStatus")] MedicalRecord medicalRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicalRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecord.SingleOrDefaultAsync(m => m.ID == id);
            if (medicalRecord == null)
            {
                return NotFound();
            }
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,AdmittingDoctor,AttendingDoctor,Contrast,DirectorName,Dob,ExamDate,FacilityId,FacilityLocation,FacilityName,Gender,Laterality,NumberOfFilms,OrderingPh,Patient,PatientId,Pos,Procedure,Radiologist,Reason,ReportStatus")] MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalRecordExists(medicalRecord.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecord.SingleOrDefaultAsync(m => m.ID == id);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalRecord = await _context.MedicalRecord.SingleOrDefaultAsync(m => m.ID == id);
            _context.MedicalRecord.Remove(medicalRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool MedicalRecordExists(int id)
        {
            return _context.MedicalRecord.Any(e => e.ID == id);
        }

        // GET: MedicalRecords/Upload
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {

            var fileName = ContentDispositionHeaderValue
                            .Parse(file.ContentDisposition)
                            .FileName
                            .Trim('"');
            fileName = hostingEnv.ContentRootPath + $@"\tmp\{fileName}";

            if (!Directory.Exists(hostingEnv.ContentRootPath + $@"\tmp\"))
            {
                Directory.CreateDirectory(hostingEnv.ContentRootPath + $@"\tmp\");
            }

            using (FileStream fs = System.IO.File.Create(fileName))
            {
                // copy file to server to read from
                file.CopyTo(fs);
                fs.Flush();
            }

            ProcessMedicalRecordsRecords(fileName);

            // remove file after done using
            System.IO.File.Delete(fileName);

            return View();
        }

        // reads medical record into memory and capture the data
        private void ProcessMedicalRecordsRecords(string path)
        {
            // string that represents the separator between records
            var separator = "===================END OF RESULT===================";

            try
            {
                // strip quotes from path that allowed for spaces
                path.Replace("\"", string.Empty);

                string line = string.Empty;
                var record = string.Empty;


                // read the file and display it line by line.
                var file = new StreamReader(System.IO.File.OpenRead(path));
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Equals(separator, StringComparison.Ordinal))
                    {
                        // regex capture each field and save it into the medical record
                        // use reflection to obtain the medicalRecord variables
                        var medicalRecord = new MedicalRecord();
                        foreach (var prop in medicalRecord.GetType().GetProperties())
                        {
                            var pattern = @"^(" + prop.Name + @"):\s*(\S*[ &'a-z0-9/:-^]*);$";
                            var match = Regex.Match(record, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            SetMedicalRecordFields(medicalRecord, prop, match);
                        }

                        Debug.WriteLine("****** Pre Save: " + medicalRecord.Patient);
                        foreach (var prop in medicalRecord.GetType().GetProperties())
                        {
                            Debug.WriteLine(prop.Name + ": " + prop.GetValue(medicalRecord));
                        }
                        _context.Add(medicalRecord);
                        _context.SaveChanges();
                        record = string.Empty;
                    }
                    else
                    {
                        var index = line.IndexOf(":");
                        if (index > 0)
                        {
                            // strip spaces from the field name to match object variables
                            var field = line.Substring(0, index);
                            field = field.Replace(" ", string.Empty);
                            line = field + line.Substring(index);
                        }

                        // added semicolon and newline to make for easier regex capturing
                        // concatenate all the lines of a single record for regex capturing
                        record += line + ";\n";
                    }
                }

                file.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.InnerException.Message);
                Debug.WriteLine(e.Message);
            }
        }

        private static void SetMedicalRecordFields(MedicalRecord medicalRecord, PropertyInfo prop, Match match)
        {
            var fieldName = match.Groups[1].Value;
            var fieldData = match.Groups[2].Value;

            // sets the captured values to the medical record object
            if (prop.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        if (string.Equals(fieldData, "", StringComparison.Ordinal))
                        {
                            fieldData = string.Empty;
                        }

                        prop.SetValue(medicalRecord, fieldData, null);
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(medicalRecord, int.Parse(fieldData), null);
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        prop.SetValue(medicalRecord, Convert.ToDateTime(fieldData), null);
                    }
                    else
                    {
                        Debug.WriteLine("A sortField type was not accounted for");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }
    }
}
