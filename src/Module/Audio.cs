using KeraLua;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static SDL2.SDL;

namespace Eight.Module {
    class Audio {
        public const int FREQUENCY = 48000;
        public const int CHANNELS = 2;
        public const int SAMPLES = 1024;
        private static LuaRegister[] AudioLib = {
            new() {
                name = "beep",
                function = Beep,
            },
            new() {
                name = "reset",
                function = (state) => {
                    Reset();
                    return 0;
                },
            },
            new(),
        };
        private static bool ready = false;
        private static uint adev;
        private static SDL_AudioSpec aspec;
        private static List<AudioCallback> callbacks = new();
        unsafe delegate bool AudioCallback(float* samples, int count);

        private static void AudioHandler(IntPtr userdata, IntPtr stream, int len) {
            unsafe {
                float* buf = (float*)stream;
                Unsafe.InitBlockUnaligned((void*)stream, 0, (uint)len);
                if ( callbacks.Count < 1 ) return;
                var count = len / sizeof(float);
                float* tmp = stackalloc float[count];
                Span<float> span = new Span<float>(tmp, count);
                for ( int i = 0; i < callbacks.Count; ) {
                    span.Clear();
                    var cb = callbacks[i];
                    if ( cb(tmp, count) ) {
                        for ( int j = 0; j < count; j++ ) {
                            buf[j] += tmp[j];
                        }
                        i++;
                    } else {
                        callbacks.RemoveAt(i);
                    }
                }
            }
        }

        public static void InitAudio() {
            if ( ready ) return;
            ready = true;

            Console.WriteLine("Initiating audio lib...");

            aspec = new() {
                freq = FREQUENCY,
                format = AUDIO_F32SYS,
                channels = CHANNELS,
                callback = AudioHandler,
                userdata = IntPtr.Zero,
                samples = SAMPLES
            };
            SDL_AudioSpec have;
            adev = SDL_OpenAudioDevice(null, 0, ref aspec, out have, 0);
            if ( adev == 0 ) throw new Exception($"failed to open audio device: {SDL_GetError()}");
            callbacks.Clear();
            SDL_PauseAudioDevice(adev, 0);
        }

        public static void Reset() {
            if ( !ready ) return;
            SDL_CloseAudioDevice(adev);
            callbacks = new();
            ready = false;
        }

        public static void Setup() {
            Runtime.LuaState.RequireF("audio", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(AudioLib);
            return 1;
        }

        private static AudioCallback BeepCallback(double freq, long duration, float vol = 1.0f) {
            vol /= 4.0f;
            long sampleDuration = duration * CHANNELS * FREQUENCY / 1000;
            long elapsed = 0;
            var period = CHANNELS * FREQUENCY / freq;
            unsafe {
                return (buffer, count) => {
                    if ( elapsed >= sampleDuration ) return false;
                    float* p = buffer;
                    if ( sampleDuration - elapsed < count ) count = (int)(sampleDuration - elapsed);
                    for ( int i = 0; i < count; i += 2 ) {
                        buffer[i] = buffer[i + 1] = elapsed % period > period / 2 ? vol : -vol;
                        elapsed += 2;
                    }
                    return true;
                };
            }
        }

        private static int Beep(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            InitAudio();

            state.CheckNumber(1);

            double freq = state.ToNumber(1);
            long duration = state.OptInteger(2, 1000);
            float vol = (float)state.OptNumber(3, 1.0f);

            callbacks.Add(BeepCallback(freq, duration, vol));

            return 0;
        }
    }
}