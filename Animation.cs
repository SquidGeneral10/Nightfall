#region 'Using' information
using System;
using Microsoft.Xna.Framework.Graphics;
#endregion
namespace Nightfall
{
    class Animation
    {
        public Texture2D Texture
        {
            get { return texture; }
        }
        Texture2D texture;

        public float FrameTime
        {
            get { return frameTime; } // Determines how long the animation's current frame is shown.
        }
        float frameTime;

        public bool IsLooping
        {
            get { return isLooping; } // Determines whether or not the animation will loop when finished.
        }
        bool isLooping;

        public int FrameCount
        {
            get { return Texture.Width / FrameHeight; } // Checks the number of frames in the animation spritesheet.
        }

        public int FrameWidth
        {
            get { return Texture.Height; } // Checks the width of the animation frame.
        }

        public int FrameHeight
        {
            get { return Texture.Height; } // Checks the height of the animation frame.
        }

        public Animation(Texture2D texture, float frameTime, bool isLooping) // Acts as a template for new animations. Determines the texture, length and loop status of every animation.
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }
    }
}
