using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katas.Algorithms
{
    public class ContainerWithMostWater
    {
        public int MaxArea(int[] height)
        {
            // Has to be more than 2 lines
            if (height.Length <= 1)
                return 0;

            var leftPointer = 0;
            var rightPointer = height.Length - 1;
            var maxArea = 0;
            int width;

            while (leftPointer < rightPointer)
            {
                width = rightPointer - leftPointer;
                maxArea = Math.Max(maxArea, Math.Min(height[leftPointer], height[rightPointer]) * width);

                if (height[leftPointer] > height[rightPointer]) rightPointer--;
                else leftPointer++;
            }

            return maxArea;
        }
    }
}
