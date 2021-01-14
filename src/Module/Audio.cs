// WORK IN PROGRESS

using System;

namespace Eight.Module {
    class Audio {
        public static KeraLua.LuaRegister[] AudioLib = {
            /*new() {
                name = "beep",
                function = Beep,
            },
            new() {
                name = "loadWAV",
                function = LoadWAV,
            },*/
            new(),
        };

        public static void Setup() {
            Runtime.LuaState.RequireF("audio", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);
            state.NewLib(AudioLib);
            return 1;
        }

        public static int Beep(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            double freq = state.ToNumber(1);
            long duration = state.ToInteger(1);


            return 0;
        }

        public static unsafe int LoadWAV(IntPtr luaState) {
            /*var state = KeraLua.Lua.FromIntPtr(luaState);

            var path = state.ToString(1);

            path = FileSystem.Resolve(path);

            if(!FileSystem.PathExists(path)) {
                return 0;
            }

            SDL_AudioSpec? spec = null;

            IntPtr buf;
            uint len;

            SDL_AudioSpec wav = SDL_LoadWAV(path, ref spec, out buf, out len);


            var deviceId = SDL_OpenAudioDevice(null, 0, ref wav, out _, 0);

            if(SDL_QueueAudio(deviceId, buf, len) != 0) {

            }
            SDL_PauseAudioDevice(deviceId, 0);

            SDL_CloseAudioDevice(deviceId);
            SDL_FreeWAV(buf);


            //state.Push*/

            return 0;
        }

        public static void PlayFrequency() {

        }
    }
}
