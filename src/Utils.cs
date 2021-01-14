using KeraLua;

namespace Eight {
    public class Utils {
        public static unsafe byte[] CString(byte* s) {
            var end = s;
            while (*end != 0) ++end;

            byte[] o = new byte[end - s];
            for (var i = 0; i < o.Length; i++) o[i] = s[i];

            return o;
        }

        public static ulong ToULong(char c, int fg, int bg) {
            return ((ulong)bg << 40) | ((ulong)fg << 16) | c;
        }

        public static TextPoint ToTextPoint(ulong p) {

            char c = (char)(p & 0xffff);
            int fg = (int)(p >> 16) & 0xffffff;
            int bg = (int)(p >> 40) & 0xffffff;

            if (c == 0) c = ' ';

            TextPoint point = new() {
                Char = c,
                Foreground = fg,
                Background = bg,
                Source = p,
            };

            return point;
        }

        /* "also
         * your struct
         * eats too much memory
         * even if it's temporary"
         * ~byte-chan™
         */
        public struct TextPoint {
            public char Char;
            public int Foreground;
            public int Background;
            public ulong Source;
        }

        public struct LuaParameter {
            public LuaType Type;
            public object Value;
        }
    }
}