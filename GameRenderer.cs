using Silk.NET.SDL;

namespace TheAdventure{
    public unsafe class GameRenderer
    {
        private Sdl _sdl;
        private IntPtr _renderer;
        private GameWindow _window;
        private GameLogic _gameLogic;

        public GameRenderer(Sdl sdl, GameWindow gameWindow, GameLogic gameLogic){
            _window = gameWindow;
            _gameLogic = gameLogic;
            _sdl = sdl;
            _renderer = gameWindow.CreateRenderer();
        }

        public void Render(){
            _sdl.RenderPresent((Renderer*)_renderer);
        }
    }
}