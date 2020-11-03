using System;
using System.Reflection;

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

        public void SetTickrate(int tickrate) {
            Eight.SetTickrate(tickrate);
        }
    }
}