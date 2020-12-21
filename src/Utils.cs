#nullable enable
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

        public struct LuaParameter {
            public LuaType Type;
            public object Value;
        }

        public struct Coords {
            public int x;
            public int y;
            public int? w;
            public int? h;
        }
    }
}