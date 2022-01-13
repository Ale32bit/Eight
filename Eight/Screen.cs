using static SDL2.SDL;

namespace Eight;

public class Screen : IDisposable {
    public IntPtr Window;
    public IntPtr Renderer;
    public IntPtr Surface;

    public bool Available = true;

    public Screen() {
        if (SDL_Init(SDL_INIT_EVERYTHING) != 0) {
            throw new ScreenException(SDL_GetError());
        }

        Window = SDL_CreateWindow("Eight", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 800, 600,
            SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI);

        if (Window == IntPtr.Zero) {
            throw new ScreenException(SDL_GetError());
        }

        Renderer = SDL_CreateRenderer(Window, 0,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (Renderer == IntPtr.Zero) {
            throw new ScreenException(SDL_GetError());
        }
    }

    public SDL_Event WaitEvent() {
        if (SDL_WaitEvent(out var ev) == 1)
            return ev;

        throw new ScreenException(SDL_GetError());
    }

    public void Dispose() {
        Available = false;
        GC.SuppressFinalize(this);
        SDL_Quit();
    }
}