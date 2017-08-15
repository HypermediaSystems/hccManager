using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hcc
{
    public static class Bom
    {
        // got from https://stackoverflow.com/a/16315911
        public static int GetCursor(Byte[] bytes)
        {
            // UTF-32, big-endian
            if (IsMatch(bytes, new byte[] { 0x00, 0x00, 0xFE, 0xFF }))
                return 4;
            // UTF-32, little-endian
            if (IsMatch(bytes, new byte[] { 0xFF, 0xFE, 0x00, 0x00 }))
                return 4;
            // UTF-16, big-endian
            if (IsMatch(bytes, new byte[] { 0xFE, 0xFF }))
                return 2;
            // UTF-16, little-endian
            if (IsMatch(bytes, new byte[] { 0xFF, 0xFE }))
                return 2;
            // UTF-8
            if (IsMatch(bytes, new byte[] { 0xEF, 0xBB, 0xBF }))
                return 3;
            return 0;
        }

        private static bool IsMatch(Byte[] bytes, byte[] match)
        {
            var buffer = new byte[match.Length];
            Array.Copy(bytes, 0, buffer, 0, buffer.Length);
            return !buffer.Where((t, i) => t != match[i]).Any();
        }
    }
}
