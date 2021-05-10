#region 'Using' information
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nightfall
{
    enum TileCollision // Controls tile collisions.
    {
        Passable = 0, // Tiles that the player can walk through - mostly used for air or general foreground detail.

        Impassable = 1, // Tiles that the player cannot walk through - good for level boundaries.

        Platform = 2, // Tiles that let the player stand on top of them, and also jump through the bottom of them to get to the top.
    }

    enum DoorCollision // Controls tile collisions.
    {
        Exit = 0, // Doors that the player can use to complete a level.

        Portal = 1, // Doors that the player can use to appear at another door, like a portal.
    }

    struct Tile
    {
        public Texture2D Texture; // Controls how the tile will look.
        public TileCollision Collision; // Controls the type of collision the tile has.

        public const int Width = 32; // All tiles have a hitbox 32 pixels wide.
        public const int Height = 32; // All tiles have a hitbox 32 pixels tall.

        public static readonly Vector2 Size = new Vector2(Width, Height);

        public Tile(Texture2D texture, TileCollision collision)
        {
            Texture = texture;
            Collision = collision;
        }
    }

    struct Door
    {
        public Texture2D Texture; // Controls how the door will look.
        public DoorCollision Collision; // Controls the type of collision the door has.

        public const int Width = 32; // All tiles have a hitbox 40 pixels wide.
        public const int Height = 38; // All tiles have a hitbox 32 pixels tall.

        public static readonly Vector2 Size = new Vector2(Width, Height);

        public Door(Texture2D doortexture, DoorCollision doorcollision)
        {
            Texture = doortexture;
            Collision = doorcollision;
        }
    }
}
