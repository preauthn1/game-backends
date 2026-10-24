using System;
using System.Text;
using example.invalid;
using System.Security;
using System.Security.Cryptography;

namespace game.Infrastructure.Encrypt
{


    /// <summary>
    /// 如果要加密的字串比較長
    /// 用一般的方式又發生 長度錯誤的 exception，可以使用此
    /// </summary>
    public static class RASLargeTextEncryptHelper
    {

        //The key size to use
        private const int _EncryptionKeySize = 1024;

        //The buffer size to encrypt per set
        private const int _EncryptionBufferSize = 117;
        
        // The buffer size to decrypt per set
        private const int _DecryptionBufferSize = 128;

      
        /// <summary>
        /// Retrieve the private key
        /// </summary>
        /// <param name="keyFilePath">The private keys full file path</param>
        /// <returns>The private key in XML string format</returns>
        private static RSACryptoServiceProvider RetrieveKey(string keyFilePath)
        {
            //Read the private key from the specified file path
            byte[] privateKeyEncrypted;

            using (FileStream fs = new FileStream(keyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {

                privateKeyEncrypted = new byte[fs.Length];

                fs.Read(privateKeyEncrypted, 0, (int)fs.Length);

            }

            //Decrypt the private key with DPAPI
            byte[] privateKeyEncoded = ProtectedData.Unprotect(privateKeyEncrypted, null, DataProtectionScope.LocalMachine);

            //Unencode the private key
            string privateKey = Encoding.UTF8.GetString(privateKeyEncoded);

            //Return the private key
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(_EncryptionBufferSize);

            rsa.FromXmlString(privateKey);

            return rsa;
        }

        /// <summary>
        /// Encrypt the data using RSA
        /// </summary>
        /// <param name="data">The string data to encrypt</param>
        /// <param name="publicKey">The public key in XML format to encrypt with</param>
        /// <returns>An array of bytes containing the encrypted data</returns>
        public static byte[] EncryptData(string data, string publicKey)
        {
            //Create a new asymmetric algorithm object using the supplied public key
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            rsa.FromXmlString(publicKey);

            //Encode the data

            byte[] dataEncoded = Encoding.UTF8.GetBytes(data);

            //Store every chunk of encrypted data during the encryption process in a memory stream
            using (MemoryStream ms = new MemoryStream())

            {

                //Create a buffer with the maximum allowed size

                byte[] buffer = new byte[_EncryptionBufferSize];

                int pos = 0;

                int copyLength = buffer.Length;

                while (true)

                {

                    //Check if the bytes left to read is smaller than the buffer size, then limit the buffer size to the number of bytes left
                    if (pos + copyLength > dataEncoded.Length)

                        copyLength = dataEncoded.Length - pos;

                    //Create a new buffer that has the correct size
                    buffer = new byte[copyLength];

                    //Copy as many bytes as the algorithm can handle at a time, iterate until the whole input array is encoded
                    Array.Copy(dataEncoded, pos, buffer, 0, copyLength);

                    //Start from here in next iteration
                    pos += copyLength;

                    //Encrypt the data using the public key and add it to the memory buffer
                    //_DecryptionBufferSize is the size of the encrypted data
                    ms.Write(rsa.Encrypt(buffer, false), 0, _DecryptionBufferSize);

                    //Clear the content of the buffer, otherwise we could end up copying the same data during the last iteration
                    Array.Clear(buffer, 0, copyLength);

                    //Check if we have reached the end, then exit
                    if (pos >= dataEncoded.Length)

                        break;

                }

                //Return the encrypted data
                return ms.ToArray();

            }

        }

        /// <summary>
        /// Decrypt the data using the private key
        /// </summary>
        /// <param name="data">The data to decrypt</param>
        /// <param name="keyFilePath">The full path to the private key file</param>
        /// <returns>The decrypted data in string format</returns>
        /// <exception cref="System.Security.CryptoGraphicException">Unable to decrypt the data, probably corrupted</exception>

        public static string DecryptData(byte[] data, string keyFilePath)
        {
            try
            {

                //Retrieve the private key from the key file path supplied
                RSACryptoServiceProvider rsa = RetrieveKey(keyFilePath);

                //Initialize a memory stream to hold the encrypted chunks of data, use the same size as the encrypted data (however, will be slightly smaller)
                using (MemoryStream ms = new MemoryStream(data.Length))

                {

                    //The buffer that will hold the encrypted chunks

                    byte[] buffer = new byte[_DecryptionBufferSize];

                    int pos = 0;

                    int copyLength = buffer.Length;

                    while (true)

                    {

                        //Copy a chunk of encrypted data / iteration
                        Array.Copy(data, pos, buffer, 0, copyLength);

                        //Set the next start position
                        pos += copyLength;

                        //Decrypt the data using the private key
                        //We need to store the decrypted data temporarily because we don't know the size of it; unlike with encryption where we know the size is 128 bytes. The only thing we know is that it's between 1-117 bytes
                        byte[] resp = rsa.Decrypt(buffer, false);

                        ms.Write(resp, 0, resp.Length);

                        //Cleat the buffers
                        Array.Clear(resp, 0, resp.Length);

                        Array.Clear(buffer, 0, copyLength);

                        //Are we ready to exit?
                        if (pos >= data.Length)

                            break;

                    }

                    //Return the decoded data
                    return Encoding.UTF8.GetString(ms.ToArray());

                }

            }

            //The data is probably corrupted
            catch (CryptographicException ce)
            {

                throw ce;

            }

        }

    }

}