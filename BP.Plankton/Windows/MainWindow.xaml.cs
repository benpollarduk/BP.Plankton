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

        private Point preFullScreenTopLeft;
        private WindowState preFullScreenWindowState;
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
        public static readonly DependencyProperty StartupFileProperty = DependencyProperty.Register("StartupFile", typeof(PlanktonSettingsFile), typeof(MainWindow), new PropertyMetadata(PlanktonSettingsFile.LuminousRandomStartup));

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            ApplyApplicationSettings(Settings.Default);
        }

        #endregion

        #region Methods

        private void ApplyApplicationSettings(Settings settings)
        {
            try
            {
                PlanktonControl.AutoPanSensitivity = settings.AutoPanSensitivity;
                PlanktonControl.AutoPanSpeed = settings.AutoPanSpeed;
                PlanktonControl.IfMainBubbleNotAvailablePreviewMostInterestingElement = settings.IfMainBubbleNotAvailablePreviewMostInterestingElement;
                PlanktonControl.MaintainStandardPhysicsWhenRandomGeneratingSettings = settings.MaintainStandardPhysicsWhenRandomGeneratingSettings;
                PlanktonControl.MaximumZoomPreviewBlur = settings.MaximumZoomPreviewBlur;
                PlanktonControl.MaximumZoom = settings.MaximumZoom;
                PlanktonControl.MinimumZoom = settings.MinimumZoom;
                PlanktonControl.RefreshTime = settings.RefreshTime;
                PlanktonControl.ShowCurrentIndicator = settings.ShowCurrentIndicator;
                PlanktonControl.UseAnimation = settings.UseAnimation;
                PlanktonControl.UseAutoPanOnZoomPreview = settings.UseAutoPanOnZoomPreview;
                PlanktonControl.UseEfficientValuesWhenRandomGenerating = settings.UseEfficientValuesWhenRandomGenerating;
                PlanktonControl.UseOnlyStandardBrushesWhenRandomGenerating = settings.UseOnlyStandardBrushesWhenRandomGenerating;
                PlanktonControl.UseSmoothMousePositionUpdating = settings.UseSmoothMousePositionUpdating;
                PlanktonControl.UseZoomPreview = settings.UseZoomPreview;
                PlanktonControl.UseZoomPreviewBlurEffect = settings.UseZoomPreviewBlurEffect;
                PlanktonControl.ZoomPreviewBlurCorrection = settings.ZoomPreviewBlurCorrection;
                PlanktonControl.ZoomPreviewBlurStrength = settings.ZoomPreviewBlurStrength;
                PlanktonControl.ZoomPreviewLocaterMode = (ZoomPreviewLocaterMode)Enum.Parse(typeof(ZoomPreviewLocaterMode), settings.ZoomPreviewLocaterMode);
                PlanktonControl.ZoomPreviewSize = settings.ZoomPreviewSize;
            }
            catch (Exception e)
            {
                MessageBox.Show($"There was a problem with the settings file, some settings may not be loaded\n\\Error: {e.Message}", "Setting Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSettingsFile(ref Settings settings)
        {
            settings.AutoPanSensitivity = PlanktonControl.AutoPanSensitivity;
            settings.AutoPanSpeed = PlanktonControl.AutoPanSpeed;
            settings.IfMainBubbleNotAvailablePreviewMostInterestingElement = PlanktonControl.IfMainBubbleNotAvailablePreviewMostInterestingElement;
            settings.MaintainStandardPhysicsWhenRandomGeneratingSettings = PlanktonControl.MaintainStandardPhysicsWhenRandomGeneratingSettings;
            settings.MaximumZoomPreviewBlur = PlanktonControl.MaximumZoomPreviewBlur;
            settings.MaximumZoom = PlanktonControl.MaximumZoom;
            settings.MinimumZoom = PlanktonControl.MinimumZoom;
            settings.RefreshTime = PlanktonControl.RefreshTime;
            settings.ShowCurrentIndicator = PlanktonControl.ShowCurrentIndicator;
            settings.UseAnimation = PlanktonControl.UseAnimation;
            settings.UseAutoPanOnZoomPreview = PlanktonControl.UseAutoPanOnZoomPreview;
            settings.UseEfficientValuesWhenRandomGenerating = PlanktonControl.UseEfficientValuesWhenRandomGenerating;
            settings.UseOnlyStandardBrushesWhenRandomGenerating = PlanktonControl.UseOnlyStandardBrushesWhenRandomGenerating;
            settings.UseSmoothMousePositionUpdating = PlanktonControl.UseSmoothMousePositionUpdating;
            settings.UseZoomPreview = PlanktonControl.UseZoomPreview;
            settings.ZoomPreviewBlurCorrection = PlanktonControl.ZoomPreviewBlurCorrection;
            settings.ZoomPreviewBlurStrength = PlanktonControl.ZoomPreviewBlurStrength;
            settings.UseZoomPreviewBlurEffect = PlanktonControl.UseZoomPreviewBlurEffect;
            settings.ZoomPreviewLocaterMode = PlanktonControl.ZoomPreviewLocaterMode.ToString();
            settings.ZoomPreviewSize = PlanktonControl.ZoomPreviewSize;
        }

        private void StartPlanktonControl()
        {
            PlanktonControl.ApplySettingsFromFile(StartupFile);
            PlanktonControl.IsPaused = false;
        }

        #endregion

        #region EventHandlers

        private void MainWindow_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            FocusManager.SetIsFocusScope(PlanktonControl, true);
            PlanktonControl.Focus();
        }

        private void FadeIntoStoryboard_Completed(object sender, EventArgs e)
        {
            MainLayoutGrid.Children.Remove(SplashBorder);
        }

        private void MainTitleStoryboard_Completed(object sender, EventArgs e)
        {
            if (PlanktonControl.IsPaused)
                StartPlanktonControl();
        }

        private void SplashBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartPlanktonControl();
            SplashBorder.Visibility = Visibility.Collapsed;
        }

        private void PlanktonControl_ExitFullScreenMode(object sender, EventArgs e)
        {
            WindowStyle = WindowStyle.ThreeDBorderWindow;
            WindowState = preFullScreenWindowState;

            if (preFullScreenWindowState != WindowState.Normal)
                return;

            Top = preFullScreenTopLeft.Y;
            Left = preFullScreenTopLeft.X;
        }

        private void PlanktonControl_EnterFullScreenMode(object sender, EventArgs e)
        {
            WindowStyle = WindowStyle.None;

            if (Math.Abs(Left) > 0.0 || Math.Abs(Top) > 0.0)
                preFullScreenTopLeft = new Point(Left, Top);

            preFullScreenWindowState = WindowState;
            Top = 0;
            Left = 0;
            WindowState = WindowState.Maximized;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var newLocation = new Point(Left, Top);

            if (!PlanktonControl.IsPaused)
            {
                var vector = new Vector(lastLocation.X - newLocation.X, lastLocation.Y - newLocation.Y);
                var strength = Math.Abs(vector.Length);

                // if no current, or the new vector has yielded a stronger current
                if ((PlanktonControl.ActiveCurrent == null) || (strength > Math.Abs(PlanktonControl.ActiveCurrent.ActiveStep().Length)))
                {
                    // determine degrees - add 90 as Atan2 returns north as -90, east a 0, south as 90 and west as 180
                    var degrees = 90 + Math.Atan2(vector.Y, vector.X) * (180d / Math.PI);

                    var c = new Current(strength, degrees, CurrentSwellStage.MainUp)
                    {
                        Deceleration = PlanktonControl.IgnoreWaterViscosityWhenGeneratingCurrent ? PlanktonControl.CurrentDeceleration : PlanktonControl.WaterViscosity,
                        Acceleration = PlanktonControl.IgnoreWaterViscosityWhenGeneratingCurrent ? PlanktonControl.CurrentAcceleration : PlanktonControl.WaterViscosity
                    };

                    PlanktonControl.TriggerCurrent(c);
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
            UpdateSettingsFile(ref settings);
            settings.Save();
        }

        #endregion
    }
}