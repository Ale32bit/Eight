using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL.SDL_EventType;

namespace Eight;

public static class Program
{
    public static Runtime Runtime;
    public static Screen Screen;

    public static ConcurrentQueue<object?[]> EventQueue = new();

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
        var name = SDL_GetKeyName(sym).ToLower().Replace(" ", "_");
        name = name
            .Replace("windows", "meta")
            .Replace("/", "divide")
            .Replace("*", "multiply")
            .Replace("-", "minus")
            .Replace("+", "plus")
            .Replace(",", "comma")
            .Replace(".", "period");
        return name;
    }

    public static string[] GetKeyMods(SDL_Keymod keymod)
    {
        if (keymod == SDL_Keymod.KMOD_NONE)
            return Array.Empty<string>();

        var mods = new List<string>();

        if (keymod.HasFlag(SDL_Keymod.KMOD_LSHIFT))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_LSHIFT));
            mods.Add("shift");
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_RSHIFT))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_RSHIFT));
            mods.Add("shift");
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_LCTRL))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_LCTRL));
            mods.Add("ctrl");
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_RCTRL))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_RCTRL));
            mods.Add("ctrl");
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_LALT))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_LALT));
            mods.Add("alt");
        }
        if (keymod.HasFlag(SDL_Keymod.KMOD_RALT))
        {
            mods.Add(GetKeyName(SDL_Keycode.SDLK_RALT));
            mods.Add("alt");
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
        return mods.Distinct().ToArray();
    }

    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

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
        var pressedMouseButtons = new List<byte>();

        // Start the Lua script with args as arguments
        Runtime.PushParameters(args);
        Runtime.Resume();

        double pfreq = SDL_GetPerformanceFrequency();
        double ptime = 0;
        ulong last = SDL_GetPerformanceCounter();
        ulong now = last;
        while (Screen.Available)
        {

            while (SDL_PollEvent(out var ev) == 1)
            {
                switch (ev.type)
                {
                    case SDL_QUIT:
                        // user closed the window
                        Screen.Dispose();
                        Runtime.Dispose();
                        break;

                    case SDL_KEYUP:
                    case SDL_KEYDOWN:
                        // key_down | key_up, keyName, keyNumber, keyMod[], isRepeated
                        var keyMods = GetKeyMods(ev.key.keysym.mod);
                        EnqueueEvent(
                            ev.key.state == SDL_PRESSED ? "key_down" : "key_up",
                            GetKeyName(ev.key.keysym.sym),
                            (long)ev.key.keysym.sym,
                            keyMods,
                            ev.key.repeat == 1
                        );

                        // Only if CTRL is pressed
                        // KMOD_CTRL,ALT,SHIFT aren't detected for some reason
                        if (keyMods.Contains("ctrl")
                            && !keyMods.Contains("alt")
                            && !keyMods.Contains("shift")
                        )
                        {
                            // on CTRL+V: paste, text
                            if (ev.key.keysym.sym == SDL_Keycode.SDLK_v)
                            {
                                if (SDL_HasClipboardText() == SDL_bool.SDL_TRUE)
                                {
                                    var text = SDL_GetClipboardText();
                                    if (text != null)
                                    {
                                        text = text.Replace("\r\n", "\n");
                                        EnqueueEvent(
                                            "paste",
                                            text
                                        );
                                    }
                                }
                            }
                            // on CTRL+T: interrupt
                            else if (ev.key.keysym.sym == SDL_Keycode.SDLK_t)
                            {
                                EnqueueEvent("interrupt");
                            }
                        }

                        break;

                    case SDL_MOUSEMOTION:
                        // mouse_move, posX, posY
                        mouseX = (int)(ev.motion.x / Screen.Scale);
                        mouseY = (int)(ev.motion.y / Screen.Scale);

                        if (oldMouseX != mouseX || oldMouseY != mouseY)
                        {
                            EnqueueEvent(
                                "mouse_move",
                                mouseX,
                                mouseY
                            );

                            if (pressedMouseButtons.Count > 0)
                            {
                                EnqueueEvent(
                                    "mouse_drag",
                                    mouseX,
                                    mouseY,
                                    pressedMouseButtons.ToArray()
                                );
                            }

                            oldMouseX = mouseX;
                            oldMouseY = mouseY;
                        }

                        break;

                    case SDL_MOUSEBUTTONUP:
                    case SDL_MOUSEBUTTONDOWN:
                        // mouse_down | mouse_up, posX, posY, buttonId, clicksAmount
                        mouseX = (int)(ev.motion.x / Screen.Scale);
                        mouseY = (int)(ev.motion.y / Screen.Scale);

                        if (ev.button.state == SDL_PRESSED)
                        {
                            pressedMouseButtons.Add(ev.button.button);
                        }
                        else
                        {
                            pressedMouseButtons.Remove(ev.button.button);
                        }

                        EnqueueEvent(
                            ev.button.state == SDL_PRESSED ? "mouse_down" : "mouse_up",
                            mouseX,
                            mouseY,
                            ev.button.button,
                            ev.button.clicks
                        );


                        break;

                    case SDL_MOUSEWHEEL:
                        // mouse_wheel, posX, posY, wheelX, wheelY
                        var wx = ev.wheel.x;
                        var wy = ev.wheel.y;

                        if (ev.wheel.direction == (uint)SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED)
                        {
                            wx *= -1;
                            wy *= -1;
                        }

                        EnqueueEvent(
                            "mouse_wheel",
                            mouseX,
                            mouseY,
                            wx,
                            wy
                        );

                        break;

                    case SDL_TEXTINPUT:
                        // text, string
                        unsafe
                        {
                            EnqueueEvent(
                                "text",
                                Marshal.PtrToStringUTF8((IntPtr)ev.text.text) ?? ""
                            );
                        }
                        break;

                    case SDL_WINDOWEVENT:
                        if (ev.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                        {
                            // window_resize, newW, newH
                            SDL_GetWindowSize(Screen.Window, out var w, out var h);
                            var cw = w / Screen.Scale;
                            cw /= Screen.CharWidth;

                            var ch = h / Screen.Scale;
                            ch /= Screen.CharHeight;

                            if (cw < 1)
                                cw = 1;
                            if (ch < 1)
                                ch = 1;

                            Screen.SetSize((int)cw, (int)ch);

                            EnqueueEvent(
                                "window_resize",
                                Screen.RealWidth,
                                Screen.RealHeight
                            );
                        }
                        break;
                }

            }

            if ((ptime * 1000) >= 60)
            {
                ptime = 0;

                Screen.Present();
            }

            SDL_Delay(1);
            now = SDL_GetPerformanceCounter();
            ptime += (now - last) / pfreq;
            last = now;

        }
    }
}