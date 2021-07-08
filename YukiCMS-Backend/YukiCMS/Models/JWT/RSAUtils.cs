using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace YukiCMS.Models.JWT
{
    public static class RSAUtils
    {
        static RSAParameters _privateKeys, _publicKeys;
        public static RSAParameters generateKey()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    _privateKeys = rsa.ExportParameters(true);
                    _publicKeys = rsa.ExportParameters(false);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
            return _privateKeys;
        }
        public static RSAParameters getPrivateKeyParas()
        {
            return _privateKeys;
        }
        public static RSAParameters getPublicKeyParas()
        {
            return _publicKeys;
        }
    }
}
