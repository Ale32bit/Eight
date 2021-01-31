// WORK IN PROGRESS

using System;
using KeraLua;
using static SDL2.SDL;

namespace Eight.Module {
    class Audio {
        public static LuaRegister[] AudioLib = {
            new() {
                name = "beep",
                function = Beep,
            },
            new(),
        };

        public static void Setup() {
            Runtime.LuaState.RequireF("audio", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(AudioLib);
            return 1;
        }

        public static int Beep(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            int wave = (int)state.ToInteger(1);
            double freq = state.ToNumber(2);
            long duration = state.ToInteger(3);

            Console.WriteLine($"Should beep");

            return 0;
        }

        public static void PlayFrequency() {
        }

        public static unsafe void PlayWAV() {
            SDL_AudioSpec wavSpec = new();
            uint wavLength;
            IntPtr wavBuffer;

            //SDL_LoadWAV("Powerup5.wav", ref wavSpec, out wavBuffer, out wavLength);

            //SDL_AudioDeviceID deviceId = SDL_OpenAudioDevice(NULL, 0, &wavSpec, NULL, 0);
        }
    }
}
