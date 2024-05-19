using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models
{
    public class PlayerObject : RenderableGameObject
    {
        public enum PlayerStateDirection
        {
            None = 0,
            Down,
            Up,
            Left,
            Right,
        }

        public enum PlayerState
        {
            None = 0,
            Idle,
            Move,
            Attack,
            GameOver
        }

        private int _basePixelsPerSecond = 192;
        private int _pixelsPerSecond;
        public bool IsInvincible { get; set; }
        public double Speed { get; set; }

        public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

        public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            _pixelsPerSecond = _basePixelsPerSecond;
            Speed = 1.0;
            IsInvincible = false;
            SetState(PlayerState.Idle, PlayerStateDirection.Down);
        }

        public void SetState(PlayerState state, PlayerStateDirection direction)
        {
            if (State.State == PlayerState.GameOver) return;
            if (State.State == state && State.Direction == direction)
            {
                return;
            }
            else if (state == PlayerState.None && direction == PlayerStateDirection.None)
            {
                SpriteSheet.ActivateAnimation(null);
            }
            else if (state == PlayerState.GameOver)
            {
                SpriteSheet.ActivateAnimation(Enum.GetName(state));
            }
            else
            {
                var animationName = Enum.GetName<PlayerState>(state) + Enum.GetName<PlayerStateDirection>(direction);
                SpriteSheet.ActivateAnimation(animationName);
            }
            State = (state, direction);
        }

        public void GameOver()
        {
            SetState(PlayerState.GameOver, PlayerStateDirection.None);
        }

        public void Attack(bool up, bool down, bool left, bool right)
        {
            if (State.State == PlayerState.GameOver) return;
            var direction = State.Direction;
            if (up)
            {
                direction = PlayerStateDirection.Up;
            }
            else if (down)
            {
                direction = PlayerStateDirection.Down;
            }
            else if (right)
            {
                direction = PlayerStateDirection.Right;
            }
            else if (left)
            {
                direction = PlayerStateDirection.Left;
            }
            SetState(PlayerState.Attack, direction);
        }

        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
            double time)
        {
            if (State.State == PlayerState.GameOver) return;
            if (up <= double.Epsilon &&
                down <= double.Epsilon &&
                left <= double.Epsilon &&
                right <= double.Epsilon &&
                State.State == PlayerState.Idle)
            {
                return;
            }

            var pixelsToMove = time * _pixelsPerSecond * Speed;

            var x = Position.X + (int)(right * pixelsToMove);
            x -= (int)(left * pixelsToMove);

            var y = Position.Y - (int)(up * pixelsToMove);
            y += (int)(down * pixelsToMove);

            if (x < 10)
            {
                x = 10;
            }

            if (y < 24)
            {
                y = 24;
            }

            if (x > width - 10)
            {
                x = width - 10;
            }

            if (y > height - 6)
            {
                y = height - 6;
            }

            if (y < Position.Y)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Up);
            }
            if (y > Position.Y)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Down);
            }
            if (x > Position.X)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Right);
            }
            if (x < Position.X)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Left);
            }
            if (x == Position.X &&
                y == Position.Y)
            {
                SetState(PlayerState.Idle, State.Direction);
            }

            Position = (x, y);
        }
    }
}
