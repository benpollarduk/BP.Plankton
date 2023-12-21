using System.Windows;
using System.Windows.Media;

namespace BP.Plankton.Model
{
    /// <summary>
    /// Represents a bubble.
    /// </summary>
    public class Bubble : IOrganicElement
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Bubble class.
        /// <param name="center">The center point of this element.</param>
        /// <param name="radius">The radius of the element.</param>
        /// <param name="vector">The vector of the element.</param>
        /// <param name="stroke">The pen to use for this elements stroke.</param>
        /// <param name="fill">The brush to use for this elements fill.</param>
        /// </summary>
        public Bubble(Point center, double radius, Vector vector, Pen stroke, Brush fill)
        {
            Geometry = new EllipseGeometry(center, radius, radius);
            Stroke = stroke;
            Fill = fill;
            Vector = vector;
        }

        #endregion

        #region Implementation of IOrganicElement

        /// <summary>
        /// Get or set this elements vector.
        /// </summary>
        public Vector Vector { get; set; }

        /// <summary>
        /// Get or set this elements geometry.
        /// </summary>
        public EllipseGeometry Geometry { get; set; }

        /// <summary>
        /// Get or set this elements stroke.
        /// </summary>
        public Pen Stroke { get; set; }

        /// <summary>
        /// Get or set this elements fill.
        /// </summary>
        public Brush Fill { get; set; }

        #endregion
    }
}