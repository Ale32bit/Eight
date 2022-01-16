using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace Eight;

public class Screen : IDisposable
{
    public int Width = 51;
    public int Height = 19;
    public float Scale = 4;

    public int CharWidth = 6;
    public int CharHeight = 9;

    public int RealWidth => Width * CharWidth;
    public int RealHeight => Height * CharHeight;

    public int WindowWidth => (int)(RealWidth * Scale);
    public int WindowHeight => (int)(RealHeight * Scale);

    public IntPtr Window;
    public IntPtr Renderer;
    public IntPtr Surface;

    public bool Available = true;

    private uint[] _screenBuffer;

    // 00, ch, bg, fg
    private uint[] _termBuffer;

    public Screen()
    {
        SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
        if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
        {
            throw new ScreenException(SDL_GetError());
        }

        Window = SDL_CreateWindow("Eight", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, WindowWidth, WindowHeight,
            SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI);

        if (Window == IntPtr.Zero)
        {
            throw new ScreenException(SDL_GetError());
        }

        Renderer = SDL_CreateRenderer(Window, 0,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (Renderer == IntPtr.Zero)
        {
            throw new ScreenException(SDL_GetError());
        }

        SDL_SetWindowResizable(Window, SDL_bool.SDL_TRUE);

        ApplySize();

        SDL_AddEventWatch(LiveResize, Window);
    }

    public unsafe void Present()
    {
        Console.WriteLine("presenting");

        var texture = SDL_CreateTextureFromSurface(Renderer, Surface);
        SDL_RenderCopy(Renderer, texture, IntPtr.Zero, IntPtr.Zero);
        SDL_DestroyTexture(texture);
        SDL_RenderPresent(Renderer);
    }

    private void ApplySize()
    {
        _screenBuffer = new uint[RealWidth * RealHeight];
        _termBuffer = new uint[Width * Height];
        SDL_SetWindowMinimumSize(Window, (int)(CharWidth * Scale), (int)(CharHeight * Scale));
        if(SDL_RenderSetScale(Renderer, Scale, Scale) != 0)
        {
            throw new ScreenException(SDL_GetError());
        }
        Surface = SDL_CreateRGBSurface(0, RealWidth, RealHeight, 32, 0x00_ff_00_00, 0x00_00_ff_00, 0x00_00_00_ff, 0xff_00_00_00);

        var r = new SDL_Rect() {
            x = 4,
            y = 6,
            w= 90,
            h = 24
        };
        SDL_FillRect(Surface, ref r, 0xff_ff_00_00);

        Present();
    }

    public void SetSize(int w, int h, float? s = null)
    {
        Width = w;
        Height = h;
        Scale = s ?? Scale;

        SDL_SetWindowSize(Window, WindowWidth, WindowHeight);

        ApplySize();
    }

    private unsafe int LiveResize(IntPtr data, IntPtr sdlev)
    {
        var ev = (SDL_Event*)sdlev;

        if (ev->type == SDL_EventType.SDL_WINDOWEVENT && ev->window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
        {
            IntPtr win = SDL_GetWindowFromID(ev->window.windowID);
            if (win == data)
            {
                SDL_GetWindowSize(Window, out var w, out var h);
                var cw = w / Scale;
                cw /= CharWidth;

                var ch = h / Scale;
                ch /= CharHeight;

                if (cw < 1)
                    cw = 1;
                if (ch < 1)
                    ch = 1;

                // Limit amount of SDL calls to avoid crashing the CLR
                SDL_SetWindowSize(Window, (int)((int)cw * CharWidth * Scale), (int)((int)ch * CharHeight * Scale));
            }
        }

        return 0;
    }

    public void SetFont() { }

    public SDL_Event WaitEvent()
    {
        if (SDL_WaitEvent(out var ev) == 1)
            return ev;

        throw new ScreenException(SDL_GetError());
    }

    public void Dispose()
    {
        Available = false;
        GC.SuppressFinalize(this);
        SDL_Quit();
    }
}