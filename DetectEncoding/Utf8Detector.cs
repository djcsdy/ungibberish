using System;

namespace DetectEncoding
{
    internal class Utf8Detector : IDetector
    {
        private Utf8State _state = Utf8State.Start;
        private int _consumedCharacters = 0;
        private int _multibytes = 0;

        public State Consume(byte b)
        {
            // TODO BOM
            switch(_state)
            {
                case Utf8State.Start:
                    if (b <= 0x7f) // TODO non-printing characters
                    {
                        _consumedCharacters++;
                    }
                    else if (b >= 0xc2 && b <= 0xdf)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytes = 1;
                    }
                    else if (b >= 0xe0 && b <= 0xef)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytes = 2;
                    }
                    else if (b >= 0xf0 && b <= 0xf7)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytes = 3;
                    }
                    else if (b >= 0xf8 && b <= 0xfb)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytes = 4;
                    }
                    else if (b >= 0xfc && b <= 0xfd)
                    {
                        _state = Utf8State.Multibyte;
                        _multibytes = 5;
                    }
                    else
                    {
                        _state = Utf8State.Error;
                    }
                    break;
                case Utf8State.Multibyte:
                    if (b >= 0x80 && b <= 0xbf)
                    {
                        if (--_multibytes == 0)
                        {
                            _state = Utf8State.Start;
                            _consumedCharacters++;
                        }
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

            var certainty = 0; // TODO

            var validity = _state == Utf8State.Error
                               ? Validity.Invalid
                               : Validity.Valid;

            return new State {Certainty = certainty, Validity = validity};
        }

        private enum Utf8State
        {
            Start,
            Multibyte,
            Error
        }
    }
}
