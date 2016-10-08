using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katas.Algorithms
{
    public class SpiralMatrix
    {
        /// <summary>
        /// What I learnt:
        /// * Switch case 
        /// * The way of thinking
        /// 
        /// Todo:
        /// * Abstract with functional programming
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int[] Run(int[][] input)
        {
            if (input == null || input.Length < 1 || input[0] == null || input[0].Length < 1)
                return null;

            var runCounter = 1;
            var side = 1;
            var topWall = 0;
            var rightWall = input[0].Length - 1;
            var bottomWall = input.Length - 1;
            var leftWall = 0;
            var spiral = new List<int>();

            while (topWall <= bottomWall && leftWall <= rightWall)
            {
                switch (side)
                {
                    case 1: //Top
                        for (var i = leftWall; i <= rightWall; i++) spiral.Add(input[topWall][i]);
                        topWall++;
                        break;
                    case 2: //Right
                        for (var i = topWall; i <= bottomWall; i++) spiral.Add(input[i][rightWall]);
                        rightWall--;
                        break;
                    case 3: //Bottom
                        for (var i = rightWall; i >= leftWall; i--) spiral.Add(input[bottomWall][i]);
                        bottomWall--;
                        break;
                    case 4: //Left
                        for (var i = bottomWall; i >= topWall; i--) spiral.Add(input[i][leftWall]);
                        leftWall++;
                        break;
                }

                runCounter++;
                side = runCounter % 4;
            }

            return spiral.ToArray();
        }
    }
}
