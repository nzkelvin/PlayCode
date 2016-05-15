using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Cryptography.DES
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add(@"http://bluepepper.co.nz/edi/interfaces/orders", @"EDIOrderListSubmit.xsd");

            XDocument doc = XDocument.Load(@"EDIOrderDummySample.xml");
            bool validationErrors = false;

            doc.Validate(schema, (s, e) =>
            {
                Console.WriteLine(e.Message);
                validationErrors = true;
            });

            if (validationErrors)
                Console.WriteLine("Validation failed");
            else
                Console.WriteLine("Validation successed");

            //TestDesEncryption();
            Console.ReadLine();
        }

        private static void TestDesEncryption()
        {
            var dataToEncrypt = Encoding.UTF8.GetBytes("MyPassword");
            var des = new DesEncryption();
            var key = des.GenerateRandomNumber(8);
            var iv = des.GenerateRandomNumber(8);
            var encryptedData = des.Encrypt(dataToEncrypt, key, iv);

            Console.WriteLine(Encoding.UTF8.GetString(dataToEncrypt));
            Console.WriteLine(Encoding.UTF8.GetString(encryptedData));
        }
    }
}
