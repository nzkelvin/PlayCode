using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    /// <summary>
    /// todo: use buying stock scenario
    /// todo: extend to func
    /// todo: record tutorial
    /// </summary>
    public class EventRedux
    {
        private static int THRESHOLD = 5;
        private static event ThresholdReachedDelegate thresholdBreachHandler;

        public static void EventDemo()
        {
            thresholdBreachHandler += OnThresholdReached;

            IncreaseValue(thresholdBreachHandler);
        }

        private static void IncreaseValue(ThresholdReachedDelegate reachHandler)
        {
            for (var i = 0; i < 10; i++)
            {
                if (THRESHOLD < i)
                {
                    reachHandler(i);
                }
            }
        }

        private static void OnThresholdReached(int value)
        {
            Console.WriteLine("value {0} has exceeded the threshold", value);
        }
    }

    public delegate void ThresholdReachedDelegate(int value);
}
