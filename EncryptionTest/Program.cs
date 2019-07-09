using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EncryptionTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter choice: \n" +
                    "1 - Serialize \n" +
                    "2 - Deserialize \n" +
                    "3 - Encrypt \n" +
                    "4 - Decrypt \n" +
                    "5 - Serialize and Encrypt \n" +
                    "6 - Decrypt and Deserialize \n");
                string input = Console.ReadLine();
                int.TryParse(input, out var number);

                string password;
                switch (number)
                {
                    case 1:
                        Serialize();
                        break;
                    case 2:
                        Deserialize();
                        break;
                    case 3:
                        password = PasswordPrompt();
                        Encrypt(password);
                        break;
                    case 4:
                        password = PasswordPrompt();
                        Decrypt(password);
                        break;
                    case 5:
                        password = PasswordPrompt();
                        SerializeEncrypted(password);
                        break;
                    case 6:
                        password = PasswordPrompt();
                        DeserializeEncrypted(password);
                        break;

                }
            }
        }

        static string PasswordPrompt()
        {
            Console.WriteLine("Enter password: ");
            return Console.ReadLine();
        }

        static void Serialize()
        {
            var someObject = new SomeObject();
            someObject.Name = "Test Name";
            someObject.Value = 1234;

            using (FileStream fs = new FileStream("DataFile.dat", FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, someObject);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                }
            }
        }

        static void Deserialize()
        {
            using (FileStream fs = new FileStream("DataFile.dat", FileMode.Open))
            {
                SomeObject someObject;
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    someObject = (SomeObject)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    return;
                }

                Console.WriteLine("Name: {0}, Value: {1}.", someObject.Name, someObject.Value);
            }
        }

        static void Encrypt(string password)
        {
            var encryptor = new Encryptor();
            encryptor.FileEncrypt("DataFile.dat", password);
        }

        static void Decrypt(string password)
        {
            var encryptor = new Encryptor();
            encryptor.FileDecrypt("DataFile.dat.aes", "DataFile.dat", password);
        }

        static void SerializeEncrypted(string password)
        {
            var someObject = new SomeObject();
            someObject.Name = "Test Name";
            someObject.Value = 1234;

            var encryptor = new Encryptor();

            try
            {
                encryptor.ObjectEncrypt(someObject, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void DeserializeEncrypted(string password)
        {
            var encryptor = new Encryptor();
            try
            {
                var someObject = encryptor.ObjectDecrypt(password);
                Console.WriteLine("Name: {0}, Value: {1}.", someObject.Name, someObject.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}