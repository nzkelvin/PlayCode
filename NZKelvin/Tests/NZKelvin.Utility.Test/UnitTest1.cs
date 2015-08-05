using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NZKelvin.Utility;

namespace NZKelvin.Utility.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var gen = new VerificationCodeGenerator();
            var code = gen.Generate();
            
            


            var time = gen.GetGeneratedTime(code);
        }
    }
}
