using System;
using System.Windows;
using System.Windows.Media;

namespace BP.Plankton.Model
{
    /// <summary>
    /// Provides helper functionality for math functions.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Determine the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The distance between the two points.</returns>
        public static double DetermineDistanceBetweenTwoPoints(Point a, Point b)
        {
            return Math.Abs(Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y)));
        }

        /// <summary>
        /// Determine the distance between two points.
        /// </summary>
        /// <param name="aX">Point a x location.</param>
        /// <param name="aY">Point a y location.</param>
        /// <param name="bX">Point b x location.</param>
        /// <param name="bY">Point b y location.</param>
        /// <returns>The distance between the two points.</returns>
        public static double DetermineDistanceBetweenTwoPoints(double aX, double aY, double bX, double bY)
        {
            return Math.Abs(Math.Sqrt((bX - aX) * (bX - aX) + (bY - aY) * (bY - aY)));
        }

        /// <summary>
        /// Determine if two regular circles intersect each other.
        /// </summary>
        /// <param name="aLeft">The a left position.</param>
        /// <param name="aTop">The a top position.</param>
        /// <param name="aWidth">The a width.</param>
        /// <param name="aHeight">The a height.</param>
        /// <param name="bLeft">The b left position.</param>
        /// <param name="bTop">The b top position.</param>
        /// <param name="bWidth">The be width.</param>
        /// <param name="bHeight">The b height.</param>
        /// <returns>True if the ellipses intersect or touch, else false.</returns>
        public static bool DoRegularCirclesIntersect(double aLeft, double aTop, double aWidth, double aHeight, double bLeft, double bTop, double bWidth, double bHeight)
        {
            if (Math.Abs(aWidth - aHeight) > 0.0 || Math.Abs(bWidth - bHeight) > 0.0)
                return new Rect(aLeft, aTop, aWidth, aHeight).IntersectsWith(new Rect(bLeft, bTop, bWidth, bHeight));

            return DetermineDistanceBetweenTwoPoints(aLeft + aWidth / 2d, aTop + aHeight / 2d, bLeft + bWidth / 2d, bTop + bHeight / 2d) <= aWidth / 2d + bWidth / 2d;
        }

        /// <summary>
        /// Determine if two regular circles intersect each other on a path.
        /// </summary>
        /// <param name="endALeft">The a left position.</param>
        /// <param name="endATop">The a top position.</param>
        /// <param name="startALeft">The start left position of the a ellipse.</param>
        /// <param name="startATop">The start top position of the a ellipse.</param>
        /// <param name="aWidth">The a width.</param>
        /// <param name="aHeight">The a height.</param>
        /// <param name="endBLeft">The b left position.</param>
        /// <param name="endBTop">The b top position.</param>
        /// <param name="bWidth">The be width.</param>
        /// <param name="bHeight">The b height.</param>
        /// <param name="startBLeft">The start left position of the a ellipse.</param>
        /// <param name="startBTop">The start top position of the a ellipse.</param>
        /// <param name="steps">The amount of steps to check on the vector path.</param>
        /// <returns>True if the ellipses intersect or touch, else false.</returns>
        public static bool DoRegularCirclesIntersectOnVectorPath(double endALeft, double endATop, double startALeft, double startATop, double aWidth, double aHeight, double endBLeft, double endBTop, double startBLeft, double startBTop, double bWidth, double bHeight, int steps)
        {
            var appliedSteps = Math.Min(Math.Abs(endALeft - startALeft) > 0.0 || Math.Abs(endATop - startATop) > 0.0 || Math.Abs(endBLeft - startBLeft) > 0.0 || Math.Abs(endBTop - startBTop) > 0.0 ? steps : 1, 10);

            for (var index = 0; index < appliedSteps; index++)
                if (DoRegularCirclesIntersect(endALeft - (startALeft - endALeft) / appliedSteps * (appliedSteps - index), endATop - (startATop - endATop) / appliedSteps * (appliedSteps - index), aWidth, aHeight, endBLeft - (startBLeft - endBLeft) / appliedSteps * (appliedSteps - index), endBTop - (startBTop - endBTop) / appliedSteps * (appliedSteps - index), bWidth, bHeight))
                    return true;

            return false;
        }

        /// <summary>
        /// Determine if two regular circles fully overlap each other.
        /// </summary>
        /// <param name="aLeft">The a lift position.</param>
        /// <param name="aTop">The a top position.</param>
        /// <param name="aRadius">The a radius.</param>
        /// <param name="bLeft">The b left position.</param>
        /// <param name="bTop">The b top position.</param>
        /// <param name="bRadius">The b radius.</param>
        /// <returns>True if the ellipses fully overlap, else false.</returns>
        public static bool DoRegularCirclesOverlap(double aLeft, double aTop, double aRadius, double bLeft, double bTop, double bRadius)
        {
            return DetermineDistanceBetweenTwoPoints(aLeft + aRadius, aTop + aRadius, bLeft + bRadius, bTop + bRadius) <= Math.Max(aRadius, bRadius) - Math.Min(aRadius, bRadius);
        }

        /// <summary>
        /// Determine a projected collision point for an Ellipse once a vector has been applied to it.
        /// </summary>
        /// <param name="ellipse">The rectangle that bounds a virtual Ellipse to use for determining the collision point.</param>
        /// <param name="vector">The vector of the ellipse.</param>
        /// <returns>The 2D point describing where to use for testing a projected collision.</returns>
        public static Point DetermineProjectedCollisionPoint(Rect ellipse, Vector vector)
        {
            // -calculate center point of ellipse
            // -using vector to get angle find the point on the ellipse that is going to connect first
            // -add VectorX and VectorY to projected point

            var angle = Math.Atan(vector.X / vector.Y);
            var collisionPoint = new Point(ellipse.Left + ellipse.Width / 2d, ellipse.Top + ellipse.Height / 2d);
            collisionPoint.X += Math.Sin(angle) * (ellipse.Width / 2d);
            collisionPoint.Y += Math.Cos(angle) * (ellipse.Height / 2d);
            collisionPoint.X += vector.X;
            collisionPoint.Y += vector.Y;
            return collisionPoint;
        }
    }
}
