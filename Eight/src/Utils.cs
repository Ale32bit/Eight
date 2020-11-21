using System;
using System.Linq;
using System.Text;

namespace Eight {
    public class Utils {
        public static unsafe byte[] CString(byte* s) {
            byte* end = s;
            while (*end != 0) ++end;

            byte[] o = new byte[end - s];
            for (var i = 0; i < o.Length; i++) {
                o[i] = s[i];
            }

            return o;
        }

        /// <summary>
        /// Generate an argument exception string
        /// </summary>
        /// <param name="index">Index of the argument</param>
        /// <param name="got">Type gotten</param>
        /// <param name="expectedArray">Types expected...</param>
        /// <returns>Formatted argument exception message</returns>
        public static string GenArgError(int index, string got, params string[] expectedArray) {
            // bad argument #%d (expected %s, got %s)

            var expected = string.Join(", ", expectedArray);

            return $"bad argument #{index} (expected {expected}, got {got})";
        }
    }
}