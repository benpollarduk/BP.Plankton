using System.Windows;
using System.Windows.Media;

namespace BP.Plankton.Model.Rendering
{
    /// <summary>
    /// Represents a moveable element.
    /// </summary>
    public class MoveableElement
    {
        #region Properties

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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MoveableElement class.
        /// </summary>
        protected MoveableElement()
        {
        }

        #endregion

        #region StaticMethods

        /// <summary>
        /// Create a new MoveableElement.
        /// </summary>
        /// <param name="center">The center point of this element.</param>
        /// <param name="radius">The radius of the element.</param>
        /// <param name="vector">The vector of the element.</param>
        /// <param name="stroke">The pen to use for this elements stroke.</param>
        /// <param name="fill">The brush to use for this elements fill.</param>
        /// <returns>The created MoveableElement.</returns>
        public static MoveableElement Create(Point center, double radius, Vector vector, Pen stroke, Brush fill)
        {
            return new MoveableElement
            {
                Geometry = new EllipseGeometry(center, radius, radius),
                Stroke = stroke,
                Fill = fill,
                Vector = vector
            };
        }

        #endregion
    }
}