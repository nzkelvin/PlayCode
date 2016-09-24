using Microsoft.VisualStudio.TestTools.UnitTesting;
using Katas.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katas.Algorithms.NextGreaterPermutation_Tests
{
    [TestClass()]
    public class When_NextGreaterPermutation_NextPermutation
    {
        //[TestMethod()]
        public void Given_5264_Then_5462()
        {
            //Arrange
            var permutationGenerator = new NextGreaterPermutation();
            var actual = new int[] { 7, 2, 6, 5, 4 };
            var expected = new int[] { 7, 4, 6, 5, 2 };

            //Action
            permutationGenerator.NextPermutation(actual);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Given_132_Then_213() // All digits swapped
        {
            //Arrange
            var permutationGenerator = new NextGreaterPermutation();
            var actual = new int[] { 1, 3, 2 };
            var expected = new int[] { 2, 1, 3 };

            //Action
            permutationGenerator.NextPermutation(actual);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Given_123_Then_132() // partial digits swapped
        {
            //Arrange
            var permutationGenerator = new NextGreaterPermutation();
            var actual = new int[] { 1, 2, 3 };
            var expected = new int[] { 1, 3, 2 };

            //Action
            permutationGenerator.NextPermutation(actual);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        public void Given_NoNextGreaterPermutation321_Then_ReturnMinimumPermutation123()
        {
            //Arrange
            var permutationGenerator = new NextGreaterPermutation();
            var actual = new int[] { 3, 2, 1 };
            var expected = new int[] { 1, 2, 3 };

            //Action
            permutationGenerator.NextPermutation(actual);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Given_231_Then_312()
        {
            //Arrange
            var permutationGenerator = new NextGreaterPermutation();
            var actual = new int[] { 2, 3, 1 };
            var expected = new int[] { 3, 1, 2 };

            //Action
            permutationGenerator.NextPermutation(actual);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test 2 same value digits
        /// </summary>
        [TestMethod()]
        public void Given_151_Then_511()
        {
            //Arrange
            var permutationGenerator = new NextGreaterPermutation();
            var actual = new int[] { 1, 5, 1 };
            var expected = new int[] { 5, 1, 1 };

            //Action
            permutationGenerator.NextPermutation(actual);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}