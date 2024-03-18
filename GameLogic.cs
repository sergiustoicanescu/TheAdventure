
namespace TheAdventure
{
    public class GameLogic
    {
        private Dictionary<int, GameObject> _gameObjects;

        public GameLogic(){
            _gameObjects = new Dictionary<int, GameObject>();

        }

        public void LoadGameState(){

            //var testObject = new RenderableGameObject("image.png", 1);
            //_gameObjects.Add(testObject.Id, testObject);
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