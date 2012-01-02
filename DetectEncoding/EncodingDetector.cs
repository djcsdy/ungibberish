using System.Collections.Generic;
using System.Text;

namespace DetectEncoding
{
    public class EncodingDetector
    {
        private const string Bom = "\ufeff";

        private static readonly IEnumerable<KeyValuePair<byte[], Encoding>> ByteOrderMarks =
            new[]
                {
                    GetByteOrderMark(Encoding.UTF32), // UTF32-LE
                    GetByteOrderMark(Encoding.GetEncoding(12001)), // UTF32-BE
                    GetByteOrderMark(Encoding.Unicode), // UTF16-LE
                    GetByteOrderMark(Encoding.BigEndianUnicode), // UTF16-BE
                    GetByteOrderMark(Encoding.UTF8)
                };

        private static KeyValuePair<byte[], Encoding> GetByteOrderMark(Encoding encoding)
        {
            return new KeyValuePair<byte[], Encoding>(encoding.GetBytes(Bom), encoding);
        }

        public Encoding DetectEncoding(byte[] text)
        {
            Encoding tentativeEncoding = null;

            foreach (var byteOrderMark in ByteOrderMarks)
            {
                if (text.Length < byteOrderMark.Key.Length)
                {
                    continue;
                }

                for (var i = 0; i < byteOrderMark.Key.Length; ++i)
                {
                    if (text[i] != byteOrderMark.Key[i])
                    {
                        continue;
                    }
                }

                tentativeEncoding = byteOrderMark.Value;
                break;
            }


        }
    }
}