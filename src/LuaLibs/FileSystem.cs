using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lua = Eight.Logic.Lua;

namespace Eight.LuaLibs {
    public class FileSystem {
        public static LuaRegister[] FileSystemLib = {
            new() {
                name = "open",
                function = Open
            },
            new() {
                name = "makeDir",
                function = MakeDir
            },
            new() {
                name = "list",
                function = List
            },
            new() {
                name = "delete",
                function = Delete
            },
            new() {
                name = "exists",
                function = Exists
            },
            new() {
                name = "getType",
                function = GetType
            },
            new() {
                name = "move",
                function = Move,
            },
            new() // NULL
        };

        public static void Setup() {
            Console.WriteLine("Working in {0}", Directory.GetCurrentDirectory());

            Lua.LuaState.RequireF("filesystem", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);
            state.NewLib(FileSystemLib);
            return 1;
        }

        public static int Open(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");
            state.ArgumentCheck(state.IsString(2), 2, "expected string");

            var path = state.ToString(1);
            var mode = state.ToString(2);

            var resolvedPath = Resolve(path);

            state.GetField(LuaRegistry.Index, "_io_open");
            state.PushString(resolvedPath);
            state.PushString(mode);
            state.Call(2, 1);

            return 1;
        }

        public static int MakeDir(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);
            var error = "Unknown error";
            var ok = true;

            state.ArgumentCheck(state.IsString(1), 1, "expected string");

            var dirPath = state.ToString(1);
            var resolvedPath = Resolve(dirPath);

            if (PathExists(resolvedPath)) {
                ok = false;
                error = "File already exists";
            } else {
                try {
                    Directory.CreateDirectory(resolvedPath);
                } catch (Exception e) {
                    Console.WriteLine(e);
                    ok = false;
                    error = "Internal error";
                }
            }

            state.PushBoolean(ok);

            if (ok)
                state.PushNil();
            else
                state.PushString(error);

            return 2;
        }

        public static int List(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");

            var path = state.ToString(1);
            var resolvedPath = Resolve(path);

            if (PathExists(resolvedPath)) {
                var dirs = new List<string>(Directory.GetDirectories(resolvedPath));
                var files = new List<string>(Directory.GetFiles(resolvedPath));
                var allFiles = dirs.Concat(files).ToArray();

                Array.Sort(allFiles);

                state.NewTable();
                for (var i = 1; i <= allFiles.Length; i++) {
                    state.PushString(Path.GetFileName(allFiles[i - 1]));
                    state.RawSetInteger(-2, i);
                }

                state.PushNil();
            } else {
                state.PushNil();
                state.PushString("Path does not exist");
            }

            return 2;
        }

        public static int GetType(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");

            var path = state.ToString(1);
            var resolvedPath = Resolve(path);

            if (PathExists(resolvedPath)) {
                if (Directory.Exists(resolvedPath))
                    state.PushString("directory");
                else if (File.Exists(resolvedPath))
                    state.PushString("file");
                else
                    state.PushString("unknown");

                state.PushNil();
            } else {
                state.PushNil();
                state.PushString("Path not found");
            }

            return 2;
        }

        public static int Delete(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");

            var path = state.ToString(1);
            var recursive = false;
            if (state.IsBoolean(2)) recursive = state.ToBoolean(2);

            var resolvedPath = Resolve(path);

            var ok = true;
            var error = "Unknown error";

            if (PathExists(resolvedPath)) {
                try {
                    if (Directory.Exists(resolvedPath)) // if it's path
                        Directory.Delete(resolvedPath, recursive);
                    else if (File.Exists(resolvedPath)) File.Delete(resolvedPath);
                } catch (Exception e) {
                    Console.WriteLine(e);
                    ok = false;
                    error = "Internal error";
                }
            } else {
                ok = false;
                error = "File or directory not found";
            }

            if (ok) {
                state.PushBoolean(true);
                state.PushNil();
            } else {
                state.PushBoolean(false);
                state.PushString(error);
            }

            return 2;
        }

        public static int Exists(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");

            var path = state.ToString(1);
            var resolvedPath = Resolve(path);

            state.PushBoolean(PathExists(resolvedPath));

            return 1;
        }

        public static int Move(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");
            state.ArgumentCheck(state.IsString(2), 2, "expected string");
            state.ArgumentCheck(state.IsBoolean(3) || state.IsNoneOrNil(3), 3, "expected boolean, nil");

            var source = state.ToString(1);
            var destination = state.ToString(2);
            var overwrite = state.ToBoolean(3) || false;

            var resolvedSource = Resolve(source);
            var resolvedDestination = Resolve(destination);

            if (File.Exists(resolvedSource)) {
                // If it's a file
                if (File.Exists(resolvedDestination) && !overwrite) {
                    state.PushBoolean(false);
                    state.PushString("Destination path already exists");
                } else {
                    try {
                        File.Move(resolvedSource, resolvedDestination, overwrite);
                        state.PushBoolean(true);
                        state.PushNil();
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        state.PushBoolean(false);
                        state.PushString("Internal error");
                    }
                }
            } else if (Directory.Exists(resolvedSource)) {
                if (File.Exists(resolvedDestination)) {
                    state.PushBoolean(false);
                    state.PushString("Destination path is a file");
                } else {
                    Directory.Move(resolvedSource, resolvedDestination);
                    state.PushBoolean(true);
                    state.PushNil();
                }
            } else {
                state.PushBoolean(false);
                state.PushString("Path not found");
            }

            return 2;
        }

        public static bool PathExists(string path) {
            return File.Exists(path) || Directory.Exists(path);
        }

        // god, spare me please
        public static string Resolve(string path) {
            // Replace \ to / for cross compatibility in case users prefer to use \
            path = path.Replace("\\", "/");

            // Get drive root (C:\ for Windows, / for *nix)
            var rootPath = Path.GetFullPath(Path.GetPathRoot("/") ?? "/");

            // Join path to rootPath and resolves to absolute path
            // Relative paths are resolved here (es. ../ and ./)
            var absolutePath = Path.GetFullPath(path, rootPath);

            // Trim root from path
            var isolatedPath = absolutePath.Remove(0, rootPath.Length - 1);

            // Now join the isolatedPath to the Lua directory, always inside of it
            var resolvedPath = Path.Join(Eight.LuaDir, isolatedPath);

            return resolvedPath;
        }
    }
}