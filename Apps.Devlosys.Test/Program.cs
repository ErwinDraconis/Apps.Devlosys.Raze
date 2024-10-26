using Apps.Devlosys.Services;
using Apps.Devlosys.Services.Interfaces;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Apps.Devlosys.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                /*IIMSApi api = new IMSApi();
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                Console.WriteLine("Connect To ITAC...");
                await Task.Run(() => api.ItacConnection());
                Console.WriteLine("ITAC Connected");

                Console.WriteLine("Authentication...");
                await Task.Run(() => api.CheckUser("OPERATOR", "PASSWORD"));
                Console.WriteLine("Authentication done");

                bool exit = false;
                Console.WriteLine("Start check...");

                while (!exit)
                {
                    Console.WriteLine("Enter the station number ref :");
                    string stationNumber = Console.ReadLine();

                    Console.WriteLine("Enter the serial number ref :");
                    string snr = Console.ReadLine();

                    try
                    {
                        await Task.Run(() =>
                        {
                            bool result = api.CheckSerialNumberState(stationNumber, snr, new string[2] { "SERIAL_NUMBER_STATE", "ERROR_CODE" }, out string[] results, out int code);

                            Console.WriteLine($"Code : {code}");
                            Console.WriteLine("Results :");
                            Console.WriteLine($"====> {results[0]}");
                            Console.WriteLine($"====> {results[1]}");
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception : {(ex.InnerException ?? ex).Message}");
                    }

                    Console.WriteLine("==========================================");

                    Console.WriteLine("Tab y/Y key to exit!");
                    var key = Console.ReadLine();

                    exit = key.ToLower() == key.ToLower();
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {(ex.InnerException ?? ex).Message}");
                Console.ReadLine();
            }
        }
    }
}
