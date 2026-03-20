using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Entities
{
    public class Rotor
    {
        private readonly string _wiring;
        private readonly int _stepSize;
        private int _initialPosition;

        public int Position { get; private set; }
        public int FullRotations { get; private set; }

        public Rotor(string wiring, int stepSize, int initialPosition = 0)
        {
            _wiring = wiring;
            _stepSize = stepSize;
            _initialPosition = initialPosition;
            Position = initialPosition;
        }

        public void Step()
        {
            int prev = Position;
            Position = (Position + _stepSize) % 26;

            if (Position < prev)
                FullRotations++;
        }

        public int Forward(int input)
        {
            int index = (input + Position) % 26;
            int substituted = _wiring[index] - 'A';
            return (substituted - Position + 26) % 26;
        }

        public int Backward(int input)
        {
            int index = (input + Position) % 26;
            int wiringIndex = _wiring.IndexOf((char)('A' + index));
            return (wiringIndex - Position + 26) % 26;
        }

        public void Reset()
        {
            Position = _initialPosition;
            FullRotations = 0;
        }
    }
}
