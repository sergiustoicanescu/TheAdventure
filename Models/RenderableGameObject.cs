using Silk.NET.Maths;
using TheAdventure;

public class RenderableGameObject : GameObject
{
    public int TextureId { get; set; }
    public Rectangle<int> TextureSource { get; set; }
    public Rectangle<int> TextureDestination { get; set; }
    public GameRenderer.TextureInfo TextureInformation {get;set;}

    public RenderableGameObject(int textureId = -1, Rectangle<int> textureSource = new Rectangle<int>() , Rectangle<int> textureDestination = new Rectangle<int>(), GameRenderer.TextureInfo textureInfo = default(GameRenderer.TextureInfo))
    {
        TextureId = textureId;
        TextureSource = textureSource;
        TextureDestination = textureDestination;
        TextureInformation = textureInfo;
    }
}