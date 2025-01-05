using Apps.Devlosys.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Devlosys.Controls.Helpers
{
    public static class CsvHelper
    {
        public static void WriteToCsv(GaliaResult galiaResult)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string filePath = Path.Combine(directoryPath, $"GaliaData_{currentDate}.txt");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var csv = new StringBuilder();

            if (!File.Exists(filePath))
            {
                csv.AppendLine($"Galia N°,Galia PN,Serial Number,Serial Number PN,Result");
            }

            string result = galiaResult.Status == 0 ? "Pass" : "Fail";

            csv.AppendLine($"{galiaResult.GaliaNb},{galiaResult.GaliaPN},{galiaResult.PCBSN},{galiaResult.PCBPN},{result}");
            

            try
            {
                // Append data to the file (create if it doesn't exist)
                File.AppendAllText(filePath, csv.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
               // Log.Error($"Failed to write CSV for Galia result: {ex.Message}");
            }
        }

    }
}
