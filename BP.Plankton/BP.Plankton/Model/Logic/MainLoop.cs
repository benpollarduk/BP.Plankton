using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using BP.Plankton.Controls;
using BP.Plankton.Model.Currents;
using BP.Plankton.Model.Rendering;

namespace BP.Plankton.Model.Logic
{
    /// <summary>
    /// Provides the main game loop.
    /// </summary>
    public static class MainLoop
    {
        /// <summary>
        /// Update a rectangle to border the perimeter of a FrameworkElement.
        /// </summary>
        /// <param name="rectangle">The rectangle to update.</param>
        /// <param name="element">The FrameworkElement to update the rectangle property to.</param>
        private static void UpdateRectangle(ref Rect rectangle, Geometry element)
        {
            // set bounds
            rectangle = element.Bounds;
        }

        /// <summary>
        /// Handle current updates.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="random">A random generator to use for all randomisation.</param>
        private static void UpdateCurrent(PlanktonControl control, Random random)
        {
            if (control.UseCurrent &&
                !control.ActiveCurrent.IsActive &&
                random.Next(0, 1000) < control.CurrentRate)
            {
                // set random current direction - just use between 20-160, and 200-340 to avoid annoying currents
                control.ActiveCurrent.Direction = control.UseRandomCurrentDirection ? random.Next(0, 2) % 2 == 0 ? random.Next(20, 160) : random.Next(200, 340) : control.CurrentDirection;
                control.ActiveCurrentDirection = Math.Round(control.ActiveCurrent.Direction, 0d);
                control.ActiveCurrent.Strength = control.CurrentStrength - control.CurrentStrength / 100d * random.Next(0, (int)control.CurrentVariation);
                control.ActiveCurrent.Deceleration = control.IgnoreWaterViscosityWhenGeneratingCurrent ? control.CurrentDeceleration : control.WaterViscosity;
                control.ActiveCurrent.Acceleration = control.IgnoreWaterViscosityWhenGeneratingCurrent ? control.CurrentAcceleration : control.WaterViscosity;
                control.ActiveCurrent.ZAdjustmentPerStep = control.UseZOnCurrent ? Current.GenerateZStep(control.CurrentZStep, control.CurrentZStepVariation, control.ActiveCurrent.MaximumZAdjustment, control.ActiveCurrent.MinimumZAdjustment, control.CurrentZAdjustemnt, random) : 0.0d;
                control.ActiveCurrent.Start(control.CurrentMode);
                control.IsCurrentActive = true;
            }
            else if (control.UseCurrent && control.ActiveCurrent.IsActive)
            {
                control.ActiveCurrent.IncrementToNextStep();

                if (control.ShowCurrentIndicator)
                {
                    if (!control.UseAnimation)
                    {
                        control.CurrentIndicatorMasterGrid.Opacity = 1;
                    }
                    else
                    {
                        var strength = 1d / 100d * control.ActiveCurrent.GetCurrentStrengthOfTotalStrength();
                        control.CurrentIndicatorMasterGrid.Opacity = Math.Min(1.0d, Math.Max(strength, 0.25d));
                    }
                }

                control.CurrentZAdjustemnt += control.ActiveCurrent.ActiveStep().Z;

                // determine degrees - add 90 as Atan2 returns north as -90, east a 0, south as 90 and west as 180
                control.ActiveCurrentDirection = Math.Round(90 + Math.Atan2(control.ActiveCurrent.ActiveStep().Y, control.ActiveCurrent.ActiveStep().X) * (180d / Math.PI), 0d);
            }
            else if (control.UseCurrent && !control.ActiveCurrent.IsActive)
            {
                control.IsCurrentActive = false;
                control.ActiveCurrentDirection = Math.Round(control.ActiveCurrent.Direction, 0d);
            }
            else if (control.UseCurrent && control.ActiveCurrent.IsActive)
            {
                control.ActiveCurrent.Stop();
                control.IsCurrentActive = false;
                control.ActiveCurrentDirection = Math.Round(control.ActiveCurrent.Direction, 0d);
            }
        }

        /// <summary>
        /// Update the bubbles.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="area">The area.</param>
        /// <param name="random">A random generator to use for all randomisation.</param>
        /// <param name="mouseVector">The vector of the last mouse move.</param>
        /// <param name="useSeaBed">If the sea bed is being used.</param>
        /// <param name="seaBedHeight">The height of the sea bed.</param>
        /// <param name="mousePosition">The current mouse position.</param>
        /// <param name="currentBubbleElements">The current child bubble elements.</param>
        /// <param name="forceBubbleRerender">If bubble re-render should be forced.</param>
        /// <param name="mainBubbleState">The main bubble state.</param>
        /// <param name="mainBubbleRectangle">A rectangle describing the main bubble.</param>
        private static void UpdateBubbles(PlanktonControl control, FrameworkElement area, Random random, Vector mouseVector, bool useSeaBed, double seaBedHeight, Point mousePosition, out int currentBubbleElements, out bool forceBubbleRerender, out bool mainBubbleState, out Rect mainBubbleRectangle)
        {
            var bubbleBrush = control.FindResource("BubbleBrush") as Brush;
            var maxBubbleSize = control.BubbleSize * Math.PI;
            var childBubbleBuoyancy = control.ChildBubbleBuoyancy * control.WaterViscosity;
            mainBubbleRectangle = new Rect();
            forceBubbleRerender = false;

            if (control.Bubble != null)
                control.Bubble.Vector = mouseVector;

            // if generating child bubble - current count less than maximum, randomly generated (using 200 to half rate to a maximum of 50%), or left mouse button is down
            if (area.IsMouseOver && (control.UseChildBubbles && control.ChildBubbles.Count < control.MaximumChildBubbles && random.Next(0, 200) < control.ChildBubbleRate || Mouse.LeftButton == MouseButtonState.Pressed && control.ChildBubbles.Count < control.MaximumChildBubbles))
            {
                // check not over sea bed
                if (useSeaBed && mousePosition.Y > area.ActualHeight - seaBedHeight &&
                    control.SeaBedGeometry.FillContains(mousePosition))
                {
                    // don't bother creating a bubble over the sea bed
                }
                else
                {
                    var relativeChildBubbleDimension = control.BubbleSize / 2d;
                    relativeChildBubbleDimension -= relativeChildBubbleDimension / 100d * random.Next(0, (int)control.ChildBubbleSizeVariation);
                    control.ChildBubbles.Add(MoveableElement.Create(new Point(mousePosition.X - relativeChildBubbleDimension / 2d, mousePosition.Y - relativeChildBubbleDimension / 2d), Math.Max(3, relativeChildBubbleDimension), new Vector(0d, -childBubbleBuoyancy), control.BubblePen, bubbleBrush), true);
                    forceBubbleRerender = true;
                }
            }

            if (control.ChildBubbles.Count > 0)
            {
                var bubblesToCheck = control.ChildBubbles.Keys.ToArray();
                foreach (var childBubbleElement in bubblesToCheck)
                {
                    var normalisedBubbleSizeComparedToBiggest = 1d / maxBubbleSize * ((childBubbleElement.Geometry.RadiusX + childBubbleElement.Geometry.RadiusY) / 2d * Math.PI);

                    if (!control.ChildBubbles[childBubbleElement]) 
                        continue;

                    if (control.UseCurrent)
                    {
                        if (useSeaBed &&
                            childBubbleElement.Geometry.Center.Y + childBubbleElement.Geometry.RadiusY + control.ActiveCurrent.ActiveStep().Z + control.ActiveCurrent.ActiveStep().Y - childBubbleBuoyancy > area.ActualHeight - seaBedHeight &&
                            control.SeaBedGeometry.FillContains(new Point(childBubbleElement.Geometry.Center.X + control.ActiveCurrent.ActiveStep().Z + control.ActiveCurrent.ActiveStep().X, childBubbleElement.Geometry.Center.Y + childBubbleElement.Geometry.RadiusY + control.ActiveCurrent.ActiveStep().Z + control.ActiveCurrent.ActiveStep().Y - childBubbleBuoyancy)))
                        {
                            control.PopChildBubble(childBubbleElement);
                            forceBubbleRerender = true;
                        }
                        else
                        {
                            childBubbleElement.Geometry.RadiusX = Math.Max(0, Math.Min(control.BubbleSizeSlider.Maximum / 2d, childBubbleElement.Geometry.RadiusX + control.ActiveCurrent.ActiveStep().Z * (2d - normalisedBubbleSizeComparedToBiggest)));
                            childBubbleElement.Geometry.RadiusY = Math.Max(0, Math.Min(control.BubbleSizeSlider.Maximum / 2d, childBubbleElement.Geometry.RadiusY + control.ActiveCurrent.ActiveStep().Z * (2d - normalisedBubbleSizeComparedToBiggest)));
                            childBubbleElement.Geometry.Center = new Point(childBubbleElement.Geometry.Center.X + control.ActiveCurrent.ActiveStep().X * (2d - normalisedBubbleSizeComparedToBiggest), childBubbleElement.Geometry.Center.Y - childBubbleBuoyancy + control.ActiveCurrent.ActiveStep().Y * (2d - normalisedBubbleSizeComparedToBiggest));
                        }
                    }
                    else
                    {
                        childBubbleElement.Geometry.Center = new Point(childBubbleElement.Geometry.Center.X, childBubbleElement.Geometry.Center.Y - childBubbleBuoyancy);
                    }

                    // if bubble is still valid after current modification
                    if (!control.ChildBubbles[childBubbleElement])
                        continue;

                    UpdateRectangle(ref mainBubbleRectangle, childBubbleElement.Geometry);

                    if (useSeaBed)
                    {
                        if (!(childBubbleElement.Vector.Y < 0) && !(control.ActiveCurrent.ActiveStep().Y < 0))
                            continue;

                        if (mainBubbleRectangle.Y + mainBubbleRectangle.Height <= 0)
                        {
                            control.PopChildBubble(childBubbleElement);
                        }
                        else if (childBubbleElement.Vector.Y > 0 || control.ActiveCurrent.ActiveStep().Y > 0)
                        {
                            if (mainBubbleRectangle.Y > area.ActualHeight)
                            {
                                control.PopChildBubble(childBubbleElement);
                                forceBubbleRerender = true;
                            }
                            else if (mainBubbleRectangle.Y + mainBubbleRectangle.Height >= area.ActualHeight - seaBedHeight &&
                                     control.SeaBedGeometry.StrokeContainsWithDetail(control.SeaBedPen, childBubbleElement.Geometry) != IntersectionDetail.Empty ||
                                     control.SeaBedGeometry.FillContainsWithDetail(childBubbleElement.Geometry) != IntersectionDetail.Empty)
                            {
                                control.PopChildBubble(childBubbleElement);
                                forceBubbleRerender = true;
                            }
                        }
                    }
                    else
                    {
                        // if going up and off top of screen, or going down and off bottom of screen
                        if ((childBubbleElement.Vector.Y < 0 || control.ActiveCurrent.ActiveStep().Y < 0) &&
                            mainBubbleRectangle.Y + mainBubbleRectangle.Height <= 0 || (childBubbleElement.Vector.Y > 0 || control.ActiveCurrent.ActiveStep().Y > 0) && mainBubbleRectangle.Y > area.ActualHeight)
                        {
                            control.PopChildBubble(childBubbleElement);
                        }
                    }
                }

                var invalidBubbles = control.ChildBubbles.Keys.Where(x => !control.ChildBubbles[x]).ToArray();

                if (invalidBubbles.Length > 0)
                {
                    foreach (var eL in invalidBubbles)
                    {
                        control.ChildBubbles.Remove(eL);
                    }
                }

                currentBubbleElements = control.ChildBubbles.Count;
            }
            else
            {
                currentBubbleElements = 0;
            }
        }

        /// <summary>
        /// Update the plankton.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="area">The area</param>
        /// <param name="random">A random generator to use for all randomisation.</param>
        /// <param name="useSeaBed">If the sea bed is used.</param>
        /// <param name="seaBedHeight">The sea bed height.</param>
        /// <param name="currentBubbleElements">The number of bubble elements.</param>
        /// <param name="bubbleElementRectangle">A rectangle describing the main bubble.</param>
        /// <param name="mainBubbleCollisions">The number of collisions with the main bubble.</param>
        private static void UpdatePlankton(PlanktonControl control, FrameworkElement area, Random random, bool useSeaBed, double seaBedHeight, int currentBubbleElements, Rect bubbleElementRectangle, out int mainBubbleCollisions)
        {
            var centerPointOfElement = new Point(0, 0);
            var planktonElementRectangle = new Rect();
            var useGravity = control.UseGravity;
            var density = control.Density;
            var maxElementMass = control.ElementsSize * Math.PI * control.Density;
            var planktonAttractionStrength = control.PlanktonAttractionStrength / 10d;
            var actualTravel = control.Travel / 10d;
            mainBubbleCollisions = 0;

            for (var planktonIndex = 0; planktonIndex < control.Elements; planktonIndex++)
            {
                var planktonElement = control.Plankton[planktonIndex];
                var planktonVector = planktonElement.Vector;
                var originalVectorOfPlanktonElement = planktonVector;
                centerPointOfElement.X = planktonElement.Geometry.Center.X;
                centerPointOfElement.Y = planktonElement.Geometry.Center.Y;
                var massOfPlanktonElement = (planktonElement.Geometry.RadiusX + planktonElement.Geometry.RadiusY) / 2d * Math.PI * density;
                var effectOfMassOfPlanktonElement = massOfPlanktonElement / 1000d * control.WaterViscosity;
                var normalisedMassComparedToBiggest = 1d / maxElementMass * massOfPlanktonElement;

                // if using current and no sea bed, or element is above sea bed - sea bed shelters from current
                if (control.UseCurrent &&
                    (!useSeaBed || planktonElement.Geometry.Center.Y + planktonElement.Geometry.RadiusY < area.ActualHeight - seaBedHeight))
                {
                    planktonElement.Geometry.RadiusX = Math.Max(0, Math.Min(control.ElementsSizeSlider.Maximum / 2d, planktonElement.Geometry.RadiusX + control.ActiveCurrent.ActiveStep().Z * (2d - normalisedMassComparedToBiggest)));
                    planktonElement.Geometry.RadiusY = Math.Max(0, Math.Min(control.ElementsSizeSlider.Maximum / 2d, planktonElement.Geometry.RadiusY + control.ActiveCurrent.ActiveStep().Z * (2d - normalisedMassComparedToBiggest)));
                    centerPointOfElement.X = Math.Max(0 + planktonElement.Geometry.RadiusX, Math.Min(area.ActualWidth - planktonElement.Geometry.RadiusX, planktonElement.Geometry.Center.X + control.ActiveCurrent.ActiveStep().X * (2d - normalisedMassComparedToBiggest)));
                    centerPointOfElement.Y = Math.Max(0 + planktonElement.Geometry.RadiusY, Math.Min(area.ActualHeight - planktonElement.Geometry.RadiusY, planktonElement.Geometry.Center.Y + control.ActiveCurrent.ActiveStep().Y * (2d - normalisedMassComparedToBiggest)));
                    planktonElement.Geometry.Center = centerPointOfElement;
                }

                UpdateRectangle(ref planktonElementRectangle, planktonElement.Geometry);

                if (Math.Abs(planktonVector.X) + Math.Abs(planktonVector.Y) > actualTravel)
                {
                    // reduce by unit - linear deceleration
                    planktonVector.X *= control.WaterViscosity;
                    planktonVector.Y *= control.WaterViscosity;
                }

                MoveableElement closestBubble = null;
                double closestBubbleProximity = int.MaxValue;

                for (var bubbleIndex = control.Bubble != null ? -1 : 0; bubbleIndex < currentBubbleElements; bubbleIndex++)
                {
                    var bubbleElement = bubbleIndex == -1 ? control.Bubble : control.ChildBubbles.Keys.ElementAt(bubbleIndex);
                    UpdateRectangle(ref bubbleElementRectangle, bubbleElement.Geometry);

                    // if plankton is fully inside the bubble
                    if (MathHelper.DoRegularCirclesOverlap(bubbleElementRectangle.Left, bubbleElementRectangle.Top, bubbleElementRectangle.Width / 2d, planktonElementRectangle.Left, planktonElementRectangle.Top, planktonElementRectangle.Width / 2d))
                    {
                        // get angle from center of bubble to center of area
                        var angle = Math.Round(90 + Math.Atan(area.ActualWidth / 2d - (planktonElementRectangle.X + planktonElementRectangle.Width / 2d)) / (area.ActualHeight / 2d - (planktonElementRectangle.Y + planktonElementRectangle.Height / 2d)) * (180d / Math.PI), 0d);

                        // specify ejection vector
                        planktonVector.X = actualTravel * 5d * Math.Sin(angle);
                        planktonVector.Y = actualTravel * 5d * Math.Cos(angle);
                    }
                    else if (MathHelper.DoRegularCirclesIntersect(bubbleElementRectangle.Left, bubbleElementRectangle.Top, bubbleElementRectangle.Width, bubbleElementRectangle.Height, planktonElementRectangle.Left, planktonElementRectangle.Top, planktonElementRectangle.Width, planktonElementRectangle.Height))
                    {
                        // if in left side of bubble and moving left or right side of bubble and moving right
                        if (planktonElementRectangle.Left + planktonElementRectangle.Width / 2d <= bubbleElementRectangle.Left + bubbleElementRectangle.Width / 2d && planktonVector.X > 0 || planktonElementRectangle.Left + planktonElementRectangle.Width / 2d > bubbleElementRectangle.Left + bubbleElementRectangle.Width / 2d && planktonVector.X < 0)
                            planktonVector.X = -planktonVector.X;
                        else if (Math.Abs(bubbleElement.Vector.X) > 0.0)
                            planktonVector.X = bubbleElement.Vector.X;

                        // if in top side of bubble and moving down or bottom side of bubble and moving up
                        if (planktonElementRectangle.Top + planktonElementRectangle.Height / 2d <= bubbleElementRectangle.Top + bubbleElementRectangle.Height / 2d && planktonVector.Y > 0 || planktonElementRectangle.Top + planktonElementRectangle.Height / 2d > bubbleElementRectangle.Top + bubbleElementRectangle.Height / 2d && planktonVector.Y < 0)
                            planktonVector.Y = -planktonVector.Y;
                        else if (Math.Abs(bubbleElement.Vector.Y) > 0.0)
                            planktonVector.Y = bubbleElement.Vector.Y;

                        // if first, therefore main bubble increment collisions
                        if (bubbleElement == control.Bubble)
                            mainBubbleCollisions++;

                        break;
                    }
                    else if (control.UsePlanktonAttraction && (bubbleElement == control.Bubble || control.PlanktonAttractToChildBubbles && bubbleElement != control.Bubble))
                    {
                        var bubbleProximity = MathHelper.DetermineDistanceBetweenTwoPoints(bubbleElementRectangle.Left + bubbleElementRectangle.Width / 2d, bubbleElementRectangle.Top + bubbleElementRectangle.Height / 2d, planktonElementRectangle.Left + planktonElementRectangle.Width / 2d, planktonElementRectangle.Top + planktonElementRectangle.Height / 2d);

                        // else if within radius of attraction
                        if (bubbleProximity <= bubbleElementRectangle.Width / 2d * control.PlanktonAttractionReach)
                        {
                            // if closer than previous closest bubble, or no previous close bubble
                            if (closestBubble == null || closestBubbleProximity > bubbleProximity)
                            {
                                closestBubble = bubbleElement;
                                closestBubbleProximity = bubbleProximity;
                            }
                        }
                    }

                    // if a closest bubble found
                    if (closestBubble == null)
                        continue;

                    UpdateRectangle(ref bubbleElementRectangle, closestBubble.Geometry);
                    planktonVector.X = Math.Min(Math.Max(Math.Abs(bubbleElementRectangle.Left + bubbleElementRectangle.Width / 2d - (planktonElementRectangle.Left + planktonElementRectangle.Width / 2d)) - (planktonElementRectangle.Width / 2d + bubbleElementRectangle.Width / 2d), 0), Math.Max(actualTravel, planktonAttractionStrength)) * (bubbleElementRectangle.Left + bubbleElementRectangle.Width / 2d < planktonElementRectangle.Left + planktonElementRectangle.Width / 2d ? -1 : 1) * (control.InvertPlanktonAttraction ? -1 : 1);
                    planktonVector.Y = Math.Min(Math.Max(Math.Abs(bubbleElementRectangle.Top + bubbleElementRectangle.Height / 2d - (planktonElementRectangle.Top + planktonElementRectangle.Height / 2d)) - (planktonElementRectangle.Height / 2d + bubbleElementRectangle.Height / 2d), 0), Math.Max(actualTravel, planktonAttractionStrength)) * (bubbleElementRectangle.Top + bubbleElementRectangle.Height / 2d < planktonElementRectangle.Top + planktonElementRectangle.Height / 2d ? -1 : 1) * (control.InvertPlanktonAttraction ? -1 : 1);
                }

                while (control.BubbleCollisionHistory.Count == 10)
                    control.BubbleCollisionHistory.Dequeue();

                control.BubbleCollisionHistory.Enqueue(mainBubbleCollisions);

                // if using life, vector hasn't changed, and random generator decides that vector should be changed, and not moving at an accelerated speed
                if (control.Life > 0 &&
                    Math.Abs(originalVectorOfPlanktonElement.X - planktonVector.X) < 0.0 &&
                    Math.Abs(originalVectorOfPlanktonElement.Y - planktonVector.Y) < 0.0 && random.Next(1, 100) <= control.Life &&
                    planktonVector.Length <= actualTravel)
                {
                    planktonVector = Current.GetRandomVector(actualTravel, random);
                }

                if (useGravity)
                    planktonVector.Y += effectOfMassOfPlanktonElement;

                // if off left of screen - for example a resize or error ocured
                if (planktonElementRectangle.Left + planktonElementRectangle.Width < 0)
                {
                    // set center point so elements just off of screen
                    centerPointOfElement.X = -(planktonElementRectangle.Width / 2d);

                    // set vector to make element come back on to screen
                    planktonVector.X = Math.Max(5d, actualTravel);
                    planktonElement.Geometry.Center = centerPointOfElement;
                }
                else if (planktonElementRectangle.Left > area.ActualWidth)
                {
                    // set center point so elements just off of screen
                    centerPointOfElement.X = area.ActualWidth + planktonElementRectangle.Width / 2d;

                    // set vector to make element come back on to screen
                    planktonVector.X = -Math.Max(5d, actualTravel);
                    planktonElement.Geometry.Center = centerPointOfElement;
                }
                else if (planktonElementRectangle.X + planktonElementRectangle.Width + planktonVector.X > area.ActualWidth &&
                         (planktonVector.X > 0 || control.ActiveCurrent.ActiveStep().X > 0) || planktonElementRectangle.X + planktonVector.X < 0 && (planktonVector.X < 0 || control.ActiveCurrent.ActiveStep().X < 0))
                {
                    // colliding with left or right of area

                    // invert vector x
                    planktonVector.X = -planktonVector.X;
                }

                // if off top of screen - for example a resize or error ocured
                if (planktonElementRectangle.Top + planktonElementRectangle.Height < 0)
                {
                    // set center point so elements just off of screen
                    centerPointOfElement.Y = -(planktonElementRectangle.Height / 2d);

                    // set vector to make element come back on to screen
                    planktonVector.Y = Math.Max(5d, actualTravel);
                    planktonElement.Geometry.Center = centerPointOfElement;
                }
                else if (planktonElementRectangle.Top > area.ActualHeight)
                {
                    // set center point so elements just off of screen
                    centerPointOfElement.Y = area.ActualHeight + planktonElementRectangle.Height / 2d;

                    // set vector to make element come back on to screen
                    planktonVector.Y = -Math.Max(5d, actualTravel);
                    planktonElement.Geometry.Center = centerPointOfElement;
                }
                else if (planktonElementRectangle.Y + planktonElementRectangle.Height + planktonVector.Y > area.ActualHeight && (planktonVector.Y > 0 || control.ActiveCurrent.ActiveStep().Y > 0) || planktonElementRectangle.Y + planktonVector.Y < 0 && (planktonVector.Y < 0 || control.ActiveCurrent.ActiveStep().Y < 0))
                {
                    // colliding with top or bottom of area

                    if (useGravity)
                        planktonVector.Y = planktonElementRectangle.Y + planktonElementRectangle.Height / 2d >= area.ActualHeight / 2d ? 0d : effectOfMassOfPlanktonElement;
                    else
                        planktonVector.Y = -planktonVector.Y;
                }

                // if using sea bed and in a collision range
                if (useSeaBed && planktonElementRectangle.Top + planktonElementRectangle.Height >= area.ActualHeight - seaBedHeight)
                {
                    var fillContainsElement = control.SeaBedGeometry.FillContainsWithDetail(planktonElement.Geometry) != IntersectionDetail.Empty;
                    var strokeContainsElement = control.SeaBedGeometry.StrokeContainsWithDetail(control.SeaBedPen, planktonElement.Geometry) != IntersectionDetail.Empty;

                    // if fill contains element - i.e somewhere there is a glitch and the element has become embedded in the sea bed
                    if (fillContainsElement && !strokeContainsElement)
                    {
                        // encapsulated within the sea bed, uh oh, eject!

                        centerPointOfElement.X = planktonElement.Geometry.Center.X + planktonVector.X;
                        centerPointOfElement.Y = area.ActualHeight - seaBedHeight - planktonElementRectangle.Height / 2d;
                        planktonElement.Geometry.Center = centerPointOfElement;
                        planktonVector.X = 0d;
                        planktonVector.Y = -Math.Max(Math.Abs(planktonVector.Y), actualTravel);
                    }
                    else if (strokeContainsElement)
                    {
                        // if collided with the sea bed

                        planktonVector.X = -planktonVector.X;

                        if (useGravity)
                            planktonVector.Y = planktonVector.Y < 0d ? planktonVector.Y : Math.Abs(planktonVector.Y) > 0.0 ? -Math.Abs(planktonVector.Y * (1.1d - normalisedMassComparedToBiggest)) : 0d;
                        else
                            planktonVector.Y = -Math.Abs(planktonVector.Y);
                    }
                }

                planktonElement.Vector = planktonVector;
                centerPointOfElement.X = planktonElement.Geometry.Center.X + planktonVector.X;
                centerPointOfElement.Y = planktonElement.Geometry.Center.Y + planktonVector.Y;
                planktonElement.Geometry.Center = centerPointOfElement;
            }
        }

        /// <summary>
        /// Update the preview.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="area">The area.</param>
        /// <param name="mousePosition">The mouse position.</param>
        private static void UpdatePreview(PlanktonControl control, MoveableElementsHost area, Point mousePosition)
        {
            if (!control.UseZoomPreview)
                return;

            var zoomPreviewBlurCorrectionFactor = 0.0d;

            var autoPanSensitity = control.ZoomSensitivitySlider.Maximum + control.ZoomSensitivitySlider.Minimum - control.AutoPanSensitivity;

            if (control.UseZoomPreviewBlurEffect)
            {
                BlurEffect blurEffect;

                if (control.UseZoomPreviewBlurEffect)
                {
                    blurEffect = control.PreviewAreaPresenter.Effect as BlurEffect;
                    zoomPreviewBlurCorrectionFactor = control.ZoomPreviewBlurCorrection;
                }
                else
                {
                    blurEffect = null;
                }

                if (blurEffect != null)
                {
                    lock (blurEffect)
                    {
                        if (Math.Abs(blurEffect.Radius - control.NextZoomPreviewBlurRadius) > 0.0)
                            blurEffect.Radius = control.NextZoomPreviewBlurRadius;

                        if (control.NextZoomPreviewBlurRadius > 0.0d)
                            control.NextZoomPreviewBlurRadius = Math.Max(0, control.NextZoomPreviewBlurRadius - zoomPreviewBlurCorrectionFactor);
                    }
                }
            }

            var zoomPreviewVector = control.ZoomPreviewVector;

            if (control.UseAutoPanOnZoomPreview)
            {
                var averageCollision = control.BubbleCollisionHistory.Average(i => i);

                if (averageCollision > 7d * autoPanSensitity)
                    zoomPreviewVector.Z = control.ZoomPreviewFactor < control.MinimumZoom ? control.AutoPanSpeed : 0.0d;
                else if (averageCollision > 5d * autoPanSensitity)
                    zoomPreviewVector.Z = control.ZoomPreviewFactor < 4d ? control.AutoPanSpeed : 0.0d;
                else if (averageCollision > 3d * autoPanSensitity)
                    zoomPreviewVector.Z = control.ZoomPreviewFactor < 3d ? control.AutoPanSpeed : 0.0d;
                else
                    zoomPreviewVector.Z = control.ZoomPreviewFactor > control.MinimumZoom ? -control.AutoPanSpeed : 0.0d;
            }
            else
            {
                zoomPreviewVector.Z = control.ZoomPreviewFactor > control.MaximumZoom ? -control.AutoPanSpeed : 0.0d;
            }

            if (Math.Abs(zoomPreviewVector.Z) > 0.0 && control.Bubble != null)
            {
                zoomPreviewVector.Z += zoomPreviewVector.Z;
                control.UpdateZoomPreview(mousePosition, control.GetPreviewFocusElement(control.ZoomPreviewLocaterMode, out var showLocaterLine), area, control.ZoomPreviewFactor, showLocaterLine);
            }
            else if (control.IfMainBubbleNotAvailablePreviewMostInterestingElement)
            {
                // using alternate focus and main bubble has become hidden

                control.ZoomPreviewFactor = control.MinimumZoom;
                var focusElement = control.GetPreviewFocusElement(control.ZoomPreviewLocaterMode, out var showLocaterLine);
                var focusPointForPreview = focusElement?.Geometry.Center ?? new Point(area.ActualWidth / 2d, area.ActualHeight / 2d);
                control.UpdateZoomPreview(focusPointForPreview, focusElement, area, control.ZoomPreviewFactor, showLocaterLine);
            }

            control.ZoomPreviewVector = zoomPreviewVector;
        }

        /// <summary>
        /// Update the main loop.
        /// </summary>
        /// <param name="control">The control to update.</param>
        /// <param name="area">The game area.</param>
        /// <param name="random">A random generator used to handle randomisation.</param>
        /// <param name="mouseVector">The vector of the last mouse move.</param>
        /// <param name="maintainAnyGeneratedBrushes">If generated brushes should be maintained.</param>
        public static void Update(PlanktonControl control, MoveableElementsHost area, Random random, Vector mouseVector, bool maintainAnyGeneratedBrushes)
        {
            // if not updating already - too many elements on a slow processor could lock up if this is called too frequently
            if (control.IsUpdating)
                return;

            control.IsUpdating = true;

            var seaBedHeight = control.SeaBedGeometry?.Bounds.Height ?? 0d;
            var lastRenderdPlankton = 0;
            var lastRenderedBubbleElements = 0;
            var mousePosition = Mouse.GetPosition(area);
            var useSeaBed = control.SeaBedGeometry != null && control.UseSeaBed;

            UpdateCurrent(control, random);
            UpdateBubbles(control, area, random, mouseVector, useSeaBed, seaBedHeight, mousePosition, out var currentBubbleElements, out var forceBubbleRerender, out var lastMainBubbleState, out var bubbleElementRectangle);
            UpdatePlankton(control, area, random, useSeaBed, seaBedHeight, currentBubbleElements, bubbleElementRectangle, out var mainBubbleCollisions);
            UpdatePreview(control, area, mousePosition);

            if (control.Elements != lastRenderdPlankton)
            {
                var planktonHostVisual = new DrawingVisual();
                control.ElementHost.SpecifyPlanktonDrawingVisual(planktonHostVisual);
                control.ElementHost.AddPlanktonElements(control.Plankton.ToArray());
            }

            // if forcing bubble re-render, new bubbles added, or got down to no rendered bubbles but there were before - we don't want to re render everything when a bubble goes off screen
            if (forceBubbleRerender || currentBubbleElements == 0 && lastRenderedBubbleElements > 0)
            {
                var bubbleHostVisual = new DrawingVisual();
                control.ElementHost.SpecifyBubbleDrawingVisual(bubbleHostVisual);
                control.ElementHost.AddBubbleElements(control.ChildBubbles.Keys.ToArray());
            }

            if (control.Bubble != null)
            {
                lock (control.Bubble)
                {
                    if (!area.HasBubbleHostVisual || control.Bubble != null && !lastMainBubbleState)
                    {
                        var mainBubbleHostVisual = new DrawingVisual();
                        control.ElementHost.SpecifyMainBubbleDrawingVisual(mainBubbleHostVisual);
                        control.ElementHost.AddMainBubbleElement(control.Bubble);
                    }
                }
            }
            else if (lastMainBubbleState)
            {
                // need to remove the main bubble visual to not leave a hanging element
                var mainBubbleHostVisual = new DrawingVisual();
                control.ElementHost.SpecifyMainBubbleDrawingVisual(mainBubbleHostVisual);
            }

            control.ActiveChildBubbles = currentBubbleElements;
            control.MainBubbleCollisionsThisUpdate = mainBubbleCollisions;
            control.IsUpdating = false;
        }
    }
}
