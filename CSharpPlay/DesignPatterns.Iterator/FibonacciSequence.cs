using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPatterns.Iterator
{
    public class FibonacciSequence : IEnumerable<int>
    {
        public int NumberOfValues { get; set; }

        public FibonacciSequence(int numberOfValues)
        {
            NumberOfValues = numberOfValues;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new FibonacciEnumerator(NumberOfValues);
        }

        // Because IEnumerable<T> inherits from IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
