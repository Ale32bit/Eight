using KeraLua;
using System;
using System.IO;

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

        [Flags]
        public enum TextFlag : byte {
            None = 0,
            Underlined = 1,
            Strikethrough = 2,
            Bold = 4,
            Reversed = 8,
            Blinking = 16,
            Mirrored = 32,
            UpsideDown = 64,

        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if ( !dir.Exists ) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach ( FileInfo file in files ) {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if ( copySubDirs ) {
                foreach ( DirectoryInfo subdir in dirs ) {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}