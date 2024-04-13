using Silk.NET.Maths;


/// <summary>
/// Provides translation from world coordinates to screen coordinates.
/// </summary>
/// <remarks>
/// World coordinates are top = 0, left = 0, positivie in right and down direction.
/// </remarks>
public class GameCamera
{
    private int _x;
    private int _y;

    private Rectangle<int> _gameWorld;

    /// <summary>
    /// World coordinates.
    /// </summary>
    public int X { 
        get {
            return _x;
        } 
        set{
            //var world = FromScreenToWorld(value, Y);
            if(_gameWorld.Contains(new Vector2D<int>(value, Y)))
            {
                _x = value;
            }
            else{
                //Console.WriteLine($"Set Y: {value},{Y} {_gameWorld.Size.X}, {_gameWorld.Size.Y}");
            }
            /*if(Width / 2 - value < 0){
                _x = value;
            }*/
        } 
    }

    /// <summary>
    /// World coordinates.
    /// </summary>
    public int Y { 
        get {
            return _y;
        } 
        set { 
            //var world = FromScreenToWorld(X, value);
            if(_gameWorld.Contains(new Vector2D<int>(X, value)))
            {
                _y = value;
            }
            else{
                //Console.WriteLine($"Set Y: {X},{value} {_gameWorld.Size.X}, {_gameWorld.Size.Y}");
            }
            /*if (Height / 2 - value < 0){
                _y = value;
            }*/
        } 
    }

    public int Width { get; set; }
    public int Height { get; set; }

    public GameCamera(int width, int height, Rectangle<int> gameWorld){
        Width = width;
        Height = height;
        _gameWorld = gameWorld;

        var marginLeft = Width / 2;
        var marginTop = Height / 2;

        if (marginLeft * 2 > gameWorld.Size.X){
            marginLeft = 48;//gameWorld.Size.X / 2;
        }

        if (marginTop * 2 > gameWorld.Size.Y){
            marginTop =  48;//0;//48;
        }

        _gameWorld = new Rectangle<int>(marginLeft, marginTop, gameWorld.Size.X - marginLeft * 2, gameWorld.Size.Y - marginTop * 2);
        _x = marginLeft;
        _y = marginTop;
    }

    public Rectangle<int> TranslateToScreenCoordinates(Rectangle<int> textureDestination)
    {
        // In screen coordinates x = width / 2, y = height / 2.        
        var newDestination = textureDestination.GetTranslated(new Vector2D<int>(Width / 2 - X, Height / 2 - Y));
        return newDestination;
    }

    public Vector2D<int> FromScreenToWorld(int x, int y){
        return new Vector2D<int>(x - (Width / 2 - X), y - (Height / 2 - Y));
    }
}