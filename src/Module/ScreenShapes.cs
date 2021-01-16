using static SDL2.SDL;

namespace Eight.Module {
    public class ScreenShapes {
        public static unsafe void DrawRectangle(int x, int y, int w, int h, byte r, byte g, byte b) {
            if ( Eight.IsQuitting ) return;

            var rect = new SDL_Rect {
                x = x,
                y = y,
                w = w,
                h = h
            };

            SDL_FillRect(Display.Surface, ref rect, SDL_MapRGB(((SDL_Surface*)Display.Surface)->format, r, g, b));

            Display.Dirty = true;
        }

        public static unsafe void DrawPixel(int x, int y, byte r, byte g, byte b) {
            if ( Eight.IsQuitting ) return;

            if ( x < 0 && y < 0 && x >= Eight.RealWidth && y >= Eight.RealHeight ) {
                return;
            }

            var pixel = new SDL_Rect {
                x = x,
                y = y,
                w = 1,
                h = 1
            };

            SDL_FillRect(Display.Surface, ref pixel, SDL_MapRGB(((SDL_Surface*)Display.Surface)->format, r, g, b));

            Display.Dirty = true;
        }
    }
}