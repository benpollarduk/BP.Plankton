using System.Windows;
using System.Windows.Input;
using BP.Plankton.Controls;

namespace BP.Plankton.Windows
{
    /// <summary>
    /// Interaction logic for DrawSeaBedWindow.xaml
    /// </summary>
    public partial class DrawSeaBedWindow : Window
    {
        #region Properties

        /// <summary>
        /// Get the drawing control. This is a dependency property.
        /// </summary>
        public DrawSeaBedControl DrawingControl
        {
            get { return (DrawSeaBedControl)GetValue(DrawingControlProperty); }
            protected set { SetValue(DrawingControlProperty, value); }
        }

        /// <summary>
        /// Occurs when a geometry is accepted.
        /// </summary>
        public event RoutedEventHandler GeomertyAccepted;

        #endregion

        #region DependencyProperties

        /// <summary>
        /// Identifies the DrawSeaBedWindow.DrawingControl property.
        /// </summary>
        public static readonly DependencyProperty DrawingControlProperty = DependencyProperty.Register("DrawingControl", typeof (DrawSeaBedControl), typeof (DrawSeaBedWindow), new PropertyMetadata(null));

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DrawSeaBedWindow class.
        /// </summary>
        public DrawSeaBedWindow()
        {
            InitializeComponent();

            DrawingControl = SeaBedControl;
        }

        /// <summary>
        /// Initializes a new instance of the DrawSeaBedWindow class.
        /// </summary>
        /// <param name="drawingAreaSize">Specif the size of the drawing area.</param>
        public DrawSeaBedWindow(Size drawingAreaSize)
        {
            InitializeComponent();

            DrawingControl = SeaBedControl;
            DrawingControl.Width = drawingAreaSize.Width;
            DrawingControl.Height = drawingAreaSize.Height;
        }

        #endregion

        #region CommandBindingCallbacks

        private void ClearCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingControl.Clear();
        }

        private void OKCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GeomertyAccepted?.Invoke(sender, new RoutedEventArgs());
            Close();
        }

        private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}