using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katas.Algorithms
{
    public class NextGreaterPermutation
    {
        public void NextPermutation(int[] nums)
        {
            if (nums.Length <= 1)
                return;

            // find a[i-1] < a[i]
            var i = nums.Length - 1;

            while (i > 0 && nums[i - 1] >= nums[i])  i--;

            //if (i == 0 && nums[i] >= nums[i + 1]) reverse(nums);
            var j = i;
            if (i > 0 && nums[i - 1] < nums[i])
            {
                // find j where a[j] just bigger than a[i - 1]
                //var j = i;

                while (j < nums.Length && nums[i - 1] < nums[j]) j++;

                swap(nums, i - 1, j - 1);
            }
            //else i--;

            reverse(nums, i, nums.Length - 1);
        }

        private void swap(int[] nums, int leftIdx, int rightIdx)
        {
            var temp = nums[leftIdx];
            nums[leftIdx] = nums[rightIdx];
            nums[rightIdx] = temp;
        }

        private void reverse(int[] nums, int leftIdx, int rightIdx)
        {
            //var leftIdx = 0;
            //var rightIdx = nums.Length - 1;

            while (leftIdx < rightIdx)
            {
                swap(nums, leftIdx, rightIdx);
                leftIdx++;
                rightIdx--;
            }
        }
    }
}
