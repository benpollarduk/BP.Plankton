using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BP.Plankton.Model.Rendering
{
    /// <summary>
    /// Provides a class for hosting MoveableElements.
    /// </summary>
    public class MoveableElementsHost : FrameworkElement
    {
        #region Fields

        private DrawingVisual planktonDrawingVisual;
        private DrawingVisual bubbleDrawingVisual;
        private DrawingVisual mainBubbleDrawingVisual;
        private readonly List<DrawingVisual> extendedDrawingVisuals = new List<DrawingVisual>();
        private readonly VisualCollection children;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount => children.Count;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MoveableElementsHost class.
        /// </summary>
        public MoveableElementsHost()
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
            RemoveAllExtendedVisuals();
        }

        /// <summary>
        /// Remove the plankton DrawingVisual from this control.
        /// </summary>
        public virtual void RemovePlanktonDrawingVisual()
        {
            if (planktonDrawingVisual != null)
                Remove(planktonDrawingVisual);
        }

        /// <summary>
        /// Remove the bubbles DrawingVisual from this control.
        /// </summary>
        public virtual void RemoveBubblesDrawingVisual()
        {
            if (bubbleDrawingVisual != null)
                Remove(bubbleDrawingVisual);
        }

        /// <summary>
        /// Remove the main bubble DrawingVisual from this control.
        /// </summary>
        public virtual void RemoveMainBubbleDrawingVisual()
        {
            if (mainBubbleDrawingVisual != null)
                Remove(mainBubbleDrawingVisual);
        }

        /// <summary>
        /// Specify the plankton drawing visual to use as the render surface.
        /// </summary>
        /// <param name="visual">The visual to use as the render surface.</param>
        public virtual void SpecifyPlanktonDrawingVisual(DrawingVisual visual)
        {
            RemovePlanktonDrawingVisual();
            planktonDrawingVisual = visual;
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
        /// Add an additional DrawwingVisual layer.
        /// </summary>
        /// <param name="visual">The visual to add.</param>
        public virtual void AddExtendedVisual(DrawingVisual visual)
        {
            extendedDrawingVisuals.Add(visual);
            Add(visual);
        }

        /// <summary>
        /// Remove all of the extended DrawingVisual layers.
        /// </summary>
        public virtual void RemoveAllExtendedVisuals()
        {
            foreach (var dV in extendedDrawingVisuals)
                RemoveExtendedVisual(dV);

            extendedDrawingVisuals.Clear();
        }

        /// <summary>
        /// Remove one of the extended DrawingVisual layers.
        /// </summary>
        /// <param name="visual">The visual to remove.</param>
        public virtual void RemoveExtendedVisual(DrawingVisual visual)
        {
            if (!extendedDrawingVisuals.Contains(visual))
                throw new InvalidOperationException("The specified visual is not included in this hosts visual children.");

            extendedDrawingVisuals.Remove(visual);
            Remove(visual);
        }

        /// <summary>
        /// Get an array containing all the extended DraingVisual layers.
        /// </summary>
        /// <returns>The extended visuals in an array.</returns>
        public DrawingVisual[] GetExtendedVisuals()
        {
            return extendedDrawingVisuals.ToArray<DrawingVisual>();
        }

        /// <summary>
        /// Get an extended DraingVisual layer at a specified index.
        /// </summary>
        /// <param name="index">The index of the visual.</param>
        /// <returns>A DrawingVisual layer at the specified index.</returns>
        public DrawingVisual GetExtendedVisual(int index)
        {
            if ((index < 0) || (index >= extendedDrawingVisuals.Count))
                throw new ArgumentOutOfRangeException();
            
            return extendedDrawingVisuals.ElementAt(index);
        }

        /// <summary>
        /// Determine if this contains a visual.
        /// </summary>
        /// <param name="visual">The visual to locate.</param>
        /// <returns>True if the visual could be located, else false.</returns>
        public virtual bool ContainsVisual(Visual visual)
        {
            return children.Contains(visual);
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
        /// Add a collection of MoveableElements to the plankton layer.
        /// </summary>
        /// <param name="elements">The elements to add.</param>
        public virtual void AddPlanktonElements(params MoveableElement[] elements)
        {
            using (var dC = planktonDrawingVisual.RenderOpen())
            {
                foreach (var g in elements)
                    dC.DrawGeometry(g.Fill, g.Stroke, g.Geometry);
            }
        }

        /// <summary>
        /// Add a collection of MoveableElements to the bubbles layer.
        /// </summary>
        /// <param name="elements">The elements to add.</param>
        public virtual void AddBubbleElements(params MoveableElement[] elements)
        {
            using (var dC = bubbleDrawingVisual.RenderOpen())
            {
                foreach (var g in elements)
                    dC.DrawGeometry(g.Fill, g.Stroke, g.Geometry);
            }
        }

        /// <summary>
        /// Add the main bubble element to the main bubble layer.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public virtual void AddMainBubbleElement(MoveableElement element)
        {
            using (var dC = mainBubbleDrawingVisual.RenderOpen())
                dC.DrawGeometry(element.Fill, element.Stroke, element.Geometry);
        }

        /// <summary>
        /// Add an element to one of the extended visual layers.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="stroke">The pen to use to render the elements stroke.</param>
        /// <param name="fill">The brush to use to render the elements fill.</param>
        /// <param name="visual">The visual to add the element to.</param>
        public virtual void AddElementToExtendedVisualLayer(Geometry element, Pen stroke, Brush fill, DrawingVisual visual)
        {
            using (var dC = visual.RenderOpen())
                dC.DrawGeometry(fill, stroke, element);
        }

        #endregion
    }
}