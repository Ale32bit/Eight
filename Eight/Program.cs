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

    public static string GetKeyName(SDL_Keycode sym)
    {
        return SDL_GetKeyName(sym).ToLower().Replace(" ", "_");
    }

    public static string[] GetKeyMods(SDL_Keymod keymod)
    {
        if(keymod == SDL_Keymod.KMOD_NONE)
            return new string[0];

        var mods = new List<string>();

        if(keymod.HasFlag(SDL_Keymod.KMOD_LSHIFT))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_LSHIFT));
        }
        if(keymod.HasFlag(SDL_Keymod.KMOD_RSHIFT))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_RSHIFT));
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_LCTRL))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_LCTRL));
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_RCTRL))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_RCTRL));
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_LALT))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_LALT));
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_LGUI))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_RGUI));
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_NUM))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_NUMLOCKCLEAR));
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_CAPS))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_CAPSLOCK));
        }

        return mods.ToArray();
    }

    public static void Main(string[] args)
    {
        Screen = new Screen();

        Runtime = new Runtime();

        Runtime.LoadEightLibraries();

        Runtime.LoadInit();

        int oldMouseX = -1;
        int oldMouseY = -1;
        // Initialize mouse position
        SDL_GetRelativeMouseState(out var mouseX, out var mouseY);
        mouseX = (int)(mouseX / Screen.Scale);
        mouseY = (int)(mouseY / Screen.Scale);

        while (Screen.Available)
        {
            var ev = Screen.WaitEvent();
            switch (ev.type)
            {
                case SDL_QUIT:
                    // user closed the window
                    Screen.Dispose();
                    Runtime.Dispose();
                    break;

                case SDL_KEYUP:
                case SDL_KEYDOWN:
                    // evname, keyname, keynumber, repeated
                    EnqueueEvent(
                        ev.key.state == SDL_PRESSED ? "key_down" : "key_up",
                        GetKeyName(ev.key.keysym.sym),
                        (long)ev.key.keysym.sym,
                        GetKeyMods(ev.key.keysym.mod),
                        ev.key.repeat == 1
                    );
                    break;

                case SDL_MOUSEMOTION:
                    // evname, x, y
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
                    // evname, x, y, button, clicks
                    mouseX = (int)(ev.motion.x / Screen.Scale);
                    mouseY = (int)(ev.motion.y / Screen.Scale);

                    EnqueueEvent(
                        ev.button.state == SDL_PRESSED ? "mouse_down" : "mouse_up",
                        mouseX,
                        mouseY,
                        ev.button.button,
                        ev.button.clicks
                    );

                    break;

                case SDL_MOUSEWHEEL:
                    // evname, x, y, wx, wy
                    var wx = ev.wheel.x;
                    var wy = ev.wheel.y;

                    if(ev.wheel.direction == (uint)SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED)
                    {
                        wx *= -1;
                        wy *= -1;
                    }

                    EnqueueEvent(
                        "mouse_scroll",
                        mouseX,
                        mouseY,
                        wx,
                        wy
                    );

                    break;
            }
        }
    }
}