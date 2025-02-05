using Apps.Devlosys.Infrastructure.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Apps.Devlosys.Controls.Helpers
{
    public static class CsvHelper
    {
        public static void WriteToCsv(GaliaResult galiaResult)
        {
            string CC_FolderName = "CC_logs";
            string currentDate = DateTime.Now.ToString("yyyyMMdd"); 
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CC_FolderName);

            // Generate file name using GaliaNb, GaliaPN, and date
            string fileName = $"{galiaResult.GaliaNb}_{galiaResult.GaliaPN}_{currentDate}.txt";
            string filePath = Path.Combine(directoryPath, fileName);
            var csv = new StringBuilder();

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Check if a file already exists
            string existingFile = Directory.GetFiles(directoryPath, $"{galiaResult.GaliaNb}_{galiaResult.GaliaPN}_*.txt")
                                           .FirstOrDefault();

            if (existingFile != null)
            {
                filePath = existingFile;
            }

            bool fileExists = File.Exists(filePath);

            if (!fileExists)
            {
                csv.AppendLine("Galia N°,Galia PN,Serial Number,Serial Number PN,Result");
            }

            string result = galiaResult.Status == 0 ? "Pass" : "Fail";
            csv.AppendLine($"{galiaResult.GaliaNb},{galiaResult.GaliaPN},{galiaResult.PCBSN},{galiaResult.PCBPN},{result}");

            try
            {
                File.AppendAllText(filePath, csv.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Log.Error($"Failed to write CSV for Galia result: {ex.Message}");
            }
        }

    }
}
