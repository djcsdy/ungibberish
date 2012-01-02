using System;

namespace DetectEncoding
{
    internal class Utf8Detector : IDetector
    {
        private const int InitialByteProbability = (int) (0.7*Detector.MaxCertainty);
        private const int ContinuationByteProbability = (int) (0.25*Detector.MaxCertainty);

        private Utf8State _state = Utf8State.Start;
        private int _uncertainty = Detector.MaxCertainty;
        private int _multibytesRemaining;

        public void Consume(byte b)
        {
            // TODO BOM
            switch(_state)
            {
                case Utf8State.Start:
                    if (b >= 0xc2 && b <= 0xdf)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytesRemaining = 1;
                    }
                    else if (b >= 0xe0 && b <= 0xef)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytesRemaining = 2;
                    }
                    else if (b >= 0xf0 && b <= 0xf4)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytesRemaining = 3;
                    }
                    else if (b >= 0x80)
                    {
                        _state = Utf8State.Error;
                    }

                    if (_state != Utf8State.Error)
                    {
                        _uncertainty = (_uncertainty*InitialByteProbability) >> Detector.MaxCertaintyBits;
                    }

                    break;
                case Utf8State.Multibyte:
                    if (b >= 0x80 && b <= 0xbf)
                    {
                        if (--_multibytesRemaining == 0)
                        {
                            _state = Utf8State.Start;
                        }
                        _uncertainty = (_uncertainty*ContinuationByteProbability) >> Detector.MaxCertaintyBits;
                    }
                    else
                    {
                        _state = Utf8State.Error;
                    }
                    break;
                case Utf8State.Error:
                    throw new InvalidOperationException("Utf8Detector.Consume called when in error");
                default:
                    throw new InvalidOperationException("Unknown UTF-8 state");
            }
        }

        public Validity Validity
        {
            get
            {
                return _state == Utf8State.Error
                           ? Validity.Invalid
                           : Validity.Valid;
            }
        }

        public int Certainty
        {
            get
            {
                int maxCertainty = Detector.MaxCertainty;
                return _state == Utf8State.Error
                           ? maxCertainty
                           : maxCertainty - _uncertainty;
            }
        }

        private enum Utf8State
        {
            Start,
            Multibyte,
            Error
        }
    }
}
