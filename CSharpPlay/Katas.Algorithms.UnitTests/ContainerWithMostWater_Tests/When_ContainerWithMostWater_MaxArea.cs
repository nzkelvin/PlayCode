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

        [TestMethod]
        public void Given_186254837_Then_49()
        {
            // Arrange
            var algo = new ContainerWithMostWater();

            // Action
            var actual = algo.MaxArea(new int[] { 1, 8, 6, 2, 5, 4, 8, 3, 7 });

            // Assert
            Assert.AreEqual(49, actual);
        }
    }
}
