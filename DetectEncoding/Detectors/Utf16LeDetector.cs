using System;

namespace DetectEncoding.Detectors
{
    internal class Utf16LeDetector : IDetector
    {
        private const int InitialWordProbability = (int) ((long) 64512*Detector.MaxCertainty/65536);
        private const int SurrogateWordProbability = 1024*Detector.MaxCertainty/65536;

        private bool _isEven;
        private byte _littleEnd;
        private bool _isSurrogate;
        private int _uncertainty = Detector.MaxCertainty;

        public void Consume(byte b)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Utf16LeDetector.Consume called while invalid");
            }

            if (_isEven)
            {
                var value = (b << 8) | _littleEnd;
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
                _littleEnd = b;
            }

            _isEven = !_isEven;
        }

        public bool IsValid { get; private set; }

        public int Certainty { get { return Detector.MaxCertainty - _uncertainty; } }
    }
}
