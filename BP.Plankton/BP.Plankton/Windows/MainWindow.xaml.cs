using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BP.Plankton.Model;
using BP.Plankton.Model.Currents;
using BP.Plankton.Model.Settings;
using BP.Plankton.Properties;

namespace BP.Plankton.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        /// <summary>
        /// Get or set the pre full screen top left coordinates.
        /// </summary>
        private Point preFullScreenTopLeft;

        /// <summary>
        /// Get or set the pre full screen window state.
        /// </summary>
        private WindowState preFullScreenWindowState;

        /// <summary>
        /// Get or set the last location of this window.
        /// </summary>
        private Point lastLocation = new Point(0, 0);

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the startup settings file. This is a dependency property.
        /// </summary>
        public PlanktonSettingsFile StartupFile
        {
            get { return (PlanktonSettingsFile)GetValue(StartupFileProperty); }
            set { SetValue(StartupFileProperty, value); }
        }

        #endregion

        #region DependencyProperties

        /// <summary>
        /// Identifies the MainWindow.StartupFile property.
        /// </summary>
        public static readonly DependencyProperty StartupFileProperty = DependencyProperty.Register("StartupFile", typeof (PlanktonSettingsFile), typeof (MainWindow), new PropertyMetadata(PlanktonSettingsFile.LuminousRandomStartup));

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            applyApplicationSettings(Settings.Default);
        }

        /// <summary>
        /// Apply a settings file to this window.
        /// </summary>
        /// <param name="settings">Specify the settings to apply.</param>
        private void applyApplicationSettings(Settings settings)
        {
            try
            {
                planktonControl.AutoPanSensitivity = settings.AutoPanSensitivity;
                planktonControl.AutoPanSpeed = settings.AutoPanSpeed;
                planktonControl.IfMainBubbleNotAvailablePreviewMostInterestingElement = settings.IfMainBubbleNotAvailablePreviewMostInterestingElement;
                planktonControl.MaintainStandardPhysicsWhenRandomGeneratingSettings = settings.MaintainStandardPhysicsWhenRandomGeneratingSettings;
                planktonControl.MaximumZoomPreviewBlur = settings.MaximumZoomPreviewBlur;
                planktonControl.MaximumZoom = settings.MaximumZoom;
                planktonControl.MinimumZoom = settings.MinimumZoom;
                planktonControl.RefreshTime = settings.RefreshTime;
                planktonControl.ShowCurrentIndicator = settings.ShowCurrentIndicator;
                planktonControl.UseAnimation = settings.UseAnimation;
                planktonControl.UseAutoPanOnZoomPreview = settings.UseAutoPanOnZoomPreview;
                planktonControl.UseEfficientValuesWhenRandomGenerating = settings.UseEfficientValuesWhenRandomGenerating;
                planktonControl.UseOnlyStandardBrushesWhenRandomGenerating = settings.UseOnlyStandardBrushesWhenRandomGenerating;
                planktonControl.UseSmoothMousePositionUpdating = settings.UseSmoothMousePositionUpdating;
                planktonControl.UseZoomPreview = settings.UseZoomPreview;
                planktonControl.UseZoomPreviewBlurEffect = settings.UseZoomPreviewBlurEffect;
                planktonControl.ZoomPreviewBlurCorrection = settings.ZoomPreviewBlurCorrection;
                planktonControl.ZoomPreviewBlurStrength = settings.ZoomPreviewBlurStrength;
                planktonControl.ZoomPreviewLocatorMode = (ZoomPreviewLocaterMode)Enum.Parse(typeof (ZoomPreviewLocaterMode), settings.ZoomPreviewLocatorMode);
                planktonControl.ZoomPreviewSize = settings.ZoomPreviewSize;
            }
            catch (Exception e)
            {
                // show
                MessageBox.Show($"There was a problem with the settings file, some settings may not be loaded\n\\Error: {e.Message}", "Setting Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Update a settings file to comply with this window.
        /// </summary>
        /// <param name="settings">Specify the settings to update.</param>
        private void updateSettingsFile(ref Settings settings)
        {
            settings.AutoPanSensitivity = planktonControl.AutoPanSensitivity;
            settings.AutoPanSpeed = planktonControl.AutoPanSpeed;
            settings.IfMainBubbleNotAvailablePreviewMostInterestingElement = planktonControl.IfMainBubbleNotAvailablePreviewMostInterestingElement;
            settings.MaintainStandardPhysicsWhenRandomGeneratingSettings = planktonControl.MaintainStandardPhysicsWhenRandomGeneratingSettings;
            settings.MaximumZoomPreviewBlur = planktonControl.MaximumZoomPreviewBlur;
            settings.MaximumZoom = planktonControl.MaximumZoom;
            settings.MinimumZoom = planktonControl.MinimumZoom;
            settings.RefreshTime = planktonControl.RefreshTime;
            settings.ShowCurrentIndicator = planktonControl.ShowCurrentIndicator;
            settings.UseAnimation = planktonControl.UseAnimation;
            settings.UseAutoPanOnZoomPreview = planktonControl.UseAutoPanOnZoomPreview;
            settings.UseEfficientValuesWhenRandomGenerating = planktonControl.UseEfficientValuesWhenRandomGenerating;
            settings.UseOnlyStandardBrushesWhenRandomGenerating = planktonControl.UseOnlyStandardBrushesWhenRandomGenerating;
            settings.UseSmoothMousePositionUpdating = planktonControl.UseSmoothMousePositionUpdating;
            settings.UseZoomPreview = planktonControl.UseZoomPreview;
            settings.ZoomPreviewBlurCorrection = planktonControl.ZoomPreviewBlurCorrection;
            settings.ZoomPreviewBlurStrength = planktonControl.ZoomPreviewBlurStrength;
            settings.UseZoomPreviewBlurEffect = planktonControl.UseZoomPreviewBlurEffect;
            settings.ZoomPreviewLocatorMode = planktonControl.ZoomPreviewLocatorMode.ToString();
            settings.ZoomPreviewSize = planktonControl.ZoomPreviewSize;
        }

        /// <summary>
        /// Start the plankton control running.
        /// </summary>
        private void startPlanktonControl()
        {
            planktonControl.ApplySettingsFromFile(StartupFile);
            planktonControl.IsPaused = false;
        }

        #endregion

        #region EventHandlers

        private void MainWindow_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            FocusManager.SetIsFocusScope(planktonControl, true);
            planktonControl.Focus();
        }

        private void fadeIntoStoryboard_Completed(object sender, EventArgs e)
        {
            // remove the intro from the main grid
            layoutGrid.Children.Remove(splashBorder);
        }

        private void mainTitleStoryboard_Completed(object sender, EventArgs e)
        {
            if (planktonControl.IsPaused)
                startPlanktonControl();
        }

        private void splashBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPlanktonControl();
            splashBorder.Visibility = Visibility.Collapsed;
        }

        private void planktonControl_ExitFullScreenMode(object sender, EventArgs e)
        {
            WindowStyle = WindowStyle.ThreeDBorderWindow;
            WindowState = preFullScreenWindowState;

            if (preFullScreenWindowState != WindowState.Normal)
                return;

            Top = preFullScreenTopLeft.Y;
            Left = preFullScreenTopLeft.X;
        }

        private void planktonControl_EnterFullScreenMode(object sender, EventArgs e)
        {
            WindowStyle = WindowStyle.None;

            if ((Math.Abs(Left) > 0.0) || (Math.Abs(Top) > 0.0))
                preFullScreenTopLeft = new Point(Left, Top);

            preFullScreenWindowState = WindowState;
            Top = 0;
            Left = 0;
            WindowState = WindowState.Maximized;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var newLocation = new Point(Left, Top);

            if (!planktonControl.IsPaused)
            {
                var vector = new Vector(lastLocation.X - newLocation.X, lastLocation.Y - newLocation.Y);
                var strength = Math.Abs(vector.Length);

                // if no current, or the new vector has yielded a stronger current
                if ((planktonControl.ActiveCurrent == null) || (strength > Math.Abs(planktonControl.ActiveCurrent.ActiveStep().Length)))
                {
                    // determine degrees - add 90 as Atan2 returns north as -90, east a 0, south as 90 and west as 180
                    var degrees = 90 + Math.Atan2(vector.Y, vector.X) * (180d / Math.PI);

                    var c = new Current(strength, degrees, CurrentSwellStage.MainUp)
                    {
                        Deceleration = planktonControl.IgnoreWaterViscosityWhenGeneratingCurrent ? planktonControl.CurrentDeceleration : planktonControl.WaterViscosity,
                        Acceleration = planktonControl.IgnoreWaterViscosityWhenGeneratingCurrent ? planktonControl.CurrentAcceleration : planktonControl.WaterViscosity
                    };

                    planktonControl.TriggerCurrent(c);
                }
            }

            lastLocation = newLocation;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lastLocation = new Point(Left, Top);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var settings = Settings.Default;
            updateSettingsFile(ref settings);
            settings.Save();
        }

        #endregion
    }
}