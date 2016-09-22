using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.Algorithms.UnitTests.ContainerWithMostWater_Tests
{
    [TestClass]
    public class When_ContainerWithMostWater_MaxArea
    {
        [TestMethod]
        public void Given_213_Then_4()
        {
            // Arrange
            var algo = new ContainerWithMostWater();

            // Action
            var actual = algo.MaxArea(new int[] { 2, 1, 3 });

            // Assert
            Assert.AreEqual(4, actual);
        }
    }
}
