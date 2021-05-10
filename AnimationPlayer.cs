#region 'Using' information.
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion
namespace Nightfall
{
    struct AnimationPlayer
    {
        public Animation Animation
        {
            get { return animation; } // Gets the currently playing animation.
        }

        Animation animation;

        public int FrameIndex
        {
            get { return frameIndex; } // Gets the current frame of the animation.
        }

        int frameIndex;

        private float time; // Used to determine how long an animation has been playing.

        public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); } // Sets the spawn point of each animation frame.
        }

        public void PlayAnimation(Animation animation) // Plays or keeps an animation playing.
        {
            if (Animation == animation) // Stops the current animation from restarting if it's active.
                return;

            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, Color color)
        {
            if (Animation == null)
                throw new NotSupportedException("No animation is currently playing."); // Displays an error message if there's no animation available.

            time += (float)gameTime.ElapsedGameTime.TotalSeconds; // Counts time passing to determine the new animation frame.
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                if (Animation.IsLooping) // Loops or stops the animation looping depending on whether or not it's an animation that should loop.
                {
                    frameIndex = (frameIndex + 1) % Animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                }
            }

            Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height); // Calculate the collision rectangle of the current frame.

            spriteBatch.Draw(Animation.Texture, position, source, color, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            Draw(gameTime, spriteBatch, position, spriteEffects, Color.White);
        }
    }
}
