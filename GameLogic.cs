
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace TheAdventure
{
    public class GameLogic
    {
        private Dictionary<int, GameObject> _gameObjects;

        public GameLogic(){
            _gameObjects = new Dictionary<int, GameObject>();

        }

        public void LoadGameState(){
        }

        public IEnumerable<RenderableGameObject> GetAllRenderableObjects(){
            foreach(var gameObject in _gameObjects.Values){
                if(gameObject is RenderableGameObject){
                    yield return (RenderableGameObject)gameObject;
                }
            }
        }

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

        public void RenderAllObjects(int timeSinceLastFrame, GameRenderer renderer){
            List<int> itemsToRemove = new List<int>();
            foreach(var gameObject in GetAllRenderableObjects()){
                if(gameObject.Update(timeSinceLastFrame)){
                    gameObject.Render(renderer);
                }
                else{
                    itemsToRemove.Add(gameObject.Id);
                }

                
            }
            foreach(var item in itemsToRemove){
                _gameObjects.Remove(item);
            }
        }

        private int _bombIds = 100;

        public void AddBomb(int x, int y){
            AnimatedGameObject bomb = new AnimatedGameObject("BombExploding.png", 2, _bombIds, 13, 13, 1, x, y);
            _gameObjects.Add(bomb.Id, bomb);
            ++_bombIds;
        }

    }
}