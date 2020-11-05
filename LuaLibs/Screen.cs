using static SDL2.SDL;

namespace Eight.LuaLibs {
    public class Screen {
        public Screen() {
            
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b) {
            SDLLogic.DrawPixel(x, y, r, g, b);
        }

        public void SetSize(int w, int h, int s) {
            SDLLogic.ResizeCanvas(w, h, s);
        }

        public int[] GetSize() {
            int[] sizes = {
                Eight.WindowWidth,
                Eight.WindowHeight,
                Eight.WindowScale,
            };
            return sizes;
        }

        public void SetTickrate(int tickrate) {
            Eight.SetTickrate(tickrate);
        }

        public void Clear() {
            SDLLogic.Clear();
        }

        public void DrawRectangle(int x, int y, int w, int h, byte r, byte g, byte b) {
            SDLLogic.DrawRectangle(x, y, w, h, r, g, b);
        }

        public void SetTitle(string title) {
            SDL_SetWindowTitle(Eight.Window, title);
        }
        public string GetTitle() {
            return SDL_GetWindowTitle(Eight.Window);
        }
    }
}