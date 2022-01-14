using static SDL2.SDL.SDL_EventType;
using static SDL2.SDL;

namespace Eight;

public static class Program
{
    public static Runtime Runtime;
    public static Screen Screen;

    public static Queue<object?[]> EventQueue = new();

    private static bool runningQueue = false;

    public static void EnqueueEvent(params object[] pars)
    {
        EventQueue.Enqueue(pars);
        Resume();
    }

    public static void Resume()
    {
        if (runningQueue) return;
        runningQueue = true;

        while (EventQueue.TryDequeue(out var pars))
        {
            Runtime.PushParameters(pars);
            Runtime.Resume();
        }

        runningQueue = false;
    }

    public static void Main(string[] args)
    {
        Screen = new Screen();

        Runtime = new Runtime();

        Runtime.LoadEightLibraries();

        Runtime.LoadInit();

        int oldMouseX = -1;
        int oldMouseY = -1;
        int mouseX, mouseY;
        while (Screen.Available)
        {
            var ev = Screen.WaitEvent();
            switch (ev.type)
            {
                case SDL_QUIT:
                    Screen.Dispose();
                    Runtime.Dispose();
                    break;

                case SDL_KEYUP:
                case SDL_KEYDOWN:
                    EnqueueEvent(
                        ev.key.state == SDL_PRESSED ? "key_down" : "key_up",
                        SDL_GetKeyName(ev.key.keysym.sym).ToLower().Replace(" ", "_"),
                        (long)ev.key.keysym.sym,
                        ev.key.repeat == 1
                    );
                    break;

                case SDL_MOUSEMOTION:
                    mouseX = (int)(ev.motion.x / Screen.Scale);
                    mouseY = (int)(ev.motion.y / Screen.Scale);

                    if (oldMouseX != mouseX || oldMouseY != mouseY)
                    {
                        EnqueueEvent(
                            "mouse_move",
                            mouseX,
                            mouseY
                        );

                        oldMouseX = mouseX;
                        oldMouseY = mouseY;
                    }

                    break;

                case SDL_MOUSEBUTTONUP:
                case SDL_MOUSEBUTTONDOWN:
                    mouseX = (int)(ev.motion.x / Screen.Scale);
                    mouseY = (int)(ev.motion.y / Screen.Scale);

                    EnqueueEvent(
                        ev.button.state == SDL_PRESSED ? "mouse_down" : "mouse_up",
                        ev.button.button,
                        mouseX,
                        mouseY,
                        ev.button.clicks != 1
                    );

                    break;
            }
        }
    }
}