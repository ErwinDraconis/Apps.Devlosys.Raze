using DeviceId;
using System.Security.Cryptography;
using System.Text;

namespace Apps.Devlosys.Infrastructure.Helpers
{
    public static class ComputerInfo
    {
        public static string HashCode => HashString(GetComputerID());

        private static string GetComputerID()
        {
            return new DeviceIdBuilder()
                .AddMachineName()
                .OnWindows(windows => windows
                    .AddProcessorId()
                    .AddMotherboardSerialNumber())
                .ToString();
        }

        private static string HashString(string text)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char[] hash2 = new char[32];
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }
    }
}
