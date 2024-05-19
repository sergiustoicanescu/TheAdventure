using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
using TheAdventure.Models.Data;

namespace TheAdventure
{
    public class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<string, TileSet> _loadedTileSets = new();

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;
        private ScriptEngine _scriptEngine;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        private int _score;
        private Random _random;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;
            _scriptEngine = new ScriptEngine();
            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);

            _score = 0;
            _random = new Random();
        }

        private void IncreaseScore()
        {
            _score++;
        }

        public void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        public (int x, int y) GetPlayerPosition()
        {
            var pos = _player.Position;
            return (pos.X, pos.Y);
        }

        public void InitializeWorld()
        {
            var executableLocation = new FileInfo(Assembly.GetExecutingAssembly().Location);
            _scriptEngine.LoadAll(Path.Combine(executableLocation.Directory.FullName, "Assets", "Scripts"));

            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var levelContent = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));

            var level = JsonSerializer.Deserialize<Level>(levelContent, jsonSerializerOptions);
            if (level == null) return;
            foreach (var refTileSet in level.TileSets)
            {
                var tileSetContent = File.ReadAllText(Path.Combine("Assets", refTileSet.Source));
                if (!_loadedTileSets.TryGetValue(refTileSet.Source, out var tileSet))
                {
                    tileSet = JsonSerializer.Deserialize<TileSet>(tileSetContent, jsonSerializerOptions);

                    foreach (var tile in tileSet.Tiles)
                    {
                        var internalTextureId = _renderer.LoadTexture(Path.Combine("Assets", tile.Image), out _);
                        tile.InternalTextureId = internalTextureId;
                    }

                    _loadedTileSets[refTileSet.Source] = tileSet;
                }

                refTileSet.Set = tileSet;
            }

            _currentLevel = level;
            /*SpriteSheet spriteSheet = new(_renderer, Path.Combine("Assets", "player.png"), 10, 6, 48, 48, new FrameOffset() { OffsetX = 24, OffsetY = 42 });
            spriteSheet.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = new FramePosition(),//(0, 0),
                EndFrame = new FramePosition() { Row = 0, Col = 5 },
                DurationMs = 1000,
                Loop = true
            };
            */
            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                _player = new PlayerObject(spriteSheet, 100, 100);
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();
            bool isAttacking = _input.IsKeyAPressed();
            bool addBomb = _input.IsKeyBPressed();

            _scriptEngine.ExecuteAll(this);

            if (isAttacking)
            {
                var dir = up ? 1 : 0;
                dir += down ? 1 : 0;
                dir += left ? 1 : 0;
                dir += right ? 1 : 0;
                if (dir <= 1)
                {
                    _player.Attack(up, down, left, right);
                }
                else
                {
                    isAttacking = false;
                }
            }
            if (!isAttacking)
            {
                _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);
            }
            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            if (addBomb)
            {
                AddBomb(_player.Position.X, _player.Position.Y, false);
            }

            bool bombDodged = false;

            foreach (var gameObjectId in itemsToRemove)
            {
                var gameObject = _gameObjects[gameObjectId];
                if (gameObject is TemporaryGameObject)
                {
                    var tempObject = (TemporaryGameObject)gameObject;
                    var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                    if (deltaX < 32 && deltaY < 32 && !_player.IsInvincible)
                    {
                        GameOver();
                    }
                    else
                    {
                        bombDodged = true;
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }

            if (bombDodged)
            {
                IncreaseScore();
                RenderScore();
                SpawnPowerUp();
            }

            foreach (var powerUp in GetAllPowerUps())
            {
                var deltaX = Math.Abs(_player.Position.X - powerUp.Position.X);
                var deltaY = Math.Abs(_player.Position.Y - powerUp.Position.Y);
                if (deltaX < 32 && deltaY < 32)
                {
                    CollectPowerUp(powerUp);
                    _gameObjects.Remove(powerUp.Id);
                }
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            _renderer.PresentFrame();
        }

        private void RenderScore()
        {
            var scoreText = $"Score: {_score}";
            Console.WriteLine(scoreText);
        }

        private async void GameOver()
        {
            _player.GameOver();
            await Task.Delay(1000);
            Environment.Exit(0);
        }

        private Tile? GetTile(int id)
        {
            if (_currentLevel == null) return null;
            foreach (var tileSet in _currentLevel.TileSets)
            {
                foreach (var tile in tileSet.Set.Tiles)
                {
                    if (tile.Id == id)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }

        private void RenderTerrain()
        {
            if (_currentLevel == null) return;
            for (var layer = 0; layer < _currentLevel.Layers.Length; ++layer)
            {
                var cLayer = _currentLevel.Layers[layer];

                for (var i = 0; i < _currentLevel.Width; ++i)
                {
                    for (var j = 0; j < _currentLevel.Height; ++j)
                    {
                        var cTileId = cLayer.Data[j * cLayer.Width + i] - 1;
                        var cTile = GetTile(cTileId);
                        if (cTile == null) continue;

                        var src = new Rectangle<int>(0, 0, cTile.ImageWidth, cTile.ImageHeight);
                        var dst = new Rectangle<int>(i * cTile.ImageWidth, j * cTile.ImageHeight, cTile.ImageWidth,
                            cTile.ImageHeight);

                        _renderer.RenderTexture(cTile.InternalTextureId, src, dst);
                    }
                }
            }
        }

        private IEnumerable<RenderableGameObject> GetAllRenderableObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is RenderableGameObject renderableGameObject)
                {
                    yield return renderableGameObject;
                }
            }
        }

        private IEnumerable<TemporaryGameObject> GetAllTemporaryGameObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is TemporaryGameObject temporaryGameObject)
                {
                    yield return temporaryGameObject;
                }
            }
        }

        private IEnumerable<PowerUp> GetAllPowerUps()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is PowerUp powerUp)
                {
                    yield return powerUp;
                }
            }
        }

        private void RenderAllObjects()
        {
            foreach (var gameObject in GetAllRenderableObjects())
            {
                gameObject.Render(_renderer);
            }

            _player.Render(_renderer);
        }

        public void AddBomb(int x, int y, bool translateCoordinates = true)
        {

            var translated = translateCoordinates ? _renderer.TranslateFromScreenToWorldCoordinates(x, y) : new Vector2D<int>(x, y);

            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }

        private void SpawnPowerUp()
        {
            if (_random.NextDouble() < 0.5)
            {
                var powerUpType = (PowerUpType)_random.Next(0, 2);
                var x = _random.Next(0, _currentLevel.Width * _currentLevel.TileWidth);
                var y = _random.Next(0, _currentLevel.Height * _currentLevel.TileHeight);
                var spriteSheet = SpriteSheet.LoadSpriteSheet("powerup.json", "Assets", _renderer);

                if (spriteSheet != null)
                {
                    PowerUp powerUp = new(spriteSheet, 10.0, (x, y), powerUpType);
                    spriteSheet.ActivateAnimation(powerUpType.ToString());
                    _gameObjects.Add(powerUp.Id, powerUp);
                }
            }
        }

        private void CollectPowerUp(PowerUp powerUp)
        {
            switch (powerUp.Type)
            {
                case PowerUpType.SpeedBoost:
                    _player.Speed += 1.0;
                    Task.Delay(5000).ContinueWith(t => _player.Speed -= 1.0);
                    break;
                case PowerUpType.Invincibility:
                    _player.IsInvincible = true;
                    Task.Delay(5000).ContinueWith(t => _player.IsInvincible = false);
                    break;
            }
        }
    }
}
