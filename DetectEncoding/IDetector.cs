namespace DetectEncoding
{
    internal interface IDetector
    {
        State Consume(byte b);
    }
}
