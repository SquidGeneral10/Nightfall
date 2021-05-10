#region 'Using' information.
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Nightfall
{
    struct Circle
    {
        public Vector2 Center; // Sets a variable for the center of the circle - will be used for score pickup locations.

        public float Radius; // Circle's size.

        public Circle(Vector2 position, float radius) // Constructs a new circle and sets its center and radius as things it needs to define.
        {
            Center = position;
            Radius = radius;
        }

        public bool Intersects(Rectangle rectangle) // Checks for overlaps between the circle (score pickup) and rectangle (player).
        {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                                    MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            Vector2 direction = Center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }
    }
}
