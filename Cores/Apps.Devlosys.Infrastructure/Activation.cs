using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Apps.Devlosys.Infrastructure
{
    public static class Activation
    {
        public const string KEY_NAME = "Licence";

        public static bool VerifySerial(string signature, string signatureFromDataBase)
        {
            try
            {
                byte[] signatureHash = Base64Decode(signatureFromDataBase);

                X509Certificate2 certificate = new("MODUS-PUBLIC-CERT.pem");

#if NET30_OR_GREATER
                RSACryptoServiceProvider cryptoServiceProvider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
#else
                RSACng cryptoServiceProvider = (RSACng)certificate.PublicKey.Key;
#endif
                return cryptoServiceProvider.VerifyHash(GetDataHash(signature), signatureHash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            }
            catch (Exception) { return false; }
        }

        private static byte[] Base64Decode(string inputStrings)
        {
            return Convert.FromBase64String(inputStrings);
        }

        private static byte[] GetDataHash(string sampleData)
        {
            SHA1Managed managedHash = new();

            return managedHash.ComputeHash(Encoding.Unicode.GetBytes(sampleData));
        }
    }
}
