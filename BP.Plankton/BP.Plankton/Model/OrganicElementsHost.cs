using System;
using System.Windows;
using System.Windows.Media;

namespace BP.Plankton.Model
{
    /// <summary>
    /// Provides a class for hosting IOrganicElements.
    /// </summary>
    public class OrganicElementsHost : FrameworkElement
    {
        #region Fields

        private DrawingVisual planktonDrawingVisual;
        private DrawingVisual bubbleDrawingVisual;
        private DrawingVisual mainBubbleDrawingVisual;
        private readonly VisualCollection children;
        private int planktonCount;
        private int bubbleCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount => children.Count;

        /// <summary>
        /// Get if there is a plankton host visual.
        /// </summary>
        public bool HasPlanktonHostVisual => planktonDrawingVisual != null;

        /// <summary>
        /// Get if there is a bubble host visual.
        /// </summary>
        public bool HasBubbleHostVisual => bubbleDrawingVisual != null;

        /// <summary>
        /// Get if there is a main bubble host visual.
        /// </summary>
        public bool HasMainBubbleHostVisual => mainBubbleDrawingVisual != null;

        /// <summary>
        /// Get the number of plankton.
        /// </summary>
        public int Plankton => HasPlanktonHostVisual ? planktonCount : 0;

        /// <summary>
        /// Get the number of bubbles.
        /// </summary>
        public int Bubbles => HasBubbleHostVisual ? bubbleCount : 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the OrganicElementsHost class.
        /// </summary>
        public OrganicElementsHost()
        {
            children = new VisualCollection(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Remove all drawing visuals from this control.
        /// </summary>
        public virtual void RemoveAllDrawingVisuals()
        {
            RemovePlanktonDrawingVisual();
            RemoveBubblesDrawingVisual();
            RemoveMainBubbleDrawingVisual();
        }

        /// <summary>
        /// Remove the plankton DrawingVisual from this control.
        /// </summary>
        public virtual void RemovePlanktonDrawingVisual()
        {
            if (planktonDrawingVisual == null) 
                return;

            Remove(planktonDrawingVisual);
            planktonDrawingVisual = null;
            planktonCount = 0;
        }

        /// <summary>
        /// Remove the bubbles DrawingVisual from this control.
        /// </summary>
        public virtual void RemoveBubblesDrawingVisual()
        {
            if (bubbleDrawingVisual == null)
                return;

            Remove(bubbleDrawingVisual);
            bubbleDrawingVisual = null;
            bubbleCount = 0;
        }

        /// <summary>
        /// Remove the main bubble DrawingVisual from this control.
        /// </summary>
        public virtual void RemoveMainBubbleDrawingVisual()
        {
            if (mainBubbleDrawingVisual == null)
                return;

            Remove(mainBubbleDrawingVisual);
            mainBubbleDrawingVisual = null;
        }

        /// <summary>
        /// Specify the plankton drawing visual to use as the render surface.
        /// </summary>
        /// <param name="visual">The visual to use as the render surface.</param>
        public virtual void SpecifyPlanktonDrawingVisual(DrawingVisual visual)
        {
            RemovePlanktonDrawingVisual();
            planktonDrawingVisual = visual;
            planktonCount = 0;
            Add(planktonDrawingVisual);
        }

        /// <summary>
        /// Specify the bubble drawing visual to use as the render surface.
        /// </summary>
        /// <param name="visual">The visual to use as the render surface.</param>
        public virtual void SpecifyBubbleDrawingVisual(DrawingVisual visual)
        {
            RemoveBubblesDrawingVisual();
            bubbleDrawingVisual = visual;
            bubbleCount = 0;
            Add(bubbleDrawingVisual);
        }

        /// <summary>
        /// Specify the main bubble drawing visual to use as the render surface.
        /// </summary>
        /// <param name="visual">The visual to use as the render surface.</param>
        public virtual void SpecifyMainBubbleDrawingVisual(DrawingVisual visual)
        {
            RemoveMainBubbleDrawingVisual();
            mainBubbleDrawingVisual = visual;
            Add(mainBubbleDrawingVisual);
        }

        /// <summary>
        /// Overrides System.Windows.Media.Visual.FrameworkElement.GetVisualChild(int index), and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>The visual at the specified index.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if ((index < 0) || (index >= children.Count))
                throw new ArgumentOutOfRangeException();

            return children[index];
        }

        /// <summary>
        /// Add a visual.
        /// </summary>
        /// <param name="visual">The visual to add.</param>
        protected virtual void Add(Visual visual)
        {
            children.Add(visual);
        }

        /// <summary>
        /// Remove a visual.
        /// </summary>
        /// <param name="visual">The visual to remove.</param>
        protected virtual void Remove(Visual visual)
        {
            if (children.Contains(visual))
                children.Remove(visual);
        }

        /// <summary>
        /// Add a collection of plankton to the plankton layer.
        /// </summary>
        /// <param name="elements">The elements to add.</param>
        public virtual void AddPlanktonElements(params Plankton[] elements)
        {
            using (var dC = planktonDrawingVisual.RenderOpen())
            {
                foreach (var g in elements)
                {
                    dC.DrawGeometry(g.Fill, g.Stroke, g.Geometry);
                    planktonCount++;
                }
            }
        }

        /// <summary>
        /// Add a collection of bubbles to the bubbles layer.
        /// </summary>
        /// <param name="elements">The elements to add.</param>
        public virtual void AddBubbleElements(params Bubble[] elements)
        {
            using (var dC = bubbleDrawingVisual.RenderOpen())
            {
                foreach (var g in elements)
                {
                    dC.DrawGeometry(g.Fill, g.Stroke, g.Geometry);
                    bubbleCount++;
                }
            }
        }

        /// <summary>
        /// Add the main bubble element to the main bubble layer.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public virtual void AddMainBubbleElement(Bubble element)
        {
            using (var dC = mainBubbleDrawingVisual.RenderOpen())
                dC.DrawGeometry(element.Fill, element.Stroke, element.Geometry);
        }

        #endregion
    }
}