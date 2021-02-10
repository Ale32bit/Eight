using System;
using KeraLua;
using static SDL2.SDL;


// Very unfinished
namespace Eight.Module {
    class GraphicsCanvas {
        public unsafe struct canvas_t {
            public SDL_Surface* s;
            public ushort color;
        }

        public static unsafe void Init(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.NewMetaTable("canvas_t");
            state.PushCFunction(__gc);
            state.SetField(-2, "__gc");
            state.Pop(1);
        }

        public static unsafe int __gc(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            canvas_t* ud = (canvas_t*)state.ToUserData(1);
            SDL_FreeSurface((IntPtr)ud->s);
            ud->s = (SDL_Surface*)0;
            return 0;
        }

        internal static unsafe canvas_t* Can(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            canvas_t* c = (canvas_t*)state.ToUserData(Lua.UpValueIndex(1));
            if ( c == null ) {
                Console.WriteLine("ID-10T: CANVAS IS NULL\n");
                Eight.Quit();
            }

            return c;
        }

        public static unsafe int Canvas(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.Error("Work in progress");

            int w = (int)state.CheckInteger(1);
            int h = (int)state.CheckInteger(2);

            var s = SDL_CreateRGBSurface(0, w, h, 32,
                0xff000000,
                0x00ff0000,
                0x0000ff00,
                0x000000ff);

            if ( s == IntPtr.Zero ) {
                state.Error("%s", SDL_GetError());
                return 0;
            }

            var data = state.OptString(3, null);

            if(data != null) {

            }

            canvas_t *ud = (canvas_t*)state.NewUserData(sizeof(canvas_t));

            state.SetMetaTable("canvas_t");
            ud->s = (SDL_Surface*)s;
            ud->color = 0;

            state.PushCopy(-1);

            state.Rotate(-2, 1);
            state.SetField(-2, "_surface");

            return 1;
        }

    }
}
