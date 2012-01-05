using System;

namespace Ungibberish.Detectors
{
    internal class Utf32Detector : IDetector
    {
        private const int CodePointProbability = (int) ((long) 1112064*Detector.MaxCertainty/4294967296);

        private readonly bool _bigEndian;
        private int _codePoint;
        private int _byteIndex;
        private int _uncertainty = Detector.MaxCertainty;

        public Utf32Detector(bool bigEndian)
        {
            IsValid = true;
            _bigEndian = bigEndian;
        }

        public void Consume(byte b)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Utf32Detector.Consume called while invalid");
            }

            if (_bigEndian)
            {
                _codePoint |= b << ((3 - _byteIndex)*8);
            }
            else
            {
                _codePoint |= b << (_byteIndex*8);
            }

            if (++_byteIndex == 4)
            {
                if ((_codePoint >= 0xd800 && _codePoint <= 0xdfff) || _codePoint > 0x10fff)
                {
                    IsValid = false;
                }
                else
                {
                    _uncertainty = (_uncertainty*CodePointProbability) >> Detector.MaxCertaintyBits;
                }
                _byteIndex = 0;
            }
        }

        public bool IsValid { get; private set; }

        public int Certainty { get { return Detector.MaxCertainty - _uncertainty; } }
    }
}
