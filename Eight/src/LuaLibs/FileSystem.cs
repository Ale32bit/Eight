using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KeraLua;

namespace Eight.LuaLibs {
    public class FileSystem {
        public static LuaRegister[] Fs_lib = {
            new LuaRegister {
                name = "open",
                function = Open,
            },
            new LuaRegister {
                name = "readFile",
                function = Read,
            },
            new LuaRegister {
                name = "writeFile",
                function = Write,
            },
            new LuaRegister {
                name = "makeDir",
                function = MakeDir,
            },
            new LuaRegister {
                name = "list",
                function = List,
            },
            new LuaRegister {
                name = "delete",
                function = Delete,
            },
            new LuaRegister {
                name = "exists",
                function = Exists,
            },
            new LuaRegister {
                name = "getType",
                function = GetType,
            },
            new LuaRegister(), // NULL
        };


        public static void Setup() {
            Console.WriteLine("Working in {0}", Directory.GetCurrentDirectory());

            Logic.Lua.LuaState.RequireF("filesystem", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(Fs_lib);
            return 1;
        }

        public static int Open(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            if (!state.IsString(1)) {
                state.Error(Utils.GenArgError(1, state.TypeName(1), "string"));
                return 0;
            }

            if (!state.IsString(2)) {
                state.Error(Utils.GenArgError(2, state.TypeName(2), "string"));
                return 0;
            }

            var path = state.ToString(1);
            var mode = state.ToString(2);

            var resolvedPath = Resolve(path);


            state.PushNil();
            state.PushNil();

            return 2;
        }

        public static int Read(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string path = state.ToString(1);
            string resolvedPath = Resolve(path);

            string text = File.ReadAllText(resolvedPath);

            state.PushString(text);

            return 1;
        }

        public static int Write(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string path = state.ToString(1);
            string resolvedPath = Resolve(path);

            string content = state.ToString(2);

            File.WriteAllText(resolvedPath, content);

            return 0;
        }

        public static int MakeDir(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            string error = "Unknown error";
            bool ok = true;

            string dirPath = state.ToString(1);
            string resolvedPath = Resolve(dirPath);

            if (PathExists(resolvedPath)) {
                ok = false;
                error = "File already exists";
            }
            else {
                try {
                    Directory.CreateDirectory(resolvedPath);
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    ok = false;
                    error = "Internal error";
                }
            }

            state.PushBoolean(ok);

            if (ok) {
                state.PushNil();
            }
            else {
                state.PushString(error);
            }

            return 2;
        }

        public static int List(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string path = state.ToString(1);
            string resolvedPath = Resolve(path);

            if (PathExists(resolvedPath)) {
                List<string> dirs = new List<string>(Directory.GetDirectories(resolvedPath));
                List<string> files = new List<string>(Directory.GetFiles(resolvedPath));
                string[] allFiles = dirs.Concat(files).ToArray();

                Array.Sort(allFiles);
                
                state.NewTable();
                for (int i = 1; i <= allFiles.Length; i++) {
                    state.PushString(Path.GetFileName(allFiles[i - 1]));
                    state.RawSetInteger(-2, i);
                }

                state.PushNil();
            }
            else {
                state.PushNil();
                state.PushString("Path does not exist");
            }

            return 2;
        }

        public static int GetType(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string path = state.ToString(1);
            string resolvedPath = Resolve(path);

            if (PathExists(resolvedPath)) {
                if (Directory.Exists(resolvedPath)) {
                    state.PushString("directory");
                }
                else if (File.Exists(resolvedPath)) {
                    state.PushString("file");
                }
                else {
                    state.PushString("unknown");
                }

                state.PushNil();
            }
            else {
                state.PushNil();
                state.PushString("Path not found");
            }

            return 2;
        }

        public static int Delete(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string path = state.ToString(1);
            bool recursive = false;
            if (state.IsBoolean(2)) {
                recursive = state.ToBoolean(2);
            }

            string resolvedPath = Resolve(path);

            bool ok = true;
            string error = "Unknown error";

            if (PathExists(resolvedPath)) {
                try {
                    if (Directory.Exists(resolvedPath)) {
                        // if it's path
                        Directory.Delete(resolvedPath, recursive);
                    }
                    else if (File.Exists(resolvedPath)) {
                        File.Delete(resolvedPath);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    ok = false;
                    error = "Internal error";
                }
            }
            else {
                ok = false;
                error = "File or directory not found";
            }

            if (ok) {
                state.PushBoolean(true);
                state.PushNil();
            }
            else {
                state.PushBoolean(false);
                state.PushString(error);
            }

            return 2;
        }

        public static int Exists(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string path = state.ToString(1);
            string resolvedPath = Resolve(path);
            
            state.PushBoolean(PathExists(resolvedPath));

            return 1;
        }

        private static bool PathExists(string path) {
            return File.Exists(path) || Directory.Exists(path);
        }

        // god, spare me please
        public static string Resolve(string path) {
            // Replace \ to / for cross compatibility in case users prefer to use \
            path = path.Replace("\\", "/");
            
            // Get drive root (C:\ for Windows, / for *nix)
            string rootPath = Path.GetFullPath(Path.GetPathRoot("/") ?? "/");
            
            // Join path to rootPath and resolves to absolute path
            // Relative paths are resolved here (es. ../ and ./)
            string absolutePath = Path.GetFullPath(path, rootPath);
            
            // Trim root from path
            string isolatedPath = absolutePath.Remove(0, rootPath.Length - 1);

            // Now join the isolatedPath to the Lua directory, always inside of it
            string resolvedPath = Path.Join(Eight.LuaDir, isolatedPath);

            return resolvedPath;
        }
    }
}