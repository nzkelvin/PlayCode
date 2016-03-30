using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.DES
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataToEncrypt = Encoding.UTF8.GetBytes("MyPassword");
            var des = new DesEncryption();
            var key = des.GenerateRandomNumber(8);
            var iv = des.GenerateRandomNumber(8);
            var encryptedData = des.Encrypt(dataToEncrypt, key, iv);

            Console.WriteLine(Encoding.UTF8.GetString(dataToEncrypt));
            Console.WriteLine(Encoding.UTF8.GetString(encryptedData));
            Console.ReadLine();
        }
    }
}
