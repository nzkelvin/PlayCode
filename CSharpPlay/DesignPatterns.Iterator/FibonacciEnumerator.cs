using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPatterns.Iterator
{
    public class FibonacciEnumerator : IEnumerator<int>
    {
        private int _numberOfValues;
        private int _currentPosition;
        private int _previousTotal;
        private int _currentTotal;

        public FibonacciEnumerator(int numberOfValues)
        {
            _numberOfValues = numberOfValues;
        }

        public int Current
        {
            get { return _currentTotal; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (_currentPosition == 0)
            {
                _currentTotal = 1;
            }
            else
            {
                int newTotal = _currentTotal + _previousTotal;
                _previousTotal = _currentTotal;
                _currentTotal = newTotal;
            }

            _currentPosition++;
            return _numberOfValues >= _currentPosition;
        }

        public void Reset()
        {
            _currentPosition = 0;
            _currentTotal = 0;
            _previousTotal = 0;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
