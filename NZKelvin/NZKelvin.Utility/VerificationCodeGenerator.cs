using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZKelvin.Utility
{
    public class VerificationCodeGenerator
    {
        public string Generate()
        {
            var timeBytes = BitConverter.GetBytes(DateTime.UtcNow.ToBinary()); // BitConvert coverts primary type to byte[]
            var guidBytes = Guid.NewGuid().ToByteArray(); // Guid is a reference type. It has its own convert method.
            return Convert.ToBase64String(guidBytes.Concat(timeBytes).ToArray());
        }

        /// <summary>
        /// Use case: You want to expire the verification code in 24 hours. You can get time from the verification code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DateTime? GetGeneratedTime(string code)
        {
            var timeBytes = Convert.FromBase64String(code).ToArray();
            var timeBinary = BitConverter.ToInt64(timeBytes, 16);
            return DateTime.FromBinary(timeBinary);
        }
    }
}
