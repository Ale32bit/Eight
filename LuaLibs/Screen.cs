using System.Diagnostics.Tracing;

namespace Eight.LuaLibs {
    public class Screen {
        public Screen() {
            
        }

        public static int Test() {
            return 0;
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
    }
}