﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BP.Plankton.Controls
{
    /// <summary>
    /// Interaction logic for DrawSeaBedControl.xaml
    /// </summary>
    public partial class DrawSeaBedControl : UserControl
    {
        #region Fields

        private readonly List<LineSegment> segments = new List<LineSegment>();
        private Point startPoint;
        private ushort frameSkipCount;

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the path geometry generated by this control. This is a dependency property.
        /// </summary>
        public PathGeometry Geometry
        {
            get { return (PathGeometry)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        /// <summary>
        /// Get or set background. This is a dependency property.
        /// </summary>
        public Brush SeaBackground
        {
            get { return (Brush)GetValue(SeaBackgroundProperty); }
            set { SetValue(SeaBackgroundProperty, value); }
        }

        /// <summary>
        /// Get or set the sea bed background. This is a dependency property.
        /// </summary>
        public Brush SeaBedBackground
        {
            get { return (Brush)GetValue(SeaBedBackgroundProperty); }
            set { SetValue(SeaBedBackgroundProperty, value); }
        }

        /// <summary>
        /// Get or set the sea bed stroke. This is a dependency property.
        /// </summary>
        public Brush SeaBedStroke
        {
            get { return (Brush)GetValue(SeaBedStrokeProperty); }
            set { SetValue(SeaBedStrokeProperty, value); }
        }

        /// <summary>
        /// Get or set the events to skip when capturing mouse input. This is a dependency property.
        /// </summary>
        public ushort CaptureSkip
        {
            get { return (ushort)GetValue(CaptureSkipProperty); }
            set { SetValue(CaptureSkipProperty, value); }
        }

        #endregion

        #region DependencyProperties

        /// <summary>
        /// Identifies the DrawSeaBedControl.Geometry property.
        /// </summary>
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register("Geometry", typeof (PathGeometry), typeof (DrawSeaBedControl), new PropertyMetadata(new PathGeometry()));

        /// <summary>
        /// Identifies the DrawSeaBedControl.SeaBackground property.
        /// </summary>
        public static readonly DependencyProperty SeaBackgroundProperty = DependencyProperty.Register("SeaBackground", typeof (Brush), typeof (DrawSeaBedControl), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// Identifies the DrawSeaBedControl.SeaBedBackground property.
        /// </summary>
        public static readonly DependencyProperty SeaBedBackgroundProperty = DependencyProperty.Register("SeaBedBackground", typeof (Brush), typeof (DrawSeaBedControl), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Identifies the DrawSeaBedControl.SeaBedStroke property.
        /// </summary>
        public static readonly DependencyProperty SeaBedStrokeProperty = DependencyProperty.Register("SeaBedStroke", typeof (Brush), typeof (DrawSeaBedControl), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// Identifies the DrawSeaBedControl.CaptureSkipProperty property.
        /// </summary>
        public static readonly DependencyProperty CaptureSkipProperty = DependencyProperty.Register("CaptureSkipProperty", typeof (ushort), typeof (DrawSeaBedControl), new PropertyMetadata((ushort)5));

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DrawSeaBedControl class.
        /// </summary>
        public DrawSeaBedControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear any drawn geometry.
        /// </summary>
        public void Clear()
        {
            segments.Clear();
            startPoint = new Point(0, 0);
            frameSkipCount = 0;
            Geometry = null;
        }

        /// <summary>
        /// Generate a scaled PathGeometry from this control.
        /// </summary>
        /// <param name="scale">Specify a scale factor to apply to the geometry.</param>
        /// <returns>A PathGeometry representing the geometry to drawn with this control.</returns>
        public PathGeometry GenerateScaledGeometry(double scale)
        {
            var geometry = new PathGeometry();
            var scalledSegments = new List<PathSegment>();
            var start = new Point(0d, segments[0].Point.Y / scale);

            for (var index = 1; index < segments.Count; index++)
                scalledSegments.Add(new LineSegment(new Point(segments[index].Point.X / scale, segments[index].Point.Y / scale), false));

            geometry.Figures.Add(new PathFigure(start, scalledSegments, true));
            geometry.FillRule = FillRule.Nonzero;
            return geometry;
        }

        /// <summary>
        /// Handle the mouse moving over the drawing canvas.
        /// </summary>
        /// <param name="mousePointOverDrawingCanvas">The current mouse point over the canvas.</param>
        /// <param name="observeCaptureSkip">Specify if capture skipping should be observed.</param>
        protected virtual void OnHandleMouseMove(Point mousePointOverDrawingCanvas, bool observeCaptureSkip)
        {
            if (observeCaptureSkip)
            {
                if (frameSkipCount == CaptureSkip)
                    frameSkipCount = 0;
                else
                {
                    frameSkipCount++;
                    return;
                }
            }

            LineSegment dummySegment = null;

            if (segments.Count == 0)
            {
                segments.Add(new LineSegment(new Point(0, DrawingCanvas.ActualHeight), false));
                startPoint = new Point(0, DrawingCanvas.ActualHeight);

                // if x is not 0 add line segment at start height
                if (mousePointOverDrawingCanvas.X > 0)
                    segments.Add(new LineSegment(new Point(0, mousePointOverDrawingCanvas.Y), false));
            }
            else if (segments.Count > 0)
            {
                dummySegment = segments[segments.Count - 1];
                segments.RemoveAt(segments.Count - 1);
            }

            if (dummySegment == null)
                dummySegment = new LineSegment(new Point(DrawingCanvas.ActualWidth, DrawingCanvas.ActualHeight), false);

            segments.Add(new LineSegment(mousePointOverDrawingCanvas, false));

            // add line segment to bottom right to prevent strange fill behaviour
            segments.Add(dummySegment);

            Geometry = new PathGeometry(new[] { new PathFigure(startPoint, segments, false) });
        }

        #endregion

        #region EventHandlers

        private void DrawingCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            OnHandleMouseMove(Mouse.GetPosition(DrawingCanvas), CaptureSkip > 0);
        }

        private void DrawingCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnHandleMouseMove(Mouse.GetPosition(DrawingCanvas), false);
        }

        #endregion
    }
}