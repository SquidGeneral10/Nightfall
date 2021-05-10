#region 'Using' information
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endregion
namespace Nightfall
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        Vector2 baseScreenSize = new Vector2(1080, 720); // Used to resize the game window & center text / backgrounds.
        private SpriteFont Font; // Loads a font used for buttons and timer UI.

        public enum GameState { MainMenu, MainGame } // Adds names for the menu gamestates to use.
        public GameState gameState = GameState.MainMenu; //Sets the default gamestate to MainMenu.
        private Texture2D winOverlay; // the 'You Win' screen.
        private Texture2D loseOverlay; // the 'You Lose' screen.
        Texture2D screen01; // Sets a code reference for the MainMenu gameState's background to call.

        private List<Component> gameComponents; // Controls main menu screen buttons.
        private List<Component> gameComponents2; // Controls game over and game win screen buttons.

        private int levelIndex = -1; // Gets the level number
        private Level level; // Gets the level list
        private Player player;
        private bool wasContinuePressed; // Controls the player's movement between levels
        private const int numberOfLevels = 5; // Lets the system know how many levels are in the game so it knows when to loop and not look for extras.

        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30); // When the time remaining is under 30 seconds, its text blinks red.

        private KeyboardState keyboardState; // Will be used to keep track of which keys the player presses
        private AccelerometerState accelerometerState; // Will be used to keep track of player's movement acceleration

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;

            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1080;
            graphics.PreferredBackBufferHeight = 720;

            Accelerometer.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Font = Content.Load<SpriteFont>("Fonts/Font"); // Loads the font used for UI elements.

            screen01 = Content.Load<Texture2D>("Backgrounds/Layer0_0"); // Attaches the 'mainmenu' background image to the 'mainmenu' gamestate.
            winOverlay = Content.Load<Texture2D>("Backgrounds/You Win"); // Attaches the 'game win' background image to the screen whenever the player beats a level.
            loseOverlay = Content.Load<Texture2D>("Backgrounds/You Lose"); // Attaches the 'game over' background image to the screen when the player runs out of time or dies.


            #region Button texture code.
            var startButton = new ClickableButton(Content.Load<Texture2D>("Sprites/Button"), Content.Load<SpriteFont>("Fonts/Font"))
            {
                Position = new Vector2(50, 450),
                Text = "Start",
            };

            startButton.Click += StartButton_Click;

            var quitButton = new ClickableButton(Content.Load<Texture2D>("Sprites/Button"), Content.Load<SpriteFont>("Fonts/Font"))
            {
                Position = new Vector2(350, 450),
                Text = "Quit",
            };

            quitButton.Click += QuitButton_Click;

            gameComponents = new List<Component>() // Shows the start and quit buttons on any gamestate with this list.
            {
                startButton,
                quitButton,
            };

            gameComponents2 = new List<Component>() // Shows only the quit button on any gamestate with this list.
            {
                quitButton,
            };

            #endregion

            LoadNextLevel(); // Stops the game from starting on level -1.
        }

        #region Tells the game what to do when the quit / start buttons are pressed.
        private void QuitButton_Click(object sender, System.EventArgs e)
        {
            Exit(); // Closes the game when the 'quit' button is pressed.
        }

        private void StartButton_Click(object sender, System.EventArgs e)
        {
            { gameState = GameState.MainGame; }
            ReloadCurrentLevel(); // Loads the first level when the player clicks 'start'.
        }
        #endregion

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit(); // Closes the game when the player presses the escape key.

            switch (gameState)
            {
                case GameState.MainMenu: // Loads the quit and start buttons when the gamestate is set to the main menu.

                    IsMouseVisible = true; // Makes the mouse visible on the main menu so players can click the buttons.

                    foreach (var component in gameComponents)
                        component.Update(gameTime);

                    break;
            }

            HandleInput(gameTime); // constantly updates input so players can move when in a level.

            level.Update(gameTime, keyboardState, accelerometerState); // Runs level updates, sending it information for gametime and inputs alongside accelerometer data.

            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState(); // Runs constant checks to see if the player is pressing keyboard keys.
            accelerometerState = Accelerometer.GetState(); // Stops the player from accelerating too quickly.

            bool continuePressed =
                (Keyboard.GetState().GetPressedKeys().Length > 0); // The player can press any key to continue.

            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit) // Loads the next level if the player reached the exit before time ran out.
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel(); // Reloads the current level if the player didn't make it to the exit in time and pressed continue.
                }
            }

            wasContinuePressed = continuePressed;
        }
        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            if (level != null) // Unloads current level content before loading the next one's content.
                level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        private void ReloadCurrentLevel() // If the player dies, this stops them from moving to the next level and makes them replay the current one.
        {
            --levelIndex;
            LoadNextLevel();
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.DarkBlue); // DarkBlue specifically to make level 1's background look a bit nicer.

            spriteBatch.Begin();

            level.Draw(gameTime, spriteBatch); // Adds sprites to level elements like tiles, enemies, doors etc.

            DrawHud(); // Calls the code that adds the timer and score text to the screen.

            switch (gameState)
            {
                case GameState.MainMenu:
                    spriteBatch.Draw(screen01, Vector2.Zero, Color.White); // Adds the main menu background when on the main menu gamestate.

                    foreach (var component in gameComponents) // Adds the buttons and their textures to the screen when on the main menu gamestate.
                        component.Draw(gameTime, spriteBatch); // Adds the buttons and their textures to the screen when on the main menu gamestate.

                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Draws the timer and score text onto the screen.
        private void DrawHud() // Adds the timer and score text to the screen.
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);

            Vector2 center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);

            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");

            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Blue; // Makes the timer stay a solid blue when the player has plenty of time.
            }
            else
            {
                timeColor = Color.Red; // Makes the timer blink red when the player is near the end of the level's time limit.
            }

            DrawShadowedString(Font, timeString, hudLocation, timeColor);

            float timeHeight = Font.MeasureString(timeString).Y;

            DrawShadowedString(Font, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.BlueViolet); // shows the player's score in the top left part of the screen.

            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay; // Loads the 'You Win' screen if the player reaches the exit in time.
                }
                else
                {
                    status = loseOverlay; // Loads the 'You Lose' screen if the player does not reach the exit in time.
                }
            }
            else if (!level.Player.IsAlive)
            {
                status = loseOverlay; // Loads the 'You Lose' screen if the player is not alive.
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
        #endregion
    }
}
