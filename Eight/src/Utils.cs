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
    }
}