using System;

namespace Ungibberish.Detectors
{
    internal class Utf16Detector : IDetector
    {
        private const int InitialWordProbability = (int) ((long) 64512*Detector.MaxCertainty/65536);
        private const int SurrogateWordProbability = 1024*Detector.MaxCertainty/65536;

        private readonly bool _bigEndian;
        private bool _isEven;
        private byte _end;
        private bool _isSurrogate;
        private int _uncertainty = Detector.MaxCertainty;

        public Utf16Detector(bool bigEndian)
        {
            _bigEndian = bigEndian;
        }

        public void Consume(byte b)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Utf16Detector.Consume called while invalid");
            }

            if (_isEven)
            {
                var value = _bigEndian
                                ? (_end << 8) | b
                                : (b << 8) | _end;

                if (_isSurrogate)
                {
                    if (value < 0xdc00 | value > 0xdfff)
                    {
                        IsValid = false;
                    }
                    else
                    {
                        _uncertainty = (_uncertainty * SurrogateWordProbability) >> Detector.MaxCertaintyBits;
                    }
                }
                else
                {
                    if (value >= 0xd800 && value <= 0xdbff)
                    {
                        _isSurrogate = true;
                    }
                    else if (value >= 0xdc00 && value <= 0xdfff)
                    {
                        IsValid = false;
                    }

                    if (IsValid)
                    {
                        _uncertainty = (_uncertainty*InitialWordProbability) >> Detector.MaxCertaintyBits;
                    }
                }
            }
            else
            {
                _end = b;
            }

            _isEven = !_isEven;
        }

        public bool IsValid { get; private set; }

        public int Certainty { get { return Detector.MaxCertainty - _uncertainty; } }
    }
}
