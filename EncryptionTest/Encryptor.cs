using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace EncryptionTest
{
    public class Encryptor
    {
        /// <summary>
        /// Creates a random salt that will be used to encrypt your file. This method is required on FileEncrypt.
        /// </summary>
        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fille the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        /// <summary>
        /// Encrypts a file from its path and a plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        public void FileEncrypt(string inputFile, string password)
        {
            // Generate random salt
            byte[] salt = GenerateRandomSalt();

            // Convert password string to byte arrray
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            // Create output file name
            using (FileStream fsWrite = new FileStream("DataFile.dat.aes", FileMode.Create, FileAccess.Write))
            {
                // Get crypto provider
                var AES = GetCrypto(passwordBytes, salt);

                // Write salt to the begining of the output file, so in this case can be random every time
                fsWrite.Write(salt, 0, salt.Length);

                try
                {
                    using (CryptoStream cs = new CryptoStream(fsWrite, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    using (FileStream fsRead = new FileStream(inputFile, FileMode.Open))
                    {
                        //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
                        byte[] buffer = new byte[1048576];
                        int read;

                        while ((read = fsRead.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cs.Write(buffer, 0, read);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        public void FileDecrypt(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            using (FileStream fsRead = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                fsRead.Read(salt, 0, salt.Length);

                var AES = GetCrypto(passwordBytes, salt);

                try
                {
                    using (CryptoStream cs = new CryptoStream(fsRead, AES.CreateDecryptor(), CryptoStreamMode.Read))
                    using (FileStream fsWrite = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        int read;
                        byte[] buffer = new byte[1048576];

                        while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fsWrite.Write(buffer, 0, read);
                        }
                    }
                }

                catch (CryptographicException ex_CryptographicException)
                {
                    Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Encrypts an object from a plain password.
        /// </summary>
        /// <param name="password"></param>
        public void ObjectEncrypt(SomeObject someObject, string password)
        {
            byte[] salt = GenerateRandomSalt();
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            using (FileStream fsWrite = new FileStream("DataFile.dat.aes", FileMode.Create, FileAccess.Write))
            {
                var AES = GetCrypto(passwordBytes, salt);

                fsWrite.Write(salt, 0, salt.Length);

                using (CryptoStream cs = new CryptoStream(fsWrite, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(cs, someObject);
                    }
                    catch (SerializationException e)
                    {
                        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts an object with its plain password.
        /// </summary>
        /// <param name="password"></param>
        public SomeObject ObjectDecrypt(string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            using (FileStream fsRead = new FileStream("DataFile.dat.aes", FileMode.Open, FileAccess.Read))
            {
                fsRead.Read(salt, 0, salt.Length);

                var AES = GetCrypto(passwordBytes, salt);

                using (CryptoStream cs = new CryptoStream(fsRead, AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        return (SomeObject)formatter.Deserialize(cs);
                    }
                    catch (SerializationException e)
                    {
                        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        private RijndaelManaged GetCrypto(byte[] passwordBytes, byte[] salt)
        {
            // Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            // Repeatedly hash the user password along with the salt. High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CFB;

            return AES;
        }
    }
}
