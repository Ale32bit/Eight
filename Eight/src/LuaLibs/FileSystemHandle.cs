using System;
using System.Collections.Generic;
using System.IO;
using KeraLua;

namespace Eight.LuaLibs {
    public class FileSystemHandle {
        public static readonly Dictionary<string, FileMode> Modes = new Dictionary<string, FileMode> {
            {"r", FileMode.Open}, // Read
            {"rb", FileMode.Open}, // Read binary
            {"w", FileMode.OpenOrCreate}, // Write
            {"wb", FileMode.OpenOrCreate}, // Write binary
            {"a", FileMode.Append}, // Append
            {"ab", FileMode.Append}, // Append binary
        };

        public string openMode;
        public string filePath;
        public bool isBinary = false;

        public FileStream Stream;

        FileSystemHandle(string path, string mode) {
            filePath = path;
            openMode = mode;

            if (!Modes.ContainsKey(mode)) {
                throw new Exception("Invalid mode");
            }
            
            Stream = new FileStream(path, Modes[mode]);
        }

        public int Read(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);


            return 1;
        }

        public int Write(IntPtr luaState) {
            return 1;
        }

        public int Close(IntPtr luaState) {
            return 1;
        }

        public int Flush(IntPtr luaState) {
            return 1;
        }
    }
}