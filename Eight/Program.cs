using static SDL2.SDL.SDL_EventType;

namespace Eight;

public static class Program {
    public static Runtime Runtime;
    public static Screen Screen;

    public static void Main(string[] args) {
        Screen = new Screen();

        Runtime = new Runtime();

        Runtime.LoadEightLibraries();

        Runtime.LoadInit();

        while (Screen.Available) {
            var ev = Screen.WaitEvent();
            switch (ev.type) {
                case SDL_QUIT:
                    Screen.Dispose();
                    break;
            }
        }
    }
}