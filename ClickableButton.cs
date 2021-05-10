#region 'Using' information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Nightfall
{
    public class ClickableButton : Component
    {
        #region Fields

        private MouseState _currentMouse; // Used to determine how the player clicks - have they clicked?

        private MouseState _previousMouse; // Used to determine how the player clicks - have they clicked and then released?

        private SpriteFont _font; // Used to determine the font that the button uses to draw text onto itself. ('start' / 'quit')

        private bool _isHovering; // Used with the mousestate to  they hovering over the button? 

        private Texture2D _texture; // Used to determine the button's texture so players can tell it apart from the background.

        #endregion

        #region Properties

        public event EventHandler Click;

        public bool Clicked { get; private set; } // Controls events whenever the button is clicked.

        public Color PenColour { get; set; } // Controls the colour of the text inside the button.

        public Vector2 Position { get; set; } // Controls the position of the button.

        public Rectangle Rectangle // Controls the size / collision of the button.
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
            }
        }

        public string Text { get; set; } // Used to have buttons with different text on them (e.g start / quit.)

        #endregion

        #region Methods

        public ClickableButton(Texture2D texture, SpriteFont font) // Makes a new constructor for buttons and sets its texture, pen colour and font as things it needs to define.
        {
            _texture = texture;

            _font = font;

            PenColour = Color.LightSteelBlue;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) // Draws the button with specific colours, changes the colour when hovered, and calls on the texture, font and text colour.
        {
            var colour = Color.White;// Makes the button white by default.

            if (_isHovering)
                colour = Color.Gray; // Turns the button gray when the mouse hovers over it.

            spriteBatch.Draw(_texture, Rectangle, colour);

            if (!string.IsNullOrEmpty(Text))
            {
                var x = (Rectangle.X + (Rectangle.Width / 2)) - (_font.MeasureString(Text).X / 2); // Centers the text inside the button.
                var y = (Rectangle.Y + (Rectangle.Height / 2)) - (_font.MeasureString(Text).Y / 2); // Centers the text inside the button.

                spriteBatch.DrawString(_font, Text, new Vector2(x, y), PenColour); // Adds the font and text colour to the button.
            }
        }

        public override void Update(GameTime gameTime) // Runs constant checks to see if the player has clicked on or is hovering over the button.
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();

            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            _isHovering = false;

            if (mouseRectangle.Intersects(Rectangle)) // Sets the button status to clicked when it is left clicked and released.
            {
                _isHovering = true;

                if (_currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed)
                {
                    Click?.Invoke(this, new EventArgs());
                }
            }
        }

        #endregion
    }
}
