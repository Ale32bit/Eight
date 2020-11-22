using System;
using System.Collections.Generic;
using System.Data;
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

        public string FilePath;
        public string OpenMode;

        public bool IsReadMode;
        public bool IsBinary = false;

        public StreamReader ReadStream;
        public FileStream WriteStream;

        public FileSystemHandle(string path, string mode) {
            FilePath = path;
            OpenMode = mode;

            if (!Modes.ContainsKey(mode)) {
                throw new Exception("Invalid mode");
            }
            
            if (Directory.Exists(path)) {
                throw new Exception("Path is a directory");
            }

            IsReadMode = mode == "r" || mode == "rb";
            
            if ((IsReadMode) && !File.Exists(path)) {
                throw new Exception("File not found");
            }
            
            IsBinary = mode == "rb" || mode == "wb" || mode == "ab";

            try {
                if (IsReadMode) {
                    ReadStream = new StreamReader(path);
                }
                else {
                    WriteStream = new FileStream(path, Modes[mode]);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        public int Read(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            if (!IsReadMode) {
                state.Error("File not open in read mode");
                return 0;
            }

            int data = ReadStream.Read();

            if (IsBinary) {
                state.PushNumber(data);
            }
            else {
                state.PushString(Convert.ToChar(data).ToString());
            }

            return 1;
        }

        public int ReadAll(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            if (!IsReadMode) {
                state.Error("File not open in read mode");
                return 0;
            }

            string data = ReadStream.ReadToEnd();
            
            state.PushString(data);

            return 1;
        }
        
        public int ReadLine(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            if (!IsReadMode) {
                state.Error("File not open in read mode");
                return 0;
            }

            string data = ReadStream.ReadLine();
            
            state.PushString(data);

            return 1;
        }

        public int Write(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            if (IsReadMode) {
                state.Error("File not open in write mode");
                return 0;
            }

            var data = state.ToBuffer(1);
            
            WriteStream.Write(data);

            return 0;
        }

        public int Close(IntPtr luaState) {
            WriteStream.Dispose();
            return 1;
        }

        public int Flush(IntPtr luaState) {
            WriteStream.Flush();
            return 1;
        }

        public LuaRegister[] Export() {
            List<LuaRegister> functions = new List<LuaRegister>();

            if (IsReadMode) {
                functions.Add(new LuaRegister {
                    name = "read",
                    function = Read,
                });
                
                functions.Add(new LuaRegister {
                    name = "readAll",
                    function = ReadAll,
                });
                
                functions.Add(new LuaRegister {
                    name = "readLine",
                    function = ReadLine,
                });
            }
            else {
                functions.Add(new LuaRegister {
                    name = "write",
                    function = Write,
                });
            }

            functions.Add(new LuaRegister {
                name = "close",
                function = Close,
            });
            
            functions.Add(new LuaRegister {
                name = "flush",
                function = Flush,
            });
            
            return functions.ToArray();
        }
    }
}