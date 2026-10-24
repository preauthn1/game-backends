using System;
using System.Text;
using System.Security.Cryptography;
using example.invalid;

namespace game.WebSite.Services
{
    public static class RSAService
    {
        private static RSACryptoServiceProvider GetRSAByPublicKey(string publicKey)
        {
            publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n","");
            var public_key = PayConfig.RSAPublicKeyJava2DotNet(publicKey);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(public_key);
            return rsa;
        }

        private static RSACryptoServiceProvider GetRSAByPrivateKey(string privateKey)
        {
            privateKey = privateKey.Replace("[PRIVATE_KEY_BLOCK_REDACTED]", "").Replace("\n", "");
            RSACryptoServiceProvider rsa = CreateRsaProviderFromPrivateKey(privateKey);
            return rsa;
        }

        public static string Sign(string privateKey, string content)
        {
            RSACryptoServiceProvider rsa = GetRSAByPrivateKey(privateKey);

            byte[] importantMessageBytes = Encoding.UTF8.GetBytes(content);
            byte[] bytes = rsa.SignData(importantMessageBytes, typeof(SHA1));
            string signature = Convert.ToBase64String(bytes);

            return signature;
        }

        public static bool SignChk(string publicKey, string encryptedContent, string sign)
        {
            RSACryptoServiceProvider rsa = GetRSAByPublicKey(publicKey);

            byte[] importantMessageBytes = Encoding.UTF8.GetBytes(encryptedContent);
            byte[] signatureBytes = Convert.FromBase64String(sign);
            bool isSignatureOkay = rsa.VerifyData(importantMessageBytes, typeof(SHA1), signatureBytes);

            return isSignatureOkay;
        }

        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var RSA = new RSACryptoServiceProvider(2048);
            var RSAparams = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            RSA.ImportParameters(RSAparams);
            return RSA;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
    }
}
