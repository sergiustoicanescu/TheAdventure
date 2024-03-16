
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace TheAdventure
{
    public class GameLogic
    {
        public GameLogic(){
            _gameObjects = new List<GameObject>();
        }

        public void InitializeGame(GameRenderer gameRenderer){
            _gameRenderer = gameRenderer;

            var textureId = gameRenderer.LoadTexture("image.png", out var textureInfo);            
            var sampleRenderableObject = new RenderableGameObject(textureId, new Rectangle<int>(0, 0, textureInfo.Width, textureInfo.Height), new Rectangle<int>(0, 0, textureInfo.Width, textureInfo.Height), textureInfo);
            _gameObjects.Add(sampleRenderableObject);
        }

        int frameCount = 0;

        public void ProcessFrame()
        {
            var renderableObject = (RenderableGameObject)_gameObjects.First();

            var i = frameCount % 10;
            var j = frameCount / 10;

            var cellWidth = renderableObject.TextureInformation.Width / 10;
            var cellHeight = renderableObject.TextureInformation.Height / 10;

            var x = i * cellWidth;
            var y = j * cellHeight;

            Rectangle<int> srcDest = new Rectangle<int>(x, y, cellWidth, cellHeight);

            renderableObject.TextureSource = srcDest;
            //renderableObject.TextureDestination = srcDest;
            renderableObject.TextureDestination = new Rectangle<int>(0, 0, cellWidth, cellHeight);

            ++frameCount;
            if(frameCount == 100){
                frameCount = 0;
            }
        }

        private GameRenderer _gameRenderer;
        private List<GameObject> _gameObjects;

        public IEnumerable<RenderableGameObject> GetRenderables()
        {
            foreach(var gameObject in _gameObjects){
                if(gameObject is RenderableGameObject){
                    yield return (RenderableGameObject)gameObject;
                }
            }
        }
    }
}