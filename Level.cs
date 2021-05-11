#region 'Using' information
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Nightfall
{
    class Level : IDisposable // IDisposable means things that aren't needed for the current level can be unloaded to help the game run faster.
    {
        private Tile[,] tiles; // Will be used to control the spawning of 'tiles' which can be platforms, enemies, the player etc.
        private Texture2D[] layers; // Will be used to control the backgrounds loading in 'layers' on top of each other.        
        private const int EntityLayer = 2; // Makes sure entities like platforms, collectibles and the player appear in front of the background.

        public Player Player // Calls the player code to be used in the level later.
        {
            get { return player; }
        }

        Player player;

        private List<ScorePickup> pickups = new List<ScorePickup>(); // Defines a name for collectibles to use via list.
        private List<Enemy> enemies = new List<Enemy>(); // Defines a name for enemies to use via list.

        private Vector2 start; // Gives the player's starting point a name
        private Point exit = InvalidPosition; // Gives the player's exit point a name and sets its default state to 'invalid' so it turns to valid when one is found in the tilemap
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(458324); // Random but constant seed

        public int Score // Controls score being impacted by things in the level.
        {
            get { return score; }
        }

        public int score; // Sets the score as a number value (integer).

        public bool ReachedExit // Checks to see if the player has reached the exit.
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining // Used in later code to make the timer flash when time is running out.
        {
            get { return timeRemaining; }
        }

        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5; // Will later be used to give the player extra points for every second left on the clock when they finish level.

        public ContentManager Content // Will be used to call content that a level needs.
        {
            get { return content; }
        }

        ContentManager content;

        private SoundEffect exitReachedSound;

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            content = new ContentManager(serviceProvider, "Content"); // Create a new content manager to load content used just by this level.

            timeRemaining = TimeSpan.FromMinutes(2.0); // Sets a time limit of 2 minutes on every level.

            LoadTiles(fileStream);

            layers = new Texture2D[3]; // Loads background textures. 
            for (int i = 0; i < layers.Length; ++i)
            {
                // Gives each level a different background.
                int segmentIndex = levelIndex;
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            }

            exitReachedSound = Content.Load<SoundEffect>("Sounds/Sound - ExitReached"); // Loads the 'exit reached' sound.
        }
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length. If not, shows an error message to say which one is wrong.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(string.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            tiles = new Tile[width, lines.Count]; // Sets the tiles for the current level.

            for (int y = 0; y < Height; ++y) // Loads a tile's height.
            {
                for (int x = 0; x < Width; ++x) // Loads a tile's width.
                {
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            if (Player == null)
                throw new NotSupportedException("A level must have a starting point."); // Throws up an error message if the level doesn't have a player spawn point.
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit."); // Throws up an error message if the level doesn't have an exit.

        }

        private Tile LoadTile(char tileType, int x, int y) // Sets up the basis for a tilemap - each symbol represents something from platforms to enemies and pickups.
        {
            switch (tileType) // Determines the type of tile that should load - is it a blank space? is it an enemy? assigns letters to each of these.
            {
                // Spawns a blank space with no texture that players can go through at any '.' symbol. Basically, air.
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Loads the level exit at the X. Only one of these can exist per level.
                case 'X':
                    return LoadExitTile(x, y);

                // Spawns a Score Pickup worth 10 points at any 'G' symbol.
                case 'G':
                    return LoadPickupTile(x, y, false);

                // Spawns a Power Up Pickup worth 100 points that also lets players kill enemies at any 'P' symbol.
                case 'P':
                    return LoadPickupTile(x, y, true);

                // Spawns a platform you can jump through the bottom of + stand on at any '-' symbol Floating platform
                case '-':
                    return LoadTile("Tiles/Block", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadEnemyTile(x, y, "EnemyA");
                case 'B':
                    return LoadEnemyTile(x, y, "EnemyB");

                // Sets the player's spawn point at any '1' symbol. Only one of these can exist per level.
                case '1':
                    return LoadStartTile(x, y);

                // Spawns an block you can't go through at any '#' symbol. Good for keeping the player on the screen or making solid platforms.
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Throws up an error message if there's an unrecognised symbol in a level's tilemap.
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadTile(string name, TileCollision collision) // Loads a tile - this code is also used by other tiles to call their collision type and name.
        {
            return new Tile(Content.Load<Texture2D>("Tiles/BlockA0" + name), collision); // Sets the default tile texture to the grass platform's.
        }

        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision) // Loads non-standard tiles with a random appearance.
        {
            int index = random.Next(variationCount); // Checks for the number of variations in the tile's group.
            return LoadTile(baseName + index, collision); // This code is responsible for giving the tiles different appearances. It calls for the tile's base name and number, then its collision.
        }

        private Tile LoadStartTile(int x, int y) // Loads an instance of the player and stores their starting point in case they die and need to reset.
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point."); // Throws up an error message if the level has more than one starting point.

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y)); // Gives the player a rectangular collision box.
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable); // Makes the player's starting position a passable tile so they don't get stuck.
        }

        private Tile LoadExitTile(int x, int y) // Loads the level exit code on an exit tile.
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit."); // Throws an error message when a level tries to have more than one exit or put the exit in a floating tile.

            exit = GetDoorBounds(x, y).Center;// Finds the center of the exit tile so the player can collide with it.

            return LoadTile("Exit", TileCollision.Passable); // Lets the player collide with the exit by making it passable and able to overlap the player.
        }

        private Tile LoadEnemyTile(int x, int y, string spriteSet) // Adds an enemy code instance to enemy tiles.
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadPickupTile(int x, int y, bool isPowerUp) // Adds score pickup code to score pickup and power up tiles.
        {
            Point position = GetBounds(x, y).Center;
            pickups.Add(new ScorePickup(this, new Vector2(position.X, position.Y), isPowerUp));

            return new Tile(null, TileCollision.Passable);
        }

        public void Dispose()
        {
            Content.Unload(); // When it's no longer needed, unloads level content. This helps the game run faster.
        }

        public TileCollision GetCollision(int x, int y)
        {
            if (x < 0 || x >= Width)
                return TileCollision.Impassable; // Stops the player from running off the sides of the level.

            if (y < 0 || y >= Height)
                return TileCollision.Passable; // Lets the player jump past the top of the screen & fall through the bottom.

            return tiles[x, y].Collision;
        }

        public Rectangle GetBounds(int x, int y) // Gives all tiles a collision box.
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public Rectangle GetDoorBounds(int x, int y) // Gives all doors a larger collision box.
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.DoorWidth, Tile.DoorHeight);
        }

        public int Width
        {
            get { return tiles.GetLength(0); } // Measures the level's width in tiles.
        }

        public int Height
        {
            get { return tiles.GetLength(1); } // Measures the level's height in tiles.
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, AccelerometerState accelState)
        {
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero) // Pauses the game when the player's dead / run out of time.
            {
                Player.ApplyPhysics(gameTime); // Stops the player from falling through the floor when the game is paused.
            }

            else if (ReachedExit)
            {
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 75.0f); // Speeds up the game's timer by 75x.
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds)); // Calculates the time that was remaining when the player reaches the exit.
                timeRemaining -= TimeSpan.FromSeconds(seconds); // Takes away the time remaining until the timer reaches 0.
                score += seconds * PointsPerSecond; // Adds the remaining seconds to the player's score as a bonus for beating the level quickly.
            }

            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState, accelState);
                UpdatePickups(gameTime);

                if (Player.BoundingRectangle.Top >= Height * Tile.Height) // Kills the player if they fall off the level.
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime); // Makes enemies move.

                if (Player.IsAlive && // If the player is alive...
                    Player.IsOnGround && // ...And on the ground...
                    Player.BoundingRectangle.Contains(exit)) // ...And colliding with the exit...
                {
                    OnExitReached(); // ...then they have successfully reached the exit.
                }
            }

            if (timeRemaining < TimeSpan.Zero) // Stops the 'time remaining' from going below 0.
                timeRemaining = TimeSpan.Zero; // Stops the 'time remaining' from going below 0.
        }

        private void UpdatePickups(GameTime gameTime)
        {
            for (int i = 0; i < pickups.Count; ++i)
            {
                ScorePickup olmec = pickups[i];

                olmec.Update(gameTime); // Makes the pickup animate.

                if (olmec.BoundingCircle.Intersects(Player.BoundingRectangle)) // Removes the pickup and sends a signal for increasing score when the player collides with it.
                {
                    pickups.RemoveAt(i--);
                    OnPickupCollected(olmec, Player); // Removes score pickups when they've been touched by the player.
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (Player.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, Player);
                    }
                    else
                    {
                        OnPlayerKilled(enemy);
                    }
                }

                enemy.Update(gameTime); // Makes enemies animate and move.
            }
        }
        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnEnemyKilled(killedBy);
        }

        private void OnPickupCollected(ScorePickup pickup, Player playerCollected) // Increases player's score and plays pickup noise when a gem is collected.
        {
            score += pickup.PointValue; // Adds the pickup's score value to the player's score.

            pickup.OnCollected(playerCollected); // Plays the score pickup noise.
        }

        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy); // When the player is killed by an enemy, this sends a signal to show the 'you lose' screen.
        }
        private void OnExitReached() // When the player reaches the exit, play the exit reached sound and send a signal to load the next level.
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        public void StartNewLife()
        {
            Player.Reset(start); // When the player is killed and presses continue, this code resets them back to the starting point of the level.
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i <= EntityLayer; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White); // Draws the game's level backgrounds.

            DrawTiles(spriteBatch); // Draws every tile's sprites if they have them.

            foreach (ScorePickup pickups in pickups)
                pickups.Draw(gameTime, spriteBatch); // Draws score pickup textures on every pickup tile.

            Player.Draw(gameTime, spriteBatch); // Draws the player's sprite.

            foreach (Enemy enemy in enemies) // Draws enemy sprites on every enemy tile.
                enemy.Draw(gameTime, spriteBatch);

            for (int i = EntityLayer + 1; i < layers.Length; ++i) // Draws the backgrounds for every level.
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
        }

        private void DrawTiles(SpriteBatch spriteBatch) // Draws all tiles in a level.
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
    }
}
