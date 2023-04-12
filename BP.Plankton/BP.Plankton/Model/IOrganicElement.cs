using System.Windows;
using System.Windows.Media;

namespace BP.Plankton.Model
{
    /// <summary>
    /// Represents any organic element.
    /// </summary>
    public interface IOrganicElement
    {
        /// <summary>
        /// Get or set this elements vector.
        /// </summary>
        Vector Vector { get; set; }
        /// <summary>
        /// Get or set this elements geometry.
        /// </summary>
        EllipseGeometry Geometry { get; set; }
        /// <summary>
        /// Get or set this elements stroke.
        /// </summary>
        Pen Stroke { get; set; }
        /// <summary>
        /// Get or set this elements fill.
        /// </summary>
        Brush Fill { get; set; }
    }
}
