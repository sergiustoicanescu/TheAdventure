using System;

namespace TheAdventure.Models
{
    public enum PowerUpType
    {
        SpeedBoost,
        Invincibility,
    }

    public class PowerUp : TemporaryGameObject
    {
        public PowerUpType Type { get; private set; }

        public PowerUp(SpriteSheet spriteSheet, double duration, (int X, int Y) position, PowerUpType type)
            : base(spriteSheet, duration, position)
        {
            Type = type;
        }
    }
}
