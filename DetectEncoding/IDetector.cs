namespace DetectEncoding
{
    internal interface IDetector
    {
        void Consume(byte b);
        bool IsValid { get; }
        int Certainty { get; }
    }
}
