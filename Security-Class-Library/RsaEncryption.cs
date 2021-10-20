using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;

namespace Security_Class_Library
{
    public class RsaEncryption
    {
        private static RSACryptoServiceProvider csp;
        private RSAParameters privateKey;
        private RSAParameters publicKey;

        private List<RSAParameters> listOtherPublicKey;

        private static int MaxEncryptSize = 100;

        private string filePath = "D:\\repos\\Security-Case-Implementation\\Server-Public-Key.txt";

        public RsaEncryption()
        {
            csp = new RSACryptoServiceProvider(2048);
            privateKey = csp.ExportParameters(true);
            publicKey = csp.ExportParameters(false);
            listOtherPublicKey = new List<RSAParameters>();
            
        }
    }
}
