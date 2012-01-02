using System;
using System.Collections.Generic;

namespace DetectEncoding.Detectors
{
    internal class EightBitDetector : IDetector
    {
        private readonly ISet<byte> _validBytes = new HashSet<byte>();
        private readonly int _byteProbability;
        private int _uncertainty = Detector.MaxCertainty;

        public EightBitDetector(IEnumerable<byte> validBytes)
        {
            foreach (var validByte in validBytes)
            {
                if (_validBytes.Contains(validByte))
                {
                    throw new ArgumentException("contains duplicate values", "validBytes");
                }

                _validBytes.Add(validByte);
            }

            _byteProbability = _validBytes.Count*Detector.MaxCertainty/256;

            IsValid = true;
        }

        public void Consume(byte b)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Utf32Detector.Consume called while invalid");
            }

            if (_validBytes.Contains(b))
            {
                _uncertainty = (_byteProbability*_uncertainty) >> Detector.MaxCertaintyBits;
            }
            else
            {
                IsValid = false;
            }
        }

        public bool IsValid { get; private set; }

        public int Certainty { get { return Detector.MaxCertainty - _uncertainty; } }
    }
}
