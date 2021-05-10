#region 'Using' information.
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Nightfall
{

    enum FaceDirection // Used to determine which way the enemy is facing.
    {
        Left = -1,
        Right = 1,
    }

    class Enemy
    {
        public Level Level // Used in level code to load enemy data.
        {
            get { return level; }
        }

        Level level;

        public Vector2 Position // Controls spawn location.
        {
            get { return position; }
        }

        Vector2 position;

        private Rectangle localBounds; // Used to give the enemy a hitbox that keeps it grounded in the level.
        public Rectangle BoundingRectangle // Keeps the enemy in the level by giving them a bounding hitbox.
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }
        public bool IsAlive { get; private set; }

        #region Enemy Animations & Sounds
        private Animation runAnimation;
        private Animation idleAnimation;
        private Animation dieAnimation;
        private AnimationPlayer sprite;
        private SoundEffect killedSound;
        #endregion

        private FaceDirection direction = FaceDirection.Left; // Determines which way the enemy is facing - default is set to left.

        private float waitTime; // Will be used to control how long the enemy waits before turning around.

        private const float MaxWaitTime = 1.6f; // Makes the enemy wait before immediately turning around.

        private const float MoveSpeed = 128.0f; // Enemy horizontal movement speed.

        public Enemy(Level level, Vector2 position, string spriteSet) // Sets up a new enemy constructor with level and position as things it needs to define.
        {
            this.level = level;
            this.position = position;
            IsAlive = true;

            LoadContent(spriteSet);
        }

        public void LoadContent(string spriteSet) // Loads an enemy's animation sprites based on what it's doing.
        {
            // Loads animations for each enemy and finds them in specifically named folders.
            spriteSet = "Sprites/EnemySprites" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "EnemyRun"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "EnemyIdle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "EnemyDead"), 0.15f, true);
            sprite.PlayAnimation(idleAnimation);

            killedSound = Level.Content.Load<SoundEffect>("Sounds/Sound - EnemyDie");

            // Calculate hitbox sizes with the current frame's texture size as a base.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        public void Update(GameTime gameTime) // Makes the enemy walk back and forth along its platform.
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsAlive) // Only updates enemies if their status of being alive is set to true.
                return;

            // Calculates the tile position so the enemy doesn't wander off its tile.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds); // Makes the enemy wait before turning.
                if (waitTime <= 0.0f)
                {
                    direction = (FaceDirection)(-(int)direction); // Makes the enemy turn around.
                }
            }
            else
            {
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable || // Makes the enemy wait when it's near a wall.
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable) // Makes the enemy wait when it's near a drop.
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f); // Makes the enemy move forward in the direction it's facing.
                    position = position + velocity;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Level.Player.IsAlive || // Only plays enemy animations if the level is active (player is alive etc) and enemy is moving.
                Level.ReachedExit ||
                Level.TimeRemaining == TimeSpan.Zero ||
                waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }

            if (IsAlive == false)
            {
                sprite.PlayAnimation(dieAnimation);
            }

            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None; // Flips the animation horizontally based on where the enemy is facing.
            
            sprite.Draw(gameTime, spriteBatch, Position, flip, Color.White);
        }
        public void OnEnemyKilled(Player killedBy)
        {
            IsAlive = false;
            killedSound.Play();
        }
    }
}
