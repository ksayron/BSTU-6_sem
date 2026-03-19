using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6.Entities
{
    public class EnigmaMachine
    {
        private readonly Rotor _left;
        private readonly Rotor _middle;
        private readonly Rotor _right;
        private readonly Reflector _reflector;

        // Запоминаем обороты предыдущего шага для сравнения
        private int _prevMiddleRotations;
        private int _prevRightRotations;

        public EnigmaMachine(Rotor left, Rotor middle, Rotor right, Reflector reflector)
        {
            _left = left;
            _middle = middle;
            _right = right;
            _reflector = reflector;
        }

        private void StepRotors()
        {
            // R шагает всегда
            _right.Step();

            // M шагает когда R совершил новый оборот
            if (_right.FullRotations > _prevRightRotations)
            {
                _prevRightRotations = _right.FullRotations;
                _middle.Step();
            }

            // L шагает когда M совершил новый оборот
            if (_middle.FullRotations > _prevMiddleRotations)
            {
                _prevMiddleRotations = _middle.FullRotations;
                _left.Step();
            }
        }

        public char EncryptChar(char c)
        {
            StepRotors();

            int signal = c - 'A';

            // Прямой проход R → M → L
            signal = _right.Forward(signal);
            signal = _middle.Forward(signal);
            signal = _left.Forward(signal);

            // Рефлектор
            signal = _reflector.Reflect(signal);

            // Обратный проход L → M → R
            signal = _left.Backward(signal);
            signal = _middle.Backward(signal);
            signal = _right.Backward(signal);

            return (char)('A' + signal);
        }

        public string Encrypt(string message)
        {
            return new string(message.ToUpper()
                .Where(char.IsLetter)
                .Select(EncryptChar)
                .ToArray());
        }

        public void Reset()
        {
            _left.Reset();
            _middle.Reset();
            _right.Reset();
            _prevRightRotations = 0;
            _prevMiddleRotations = 0;
        }
    }
}
