using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Eight.LuaLibs {
    public class FileSystem {
        public static readonly char[] IllegalChars = {
            '"',
            ':',
            '<',
            '>',
            '?',
            '|'
        };
        
        
        public static void Setup() {
            Console.WriteLine("Working in {0}", Directory.GetCurrentDirectory());

        }

        private static int OpenLib(IntPtr luaState) {
            return 1;
        }

        public static void Open(string path, string mode) {
            
        }

        public void MakeDir(string path) { }

        public void List(string path) { }

        public void Delete(string path) { }

        // god, spare me please
        
        private static string Resolve(string path) {
            string invalidChars = new string(IllegalChars);
            Regex r = new Regex($"[{Regex.Escape(invalidChars)}]");
            path = r.Replace(path, "");
            path = path.Replace("\\", "/");

            if (path.StartsWith("/")) {
                path = path.TrimStart('/');
            }

            string resolved = Path.GetFullPath(path, Path.GetPathRoot(Eight.BaseDir));
            resolved = resolved.Substring(2);
            resolved = r.Replace(resolved, "");
            string fullPath = Path.Join(Eight.BaseDir, resolved);
            return fullPath;
        }
    }
}