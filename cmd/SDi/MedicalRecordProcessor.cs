// ------------------------------
// MedicalRecordProcessor.CS
// Copyright (C) 2016
// Phillip Pham
// ------------------------------

using System.Text;

namespace SDi
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class MedicalRecordProcessor
    {
        private static readonly Dictionary<string, string> fieldDictionary = new Dictionary<string, string>();

        public static void Main(string[] args)
        {
            // require the -file flag as the first flag and ensure something is input for it
            if (args.Length < 2 || !args[0].Equals("-file", StringComparison.Ordinal))
            {
                PrintUsage();
            }

            // check for -sort flag and ensure it has a value
            var checkForSort = Array.IndexOf(args, "-sort");
            var sort = checkForSort > -1;
            if (sort && args.Length <= checkForSort + 1)
            {
                PrintUsage();
            }

            // check for -search flag and ensure it has a value
            var checkForSearch = Array.IndexOf(args, "-search");
            var search = checkForSearch > -1;
            if (search && args.Length <= checkForSearch + 1)
            {
                PrintUsage();
            }

            // check for -entity
            var checkForEntity = Array.IndexOf(args, "-entity");
            var entity = checkForEntity > -1;

            // dictionary uses to map inputted/display format to the MedicalRecord variables and vice versa
            CreateFieldDictionary();

            // validate and convert the value of the -sort flag
            // went and made the sort field case insensitive
            // use display format for input
            var sortField = "FacilityId";
            if (sort)
            {
                sortField = CheckSortValue(args[checkForSort + 1]);
            }

            // parse the medical records into memory
            var medicalRecords = ProcessMedicalRecordsRecords(args[1]);

            if (!entity)
            {
                // searches records for query
                if (search)
                {
                    medicalRecords = (SearchMedicalRecords(medicalRecords, args[checkForSearch + 1]));
                }

                // sort the records
                SortMedicalRecords(sort, sortField, medicalRecords);
            }
            else
            {
                populateDatabase(medicalRecords);

                //using (MedicalRecordContext context = new MedicalRecordContext())
                //{
                //    context.Database.Log = s => Console.WriteLine(s);
                //}
            }

            Console.WriteLine("Done processing, press any key to exit.");
            Console.ReadLine();
        }

        private static void populateDatabase(List<MedicalRecord> medicalRecords)
        {
            try
            {
                using (var db = new MedicalRecordContext())
                {
                    foreach (var record in medicalRecords)
                    {
                        db.MedicalRecords.AddOrUpdate(record);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException);
                }
                else
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        // searches each record for the query string
        private static List<MedicalRecord> SearchMedicalRecords(List<MedicalRecord> medicalRecords, string query)
        {
            // list of medical records to allow for easy sorting
            var searchRecords = new List<MedicalRecord>();

            var sb = new StringBuilder();

            // look through each property of the medical record for query
            foreach (var medicalRecord in medicalRecords)
            {
                var props = medicalRecord.GetType().GetProperties();

                foreach (var p in props)
                {
                    sb.AppendLine((p.Name + ":").PadRight(18, ' ') + p.GetValue(medicalRecord, null));
                }

                string record = sb.ToString();

                if (record.ToLower().Contains(query.ToLower()))
                {
                    searchRecords.Add(medicalRecord);
                }

                sb = new StringBuilder();
            }

            return searchRecords;
        }

        public static string PropertyList(object obj)
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
            }
            return sb.ToString();
        }

        // two way mapping, from variable format to display format and vice versa
        private static void CreateFieldDictionary()
        {
            // display format of fields
            string[] originalFields =
                {
                    "FacilityID", "FacilityName", "Facility Location", "Patient", "Gender", "DOB", 
                    "PatientID", "Procedure", "Number of Films", "Laterality", "Contrast", "Reason", 
                    "ExamDate", "Radiologist", "Ordering Ph", "POS", "ReportStatus", 
                    "Attending Doctor", "Admitting Doctor", "Director Name"
                };

            try
            {
                var counter = 0;
                var medicalRecord = new MedicalRecord();
                foreach (var prop in medicalRecord.GetType().GetProperties())
                {
                    fieldDictionary.Add(originalFields[counter].ToLower(), prop.Name);
                    fieldDictionary.Add(prop.Name, originalFields[counter]);
                    ++counter;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Dictionary input length mismatch");
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: Program1.exe -file <FILE_LOCATION> [options]\n");
            Console.WriteLine("Options:");

            Console.Write("  -sort <FIELD_NAME>".PadRight(33, ' '));
            Console.WriteLine("Sorts the data in ascending order by the field name.");

            Console.Write("  -search <QUERY>".PadRight(33, ' '));
            Console.WriteLine("Searches the data for specified query");

            Console.Write("  -entity".PadRight(33, ' '));
            Console.WriteLine("Performs operations using the Entity Framework");

            //Console.ReadLine();
            Environment.Exit(1);
        }

        // reads medical record into memory and capture the data
        private static List<MedicalRecord> ProcessMedicalRecordsRecords(string path)
        {
            // string that represents the separator between records
            var separator = "===================END OF RESULT===================";

            // list of medical records to allow for easy sorting
            var medicalRecords = new List<MedicalRecord>();

            try
            {
                // strip quotes from path that allowed for spaces
                path.Replace("\"", string.Empty);

                string line;
                var record = string.Empty;

                // read the file and display it line by line.
                var file = new StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Equals(separator, StringComparison.Ordinal))
                    {
                        // regex capture each field and save it into the medical record
                        // use reflection to obtain the medicalRecord variables
                        var medicalRecord = new MedicalRecord();
                        foreach (var prop in medicalRecord.GetType().GetProperties())
                        {
                            var pattern = @"^(" + prop.Name + @"):\s*(\S*[ 'a-z0-9/:-^;]*);$";
                            var match = Regex.Match(record, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

                            SetMedicalRecordFields(medicalRecord, prop, match);
                        }

                        medicalRecords.Add(medicalRecord);
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

                file.Close();
            }
            catch (IndexOutOfRangeException)
            {
                PrintUsage();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return medicalRecords;
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
                        Console.WriteLine("A sortField type was not accounted for");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        // checks to see if the provided field exists in the medical record
        private static string CheckSortValue(string sortField)
        {
            // allow for field names with multiple words on command line
            sortField.Replace("\"", string.Empty);

            var fieldName = "foo";

            // convert from display format to variable format
            if (fieldDictionary.ContainsKey(sortField.ToLower()))
            {
                fieldName = fieldDictionary[sortField.ToLower()];
            }
            else
            {
                Console.WriteLine("Field does not exist");
                //Console.ReadLine();
                Environment.Exit(1);
            }

            return fieldName;
        }

        private static void SortMedicalRecords(bool sort, string sortField, List<MedicalRecord> medicalRecords)
        {
            var sortedRecords = new List<MedicalRecord>();

            // use default sort if no sort flag, otherwise sort by user defined value in ascending order
            // uses reflection to sort by inputted field
            if (!sort)
            {
                sortedRecords =
                    medicalRecords.OrderByDescending(s => s.GetType().GetProperty(sortField).GetValue(s)).ToList();
            }
            else
            {
                sortedRecords = medicalRecords.OrderBy(s => s.GetType().GetProperty(sortField).GetValue(s)).ToList();
            }

            // print out the record
            // assuming a uniform medical record format was desired
            // otherwise I could have simply saved the original strings and printed those out instead of iterating the object
            foreach (var foo in sortedRecords)
            {
                foreach (var prop in foo.GetType().GetProperties())
                {
                    Console.WriteLine(
                        "{0} {1}", 
                        (fieldDictionary[prop.Name] + ":").PadRight(18, ' '), 
                        prop.GetValue(foo, null));
                }

                Console.WriteLine();
            }
        }
    }
}