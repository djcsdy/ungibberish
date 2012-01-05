using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ungibberish
{
    public class EncodingDetector
    {
        private const string Bom = "\ufeff";

        private static readonly Encoding Utf32Le = new UTF32Encoding(false, false, true);
        private static readonly Encoding Utf32Be = new UTF32Encoding(true, false, true);
        private static readonly Encoding Utf16Le = new UnicodeEncoding(false, false, true);
        private static readonly Encoding Utf16Be = new UnicodeEncoding(true, false, true);
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

        private static readonly IEnumerable<KeyValuePair<byte[], Encoding>> ByteOrderMarks =
            new[]
                {
                    GetByteOrderMark(Utf32Le),
                    GetByteOrderMark(Utf32Be),
                    GetByteOrderMark(Utf16Le),
                    GetByteOrderMark(Utf16Be),
                    GetByteOrderMark(Utf8)
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

            IEnumerable<Encoding> encodings = tentativeEncoding == null
                                                  ? new Encoding[0]
                                                  : new[] {tentativeEncoding};
            encodings = encodings.Concat(ByteOrderMarks
                                             .Select(byteOrderMark => byteOrderMark.Value)
                                             .Where(encoding => encoding != tentativeEncoding));

            foreach (var encoding in encodings)
            {
                try
                {
                    encoding.GetString(text);
                }
                catch (DecoderFallbackException)
                {
                    continue;
                }

                return encoding;
            }

            return null;
        }
    }
}