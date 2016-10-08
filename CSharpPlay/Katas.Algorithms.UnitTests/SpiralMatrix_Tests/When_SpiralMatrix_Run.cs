using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.Algorithms.UnitTests.SpiralMatrix_Tests
{
    [TestClass]
    public class When_SpiralMatrix_Run
    {
        [TestMethod]
        public void Given_3By3Matrix_Then_ProduceASpiralArray()
        {
            // Arrange
            int[][] input = new []{
                                new []{ 1, 2, 3 },
                                new []{ 4, 5, 6 },
                                new []{ 7, 8, 9 }
                              };

            var expected = new[] { 1, 2, 3, 6, 9, 8, 7, 4, 5 };

            // Act
            var spiralGenerator = new SpiralMatrix();
            var actual = spiralGenerator.Run(input);

            // Assert
            CollectionAssert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Given_NullMatrix_Then_ProduceNull()
        {
            // Arrange
            int[][] input = null;

            int[][] expected = null;

            // Act
            var spiralGenerator = new SpiralMatrix();
            var actual = spiralGenerator.Run(input);

            // Assert
            CollectionAssert.AreEqual(expected, actual);

        }
    }
}
