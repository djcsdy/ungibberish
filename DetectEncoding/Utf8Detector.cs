using System;

namespace DetectEncoding
{
    internal class Utf8Detector : IDetector
    {
        private Utf8State _state = Utf8State.Start;
        private int _uncertainty = 65536;
        private int _multibytesRemaining;

        public State Consume(byte b)
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
                        _uncertainty = (_uncertainty*45824) >> 16;
                    }

                    break;
                case Utf8State.Multibyte:
                    if (b >= 0x80 && b <= 0xbf)
                    {
                        if (--_multibytesRemaining == 0)
                        {
                            _state = Utf8State.Start;
                        }
                        _uncertainty = (_uncertainty*16384) >> 16;
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

            var validity = _state == Utf8State.Error
                               ? Validity.Invalid
                               : Validity.Valid;

            return new State {Certainty = 65536 - _uncertainty, Validity = validity};
        }

        private enum Utf8State
        {
            Start,
            Multibyte,
            Error
        }
    }
}
