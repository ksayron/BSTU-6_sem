using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Entities
{
    public class Reflector
    {
        private readonly string _wiring;

        public Reflector(string wiring)
        {
            _wiring = wiring;
        }

        public int Reflect(int input)
        {
            return _wiring[input] - 'A';
        }
    }
}
