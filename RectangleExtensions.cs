#region 'Using' information.
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Nightfall
{
    public static class RectangleExtensions
    {
        public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB) // Makes it possible for rectangles to collide (e.g player with enemy)
        {
            float halfWidthA = rectA.Width / 2.0f; // Calculates the halved size of a given rectangle. Makes it easier to find its center.
            float halfHeightA = rectA.Height / 2.0f; // Calculates the halved size of a given rectangle. Makes it easier to find its center.
            float halfWidthB = rectB.Width / 2.0f; // Calculates the halved size of a given rectangle. Makes it easier to find its center.
            float halfHeightB = rectB.Height / 2.0f; // Calculates the halved size of a given rectangle. Makes it easier to find its center.

            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA); // Calculates the center of rectangle A.
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB); // Calculates the center of rectangle B.

            float distanceX = centerA.X - centerB.X; // Calculates the horizontal distance between two rectangles to see if they have collided.
            float distanceY = centerA.Y - centerB.Y; // Calculates the vertical distance between two rectangles to see if they have collided.
            float minDistanceX = halfWidthA + halfWidthB; // Calculates the horizontal distance between two rectangles to see if they have collided.
            float minDistanceY = halfHeightA + halfHeightB; // Calculates the vertical distance between two rectangles to see if they have collided.

            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY) // Returns 0,0 if there's no intersection.
                return Vector2.Zero;

            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX; // Calculates the depth of any intersections - the center of two rectangles need to collide for it to count.
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY; // Calculates the depth of any intersections - the center of two rectangles need to collide for it to count.
            return new Vector2(depthX, depthY); // Returns with the value of the intersection for other code to use.
        }

        public static Vector2 GetBottomCenter(this Rectangle rect) // Gets the position of the center of the bottom edge of the rectangle.
        {
            return new Vector2(rect.X + rect.Width / 2.0f, rect.Bottom);
        }
    }
}
