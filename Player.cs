#region 'Using' information.
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Nightfall
{
    class Player
    {
        #region Loads sounds and animations.
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation dieAnimation;
        private Animation slideAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;
        #endregion

        public Level Level
        {
            get { return level; }
        }

        Level level;

        public bool IsAlive
        {
            get { return isAlive; } // Some code needs to check if the player is alive first to run.
        }

        bool isAlive;

        public Vector2 Position // Checks player position for gravity and movement.
        {
            get { return position; }
            set { position = value; }
        }

        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        Vector2 velocity;

        // Controls horizontal movement.
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Controls vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        // Controls input
        private const float AccelerometerScale = 1.5f;

        // Creates a bool to  define if the player is sliding or not.
        private bool isSliding;

        public bool IsSliding
        {
            get { return isSliding; } // Checks whether or not the player is sliding.
        }

        public bool IsOnGround
        {
            get { return isOnGround; } // Checks whether or not the player is on the floor.
        }

        bool isOnGround;

        private float movement;

        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        private Rectangle slideBounds;

        public Rectangle BoundingRectangle // Controls collision hitboxes.
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public Rectangle SlidingRectangle // Controls sliding hitboxes.
        {
            get
            {
                int slidingleft = (int)Math.Round(Position.X - sprite.Origin.X) + slideBounds.X;
                int slidingtop = (int)Math.Round(Position.Y - sprite.Origin.Y) + slideBounds.Y;

                return new Rectangle(slidingleft, slidingtop, slideBounds.Width, slideBounds.Height);
            }
        }

        public Player(Level level, Vector2 position) // Creates a player within the level.
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        public void LoadContent() // Loads the player's sprite sheet and sounds, attaches them to the proper code.
        {

            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/PlayerSprites/IdleSpritesheet"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/PlayerSprites/RunSpritesheet"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/PlayerSprites/JumpSpritesheet"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/PlayerSprites/DieSpritesheet"), 0.1f, false);
            slideAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/PlayerSprites/SlideSpritesheet"), 0.1f, false);

            // Changes the player hitbox size based on the size of the current animation frame.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Changes the player's hitbox size whenever they are sliding.
            int slidewidth = (int)(slideAnimation.FrameWidth * 0.4);
            int slideleft = (slideAnimation.FrameWidth - slidewidth) / 2;
            int slideheight = (int)(slideAnimation.FrameHeight * 0.8);
            int slidetop = slideAnimation.FrameHeight - slideheight;
            slideBounds = new Rectangle(slideleft, slidetop, slidewidth, slideheight);

            #region Loads sounds           
            killedSound = Level.Content.Load<SoundEffect>("Sounds/Sound - PlayerCaught");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/Sound - PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/Sound - PlayerFallDie");
            #endregion
        }

        public void Reset(Vector2 position) // Brings the player back from death.
        {
            Position = position; // The place where the player respawns.
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, AccelerometerState accelState) // Updates input, movement, physics and animations.
        {
            GetInput(keyboardState, accelState);

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround) // Plays the run / idle animations if the player is alive and on the ground depending on if they're moving or not.
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
                
            }

            if(IsAlive && IsOnGround && IsSliding) // Plays the sliding animation if the player is alive, on the ground, and holding S.
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0 )
                {
                    sprite.PlayAnimation(slideAnimation);                   
                }
            }

            movement = 0.0f; // Stops the player from moving when they aren't pressing anything.
            isJumping = false; // Stops the player from jumping when they aren't pressing anything.
            isSliding = false; // Stops the player from sliding when they aren't pressing anything.
        }

        private void GetInput(
            KeyboardState keyboardState,
            AccelerometerState accelState)
        {
            if (Math.Abs(movement) < 0.5f) // Ignores tiny taps of the keys so the player doesn't run in place.
                movement = 0.0f;

            if (Math.Abs(accelState.Acceleration.Y) > 0.10f) // Move the player with accelerometer
            {
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f); // sets player movement speed
            }

            #region Left and right movement.
            if (keyboardState.IsKeyDown(Keys.Left)) // Moves the player left when they press the left arrow key.
            {
                movement = -1.0f;
            }

            if (keyboardState.IsKeyDown(Keys.A)) // Moves the player lefft when they press the A key.
            {
                movement = -1.0f;
            }

            if (keyboardState.IsKeyDown(Keys.Right)) // Moves the player right when they press the right arrow key.

            {
                movement = 1.0f;
            }

            if (keyboardState.IsKeyDown(Keys.D)) // Moves the player right when they press the D key.

            {
                movement = 1.0f;
            }
            #endregion

            isJumping = // Lets the player jump by pressing the up arrow key, W or Spacebar.
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);

            isSliding = // Lets the player slide by pressing the S or Down arrow keys.
                keyboardState.IsKeyDown(Keys.S) ||
                keyboardState.IsKeyDown(Keys.Down);
        }

        #region Controls the player's movement physics - max speed, velocity, etc.
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            velocity.X += movement * MoveAcceleration * elapsed; // Controls player speed by calculating how long they have been moving and in which direction.
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            velocity.Y = DoJump(velocity.Y, gameTime);

            if (IsOnGround) // Makes horizontal movement seem more realistic by limiting player's maximum speed.
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed); // Stops the player from moving faster than their maximum speed.

            Position += velocity * elapsed; // Adds velocity to the player's movement.
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            HandleCollisions(); // Stops the player from getting stuck inside the level.

            if (Position.X == previousPosition.X) // Resets the player's horizontal velocity if they run into a wall.
                velocity.X = 0;

            if (Position.Y == previousPosition.Y) // Resets the player's vertical velocity if they land a jump.
                velocity.Y = 0;
        }
        #endregion

        #region Controls the player's jump physics - max speed, velocity, animations etc.
        private float DoJump(float velocityY, GameTime gameTime)
        {
            if (isJumping)
            {
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f) // Starts or continues a jump, while also playing the jump sound and jump animation.
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                if (0.0f < jumpTime && jumpTime <= MaxJumpTime) // Lets players move while in the air
                { velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower)); }

                else
                { jumpTime = 0.0f; } // Brings the jump back down to the ground.
            }
            else
            { jumpTime = 0.0f; } // Can cancel a jump in progress or stop the player from jumping when they haven't pressed anything.

            wasJumping = isJumping;

            return velocityY; // Controls the player's speed in the air.
        }
        #endregion

        #region Controls the player's collision with the tiles around them.
        private void HandleCollisions()
        {
            // Get the player's hitbox and finds tiles around them.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            Rectangle slideBounds = SlidingRectangle;

            isOnGround = false; // Checks for ground collision and sets the default value to false.

            for (int y = topTile; y <= bottomTile; ++y) // Runs code for checking if collidable tiles are nearby
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {

                    TileCollision collision = Level.GetCollision(x, y); // Checks to see if the tile is one you can collide with or just pass through
                    if (collision != TileCollision.Passable)
                    {
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds); // Checks how big the tile is
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            if (absDepthY < absDepthX || collision == TileCollision.Platform) // Determines the type of collision the tile has.
                            {
                                if (previousBottom <= tileBounds.Top)

                                    isOnGround = true; // Classifies the top of a collidable tile as 'the ground'.

                                if (collision == TileCollision.Impassable || IsOnGround) // Stops the player from walking horizontally through platforms
                                {
                                    Position = new Vector2(Position.X, Position.Y + depth.Y); // Lets the player jump through platforms with passable collision.

                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Stops the player from going through some platforms entirely.
                            {
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }
            previousBottom = bounds.Bottom; // Controls the location of the bottom of any given level
        }
        #endregion

        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play(); // Plays the killed sound if the player dies via enemy.
            else
                fallSound.Play(); // Plays the fall sound if the player dies via falling.

            sprite.PlayAnimation(dieAnimation); // Plays the death animation regardless of cause of death.
        }

        public void OnReachedExit()
        {
            sprite.PlayAnimation(idleAnimation); // Plays the idle animation when the exit is touched so the player isn't stuck in any animations when they reach the exit.
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch) // Shows the animations in the game.
        {
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally; // Makes the animation play the right way around.
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            sprite.Draw(gameTime, spriteBatch, Position, flip); // Draws the animation sprite.
        }
    }
}
