using static SDL2.SDL;

namespace Eight;

public class Screen : IDisposable
{
    public int Width = 51;
    public int Height = 19;
    public float Scale = 10;

    public int CharWidth = 2;
    public int CharHeight = 3;

    public int RealWidth => Width * CharWidth;
    public int RealHeight => Height * CharHeight;

    public int WindowWidth => (int)(RealWidth * Scale);
    public int WindowHeight => (int)(RealHeight * Scale);

    public IntPtr Window;
    public IntPtr Renderer;
    public IntPtr Surface;

    public bool Available = true;

    private uint[] _screenBuffer;
    private uint[] _termBuffer;

    public Screen()
    {
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

        ApplySize();
    }

    private void ApplySize()
    {
        _screenBuffer = new uint[RealWidth * RealHeight];
        _termBuffer = new uint[Width * Height];
        Surface = SDL_CreateRGBSurface(0, RealWidth, RealHeight, 32, 0x00_ff_00_00, 0x00_00_ff_00, 0x00_00_00_ff, 0xff_00_00_00);
    }

    public void SetSize(int w, int h, float? s = null)
    {
        Width = w;
        Height = h;
        Scale = s ?? Scale;

        SDL_SetWindowSize(Window, WindowWidth, WindowHeight);

        ApplySize();
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