namespace DetectEncoding
{
    internal interface IDetector
    {
        void Consume(byte b);
        Validity Validity { get; }
        int Certainty { get; }
    }
}
