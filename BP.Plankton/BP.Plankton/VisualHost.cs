using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Plankton
{
    /// <summary>
    /// Represents a visual host for rendering visuals
    /// </summary>
    public class VisualHost : FrameworkElement
    {
        #region Properties

        /// <summary>
        /// Get or set the visual collection
        /// </summary>
        public VisualCollection visualCollection;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the VisualHost class
        /// </summary>
        public VisualHost()
        {
            // set collection
            this.visualCollection = new VisualCollection(this);
        }

        /// <summary>
        /// Clear all visuals
        /// </summary>
        public void ClearVisuals()
        {
            // clear
            this.visualCollection.Clear();
        }

        /// <summary>
        /// Draw a collection of geometries on this visual host
        /// </summary>
        /// <param name="geometries">The collection of geometries to draw</param>
        /// <param name="fill">The fill to apply to the geometries</param>
        /// <param name="pen">The pen to apply to the geometries</param>
        public void DrawGeometries(Geometry[] geometries, Brush fill, Pen pen)
        {
            // create a new visual
            DrawingVisual drawingVisual = new DrawingVisual();

            // get the context from the visual
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            // itterate geometries
            foreach (Geometry g in geometries)
            {
                // draw the ellipse
                drawingContext.DrawGeometry(fill, pen, g);
            }

            // close the context
            drawingContext.Close();

            // add visual
            this.visualCollection.Add(drawingVisual);
        }

        protected override Visual GetVisualChild(int index)
        {
            // check range
            if ((index < 0) || (index >= this.visualCollection.Count))
            {
                // error
                throw new ArgumentOutOfRangeException();
            }

            // return visual
            return this.visualCollection[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return this.visualCollection.Count;
            }
        }

        #endregion
    }
}
