using System;
using System.Windows;
using System.Windows.Media;

namespace BP.Plankton.Model.Logic
{
    /// <summary>
    /// Provides helper functionality for generating visuals.
    /// </summary>
    public static class VisualGeneration
    {
        /// <summary>
        /// Generate a random linear gradient brush.
        /// </summary>
        /// <param name="startPoint">Specify the start point of the gradient.</param>
        /// <param name="endPoint">Specify the end point of the gradient.</param>
        /// <param name="minimumSteps">The minimum amount of steps in the gradient.</param>
        /// <param name="maximumSteps">The maximum amount of steps in the gradient.</param>
        /// <param name="random">The random generator.</param>
        /// <returns>The randomly generated brush.</returns>
        public static Brush GenerateRandomLinearGradientBrush(Point startPoint, Point endPoint, int minimumSteps, int maximumSteps, Random random)
        {
            var lGb = new LinearGradientBrush
            {
                StartPoint = startPoint,
                EndPoint = endPoint
            };

            var steps = random.Next(minimumSteps, maximumSteps);
            double segmentArea;

            if (steps > 2)
                segmentArea = 1.0d / (steps - 1);
            else
                segmentArea = double.NaN;

            var currentR = 0;
            var currentG = 0;
            var currentB = 0;

            int variance;

            if (steps > 1)
                variance = 256 / (steps - 1);
            else
                variance = 256;

            for (var index = 0; index < steps; index++)
            {
                if (index == 0)
                {
                    currentR = random.Next(0, 256);
                    currentG = random.Next(0, 256);
                    currentB = random.Next(0, 256);
                }
                else
                {
                    currentR = random.Next(Math.Max(currentR - variance, 0), Math.Min(currentR + variance, 256));
                    currentG = random.Next(Math.Max(currentG - variance, 0), Math.Min(currentG + variance, 256));
                    currentB = random.Next(Math.Max(currentB - variance, 0), Math.Min(currentB + variance, 256));
                }

                if (index == 0)
                    lGb.GradientStops.Add(new GradientStop(Color.FromArgb(255, (byte)currentR, (byte)currentG, (byte)currentB), 0.0d));
                else if (index == steps - 1)
                    lGb.GradientStops.Add(new GradientStop(Color.FromArgb(255, (byte)currentR, (byte)currentG, (byte)currentB), 1.0d));
                else
                    lGb.GradientStops.Add(new GradientStop(Color.FromArgb(255, (byte)currentR, (byte)currentG, (byte)currentB), segmentArea * index));
            }

            return lGb;
        }

        /// <summary>
        /// Generate a random texture.
        /// </summary>
        /// <param name="background">The brush to use for the texture background.</param>
        /// <param name="foreground">The pen to use for drawing foreground features on the texture.</param>
        /// <param name="dimensions">The dimensions of the texture.</param>
        /// <param name="polyLines">The amount of polylines to add to the texture.</param>
        /// <param name="segmentsInEachPolyLine">The number of segments that make up each polyline.</param>
        /// <param name="speckles">The number of speckles to add to the texture.</param>
        /// <param name="speckleRadius">The radius of each speckle.</param>
        /// <param name="random">The random generator.</param>
        /// <returns>A generated texture returned as a visual.</returns>
        public static Visual GenerateTexture(Brush background, Pen foreground, Size dimensions, int polyLines, int segmentsInEachPolyLine, int speckles, int speckleRadius, Random random)
        {
            var drawing = new DrawingVisual();
            var tileArea = new Rect(new Point(0, 0), dimensions);
            using (var dC = drawing.RenderOpen())
            {
                dC.DrawRectangle(background, new Pen(Brushes.Transparent, 0d), tileArea);

                for (var index = 0; index < 10; index++)
                {
                    var multiLineLastPoint = new Point(random.Next(0, (int)dimensions.Width), random.Next(0, (int)dimensions.Height));

                    for (var segmentPointIndex = 0; segmentPointIndex < 10; segmentPointIndex++)
                    {
                        var multiLineCurrentPoint = new Point(random.Next((int)Math.Max(0, multiLineLastPoint.X - dimensions.Width / 10), (int)Math.Min(dimensions.Width, multiLineLastPoint.X + dimensions.Width / 10)), random.Next((int)Math.Max(0, multiLineLastPoint.Y - dimensions.Height / 10), (int)Math.Min(dimensions.Height, multiLineLastPoint.Y + dimensions.Height / 10)));
                        dC.DrawLine(foreground, multiLineLastPoint, multiLineCurrentPoint);
                        multiLineLastPoint = multiLineCurrentPoint;
                    }
                }

                for (var index = 0; index < speckles; index++)
                    dC.DrawEllipse(foreground.Brush, foreground, new Point(random.Next(0 + speckleRadius, (int)dimensions.Width - speckleRadius), random.Next(0 + speckleRadius, (int)dimensions.Height - speckleRadius)), speckleRadius, speckleRadius);
            }

            return drawing;
        }
    }
}
