namespace DetectEncoding.Detectors
{
    internal interface IDetector
    {
        void Consume(byte b);
        bool IsValid { get; }
        int Certainty { get; }
    }
}
