#region 'Using' information.
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Nightfall
{
    class ScorePickup
    {
        private Texture2D texture;
        private Texture2D knuckleTexture;
        private Vector2 origin;
        private SoundEffect collectedSound;
        private SoundEffect powerupCollectedSound;

        public readonly int PointValue = 10;
        public bool IsPowerUp { get; private set; }

        private Vector2 basePosition; // Calculates the original position of the pickup so it can 'bounce' properly.
        private float bounce; // Bounce will make the pickup move slightly so the game feels more 'alive'.

        public Level Level // Will later be used in level code to determine where in the level the pickup is.
        {
            get { return level; }
        }
        Level level;

        public Vector2 Position // Sets the position of the pickup.
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        public Circle BoundingCircle // Works with the tilemaps and circle class to allocate the pickup a spot in the levels.
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }

        public ScorePickup(Level level, Vector2 position, bool isPowerUp) // Constructs a new pickup and sets the level and base position it needs to define.
        {
            this.level = level;
            basePosition = position;

            IsPowerUp = isPowerUp;
            if (IsPowerUp)
            {
                PointValue = 10;
            }
            else
            {
                PointValue = 100;
            }

            LoadContent();
        }

        public void LoadContent() // Loads the pickup sprites and pickup noise.
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Pickup");
            knuckleTexture = Level.Content.Load<Texture2D>("Sprites/Pickup2");

            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);

            collectedSound = Level.Content.Load<SoundEffect>("Sounds/Sound - ExitReached"); // Plays the exit reached noise when a regular pickup is grabbed since it sounds suitable.
            powerupCollectedSound = Level.Content.Load<SoundEffect>("Sounds/Sound - PickupNoise"); // Plays the zappy pickup noise when a powerup is grabbed since it's suitable for taser knuckles.
        }

        public void Update(GameTime gameTime) // Makes the pickup move up and down so the game feels more 'alive' and encourages players to grab it.
        {
            // Bounce control constants
            const float BounceHeight = 0.06f; // Sets the maximum height the pickup can move to.
            const float BounceRate = 3.0f; // Sets the maximum speed the pickup can move at.
            const float BounceSync = -0.75f; // Sets the syncronisation of the bounces so that nearby pickups move in sync.

            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync; // Checks for nearby pickups on the same X axis so that they bounce in sync.
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height; // Makes the pickup bounce up and down in a curve.
        }
        public void OnCollected(Player playerCollected)
        {

            if (IsPowerUp == false)
            {
                collectedSound.Play(); // Plays the 'pickupnoise' sound only when the player grabs the pickup.
            }

            if (IsPowerUp)
            {
                powerupCollectedSound.Play(); // Plays the other 'pickupnoise' sound only when the player grabs the power up pickup.
                playerCollected.PowerUp(); // Begins the 'powered up' player state.
            }

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)  // Draws the pickup with references to texture, position, color etc. 
        {

            if (IsPowerUp == false)
            {
                spriteBatch.Draw(texture, Position, null, Color.AntiqueWhite, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            }        
            
            if (IsPowerUp)
            {
                spriteBatch.Draw(knuckleTexture, Position, null, Color.AntiqueWhite, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            }
        }
    }
}
