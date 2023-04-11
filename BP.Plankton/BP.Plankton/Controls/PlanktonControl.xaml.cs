using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using BP.Plankton.Converters;
using BP.Plankton.Model;
using BP.Plankton.Model.Currents;
using BP.Plankton.Model.Interop;
using BP.Plankton.Model.Settings;
using BP.Plankton.Windows;
using Microsoft.Win32;

namespace BP.Plankton.Controls
{
    /// <summary>
    /// Interaction logic for PlanktonControl.xaml
    /// </summary>
    public partial class PlanktonControl : UserControl
    {
        #region StaticProperties

        /// <summary>
        /// Get the generator used for seeding random numbers.
        /// </summary>
        private static readonly Random randomGenerator = new Random();

        #endregion

        #region Fields

        /// <summary>
        /// Get or set the timer used for positional updates.
        /// </summary>
        private DispatcherTimer update;

        /// <summary>
        /// Get or set the previous mouse point.
        /// </summary>
        private Point previousMousePoint = new Point(0, 0);

        /// <summary>
        /// Get or set the current mouse point.
        /// </summary>
        private Point currentMousePoint = new Point(0, 0);

        /// <summary>
        /// Get or set the mouse vector.
        /// </summary>
        private Vector mouseVector;

        /// <summary>
        /// Get or set a dictionary contianing the child bubbles and a boolean value set as true representing if the bubble is valid, or false if it is waiting to be removed.
        /// </summary>
        private readonly Dictionary<MoveableElement, bool> childBubbles = new Dictionary<MoveableElement, bool>();

        /// <summary>
        /// Get or set the zoom preview factor.
        /// </summary>
        private double zoomPreviewFactor;

        /// <summary>
        /// Get or set the vector used for controlling the zoom preview.
        /// </summary>
        private Vector3D zoomPreviewVector = new Vector3D(0, 0, 0);

        /// <summary>
        /// Get or set the bubble collision history.
        /// </summary>
        private readonly Queue<int> bubbleCollisionHistory = new Queue<int>(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        /// <summary>
        /// Get or set the pen to use for geometry detection.
        /// </summary>
        private Pen seaBedPen;

        /// <summary>
        /// Get or set the sea bed geometry.
        /// </summary>
        private PathGeometry seaBedGeometry;

        /// <summary>
        /// Get or set if this user control is handling mouse position updates.
        /// </summary>
        private bool isHandlingMousePositionUpdate;

        /// <summary>
        /// Get or set if a resize process has been requested.
        /// </summary>
        private bool hasResizeProcessBeenRequested;

        /// <summary>
        /// Get or set the newest size render request.
        /// </summary>
        private Size newestSizeRenderRequest;

        /// <summary>
        /// Get or set the main bubble.
        /// </summary>
        private MoveableElement bubble;

        /// <summary>
        /// Get or set the pen used to draw bubble strokes.
        /// </summary>
        private readonly Pen bubblePen;

        /// <summary>
        /// Get or set the plankton focuses on by the zoom preview.
        /// </summary>
        private MoveableElement zoomPreviewFocusedPlankton;

        /// <summary>
        /// Occurs when full screen mode should be entered.
        /// </summary>
        public event FullScreenModeChangedEventHandler EnterFullScreenMode;

        /// <summary>
        /// Occurs when full screen mode should be exited.
        /// </summary>
        public event FullScreenModeChangedEventHandler ExitFullScreenMode;

        /// <summary>
        /// Get or set current z adjustment.
        /// </summary>
        private double currentZAdjustemnt;

        /// <summary>
        /// Get or set the last generated plankton brushes brush.
        /// </summary>
        private Brush[] lastGeneratedPlanktonBrushes;

        /// <summary>
        /// Get or set the last generated sea bed brush.
        /// </summary>
        private Brush lastGeneratedSeaBedBrush;

        /// <summary>
        /// Get or set the last generated background brush.
        /// </summary>
        private Brush lastGeneratedBackgroundBrush;

        /// <summary>
        /// Get or set if plankton are regenerated on colour changed.
        /// </summary>
        private bool preventRegenerationOfPlanktonOnColourChange = true;

        /// <summary>
        /// Get or set the next zoom preview blur radius.
        /// </summary>
        private double nextZoomPreviewBlurRadius;

        /// <summary>
        /// Get or set the location of the last visual element.
        /// </summary>
        private Point lastZoomPreviewVisualLocation = new Point(0, 0);

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the number of plankton elements that should be used. This is a dependency property.
        /// </summary>
        public int Elements
        {
            get { return (int)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }

        /// <summary>
        /// Get or set the size of the plankton elements represented as equal with and height. This is a dependency property.
        /// </summary>
        public double ElementsSize
        {
            get { return (double)GetValue(ElementsSizeProperty); }
            set { SetValue(ElementsSizeProperty, value); }
        }

        /// <summary>
        /// Get or set the plankton elements size variation, represented as a percentage. This is a dependency property.
        /// </summary>
        public double ElementsSizeVariation
        {
            get { return (double)GetValue(ElementsSizeVariationProperty); }
            set { SetValue(ElementsSizeVariationProperty, value); }
        }

        /// <summary>
        /// Get or set the amount of travel each plankton element should make along its vector on each update. This is a dependency property.
        /// </summary>
        public double Travel
        {
            get { return (double)GetValue(TravelProperty); }
            set { SetValue(TravelProperty, value); }
        }

        /// <summary>
        /// Get if an update is currently in progress. This is a dependency property.
        /// </summary>
        public bool IsUpdating
        {
            get { return (bool)GetValue(IsUpdatingProperty); }
            protected set { SetValue(IsUpdatingProperty, value); }
        }

        /// <summary>
        /// Get or set the amount of random life each plankton element has. This should be represented as a percentage. This is a dependency property.
        /// </summary>
        public double Life
        {
            get { return (double)GetValue(LifeProperty); }
            set { SetValue(LifeProperty, value); }
        }

        /// <summary>
        /// Get or set the size of the bubble represented as equal with and height. This is a dependency property.
        /// </summary>
        public double BubbleSize
        {
            get { return (double)GetValue(BubbleSizeProperty); }
            set { SetValue(BubbleSizeProperty, value); }
        }

        /// <summary>
        /// Get or set if child bubbles are being used. This is a dependency property.
        /// </summary>
        public bool UseChildBubbles
        {
            get { return (bool)GetValue(UseChildBubblesProperty); }
            set { SetValue(UseChildBubblesProperty, value); }
        }

        /// <summary>
        /// Get or set the child bubbles buoyancy value. This is a dependency property.
        /// </summary>
        public double ChildBubbleBuoyancy
        {
            get { return (double)GetValue(ChildBubbleBuoyancyProperty); }
            set { SetValue(ChildBubbleBuoyancyProperty, value); }
        }

        /// <summary>
        /// Get or set the child bubbles rate value. This is a dependency property.
        /// </summary>
        public double ChildBubbleRate
        {
            get { return (double)GetValue(ChildBubbleRateProperty); }
            set { SetValue(ChildBubbleRateProperty, value); }
        }

        /// <summary>
        /// Get or set the child bubble size variation, represented as a percentage. This is a dependency property.
        /// </summary>
        public double ChildBubbleSizeVariation
        {
            get { return (double)GetValue(ChildBubbleSizeVariationProperty); }
            set { SetValue(ChildBubbleSizeVariationProperty, value); }
        }

        /// <summary>
        /// Get or set if the zoom preview is being used. This is a dependency property.
        /// </summary>
        public bool UseZoomPreview
        {
            get { return (bool)GetValue(UseZoomPreviewProperty); }
            set { SetValue(UseZoomPreviewProperty, value); }
        }

        /// <summary>
        /// Get or set if the auto pan on zoom preview is being used. This is a dependency property.
        /// </summary>
        public bool UseAutoPanOnZoomPreview
        {
            get { return (bool)GetValue(UseAutoPanOnZoomPreviewProperty); }
            set { SetValue(UseAutoPanOnZoomPreviewProperty, value); }
        }

        /// <summary>
        /// Get or set the maximum zoom. This is a dependency property.
        /// </summary>
        public double MaximumZoom
        {
            get { return (double)GetValue(MaximumZoomProperty); }
            set { SetValue(MaximumZoomProperty, value); }
        }

        /// <summary>
        /// Get or set the minimum zoom. This is a dependency property.
        /// </summary>
        public double MinimumZoom
        {
            get { return (double)GetValue(MinimumZoomProperty); }
            set { SetValue(MinimumZoomProperty, value); }
        }

        /// <summary>
        /// Get or set the auto pan zooming speed. This is a dependency property.
        /// </summary>
        public double AutoPanSpeed
        {
            get { return (double)GetValue(AutoPanSpeedProperty); }
            set { SetValue(AutoPanSpeedProperty, value); }
        }

        /// <summary>
        /// Get or set the auto pan sensitivity. This is a dependency property.
        /// </summary>
        public double AutoPanSensitivity
        {
            get { return (double)GetValue(AutoPanSensitivityProperty); }
            set { SetValue(AutoPanSensitivityProperty, value); }
        }

        /// <summary>
        /// Get or set the maximum amount of child bubbles allowed at any one time. This is a dependency property.
        /// </summary>
        public int MaximumChildBubbles
        {
            get { return (int)GetValue(MaximumChildBubblesProperty); }
            set { SetValue(MaximumChildBubblesProperty, value); }
        }

        /// <summary>
        /// Get or set if using a sea bed. This is a dependency property.
        /// </summary>
        public bool UseSeaBed
        {
            get { return (bool)GetValue(UseSeaBedProperty); }
            set { SetValue(UseSeaBedProperty, value); }
        }

        /// <summary>
        /// Get or set the sea bed smoothness. This is a dependency property.
        /// </summary>
        public double SeaBedSmoothness
        {
            get { return (double)GetValue(SeaBedSmoothnessProperty); }
            set { SetValue(SeaBedSmoothnessProperty, value); }
        }

        /// <summary>
        /// Get or set the sea bed maximum incline. This is a dependency property.
        /// </summary>
        public double SeaBedMaxIncline
        {
            get { return (double)GetValue(SeaBedMaxInclineProperty); }
            set { SetValue(SeaBedMaxInclineProperty, value); }
        }

        /// <summary>
        /// Get or set the sea bed geometry as a string. This is a dependency property.
        /// </summary>
        public string LastSeaBedGeometry
        {
            get { return (string)GetValue(LastSeaBedGeometryProperty); }
            set { SetValue(LastSeaBedGeometryProperty, value); }
        }

        /// <summary>
        /// Get the version of this software. This is a dependency property.
        /// </summary>
        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            protected set { SetValue(VersionProperty, value); }
        }

        /// <summary>
        /// Get or set if using plankton attraction to bubbles. This is a dependency property.
        /// </summary>
        public bool UsePlanktonAttraction
        {
            get { return (bool)GetValue(UsePlanktonAttractionProperty); }
            set { SetValue(UsePlanktonAttractionProperty, value); }
        }

        /// <summary>
        /// Get or set if plankton attraction is inverted. This is a dependency property.
        /// </summary>
        public bool InvertPlanktonAttraction
        {
            get { return (bool)GetValue(InvertPlanktonAttractionProperty); }
            set { SetValue(InvertPlanktonAttractionProperty, value); }
        }

        /// <summary>
        /// Get or set if using plankton attract to child bubbles. This is a dependency property.
        /// </summary>
        public bool PlanktonAttractToChildBubbles
        {
            get { return (bool)GetValue(PlanktonAttractToChildBubblesProperty); }
            set { SetValue(PlanktonAttractToChildBubblesProperty, value); }
        }

        /// <summary>
        /// Get or set the plankton attraction strength. This is a dependency property.
        /// </summary>
        public double PlanktonAttractionStrength
        {
            get { return (double)GetValue(PlanktonAttractionStrengthProperty); }
            set { SetValue(PlanktonAttractionStrengthProperty, value); }
        }

        /// <summary>
        /// Get or set the plankton attraction reach. This is a dependency property.
        /// </summary>
        public double PlanktonAttractionReach
        {
            get { return (double)GetValue(PlanktonAttractionReachProperty); }
            set { SetValue(PlanktonAttractionReachProperty, value); }
        }

        /// <summary>
        /// Get the last refrest time in ms. This is a dependency property.
        /// </summary>
        public int LastRefreshTime
        {
            get { return (int)GetValue(LastRefreshTimeProperty); }
            private set { SetValue(LastRefreshTimeProperty, value); }
        }

        /// <summary>
        /// Get the amount of currently active child bubbles. This is a dependency property.
        /// </summary>
        public int ActiveChildBubbles
        {
            get { return (int)GetValue(ActiveChildBubblesProperty); }
            private set { SetValue(ActiveChildBubblesProperty, value); }
        }

        /// <summary>
        /// Get the amount of main bubble collisions that occured during this update. This is a dependency property.
        /// </summary>
        public int MainBubbleCollisionsThisUpdate
        {
            get { return (int)GetValue(MainBubbleCollisionsThisUpdateProperty); }
            private set { SetValue(MainBubbleCollisionsThisUpdateProperty, value); }
        }

        /// <summary>
        /// Get or set the viscosity of the water. This is a dependency property.
        /// </summary>
        public double WaterViscosity
        {
            get { return (double)GetValue(WaterViscosityProperty); }
            set { SetValue(WaterViscosityProperty, value); }
        }

        /// <summary>
        /// Get or set the refresh time in milliseconds. This is a dependency property.
        /// </summary>
        public int RefreshTime
        {
            get { return (int)GetValue(RefreshTimeProperty); }
            set { SetValue(RefreshTimeProperty, value); }
        }

        /// <summary>
        /// Get or set if using random element fill. This is a dependency property.
        /// </summary>
        public bool UseRandomElementFill
        {
            get { return (bool)GetValue(UseRandomElementFillProperty); }
            set { SetValue(UseRandomElementFillProperty, value); }
        }

        /// <summary>
        /// Get or set if random element fills are generated from random. If this is false then the standard fills are used. This is a dependency property.
        /// </summary>
        public bool GenerateRandomElementFill
        {
            get { return (bool)GetValue(GenerateRandomElementFillProperty); }
            set { SetValue(GenerateRandomElementFillProperty, value); }
        }

        /// <summary>
        /// Get or set if multiple random element fills are generated from random. If this is false just a single fill is generated. This is a dependency property.
        /// </summary>
        public bool GenerateMultipleRandomElementFill
        {
            get { return (bool)GetValue(GenerateMultipleRandomElementFillProperty); }
            set { SetValue(GenerateMultipleRandomElementFillProperty, value); }
        }

        /// <summary>
        /// Get or set if luminous random element fills are generated from random. This is a dependency property.
        /// </summary>
        public bool GenerateRandomLuminousElementFill
        {
            get { return (bool)GetValue(GenerateRandomLuminousElementFillProperty); }
            set { SetValue(GenerateRandomLuminousElementFillProperty, value); }
        }

        /// <summary>
        /// Get or set if efficient values are used when randomly generating values. This is useful for older computers to ensure useable values are generated. This is a dependency property.
        /// </summary>
        public bool UseEfficientValuesWhenRandomGenerating
        {
            get { return (bool)GetValue(UseEfficientValuesWhenRandomGeneratingProperty); }
            set { SetValue(UseEfficientValuesWhenRandomGeneratingProperty, value); }
        }

        /// <summary>
        /// Get or set if smoother mouse position updating is used. If this is set to true it could cause hang ups on slower systems with more processor intensive settings. This is a dependency property.
        /// </summary>
        public bool UseSmoothMousePositionUpdating
        {
            get { return (bool)GetValue(UseSmoothMousePositionUpdatingProperty); }
            set { SetValue(UseSmoothMousePositionUpdatingProperty, value); }
        }

        /// <summary>
        /// Get or set the zoom preview size. This is a dependency property.
        /// </summary>
        public double ZoomPreviewSize
        {
            get { return (double)GetValue(ZoomPreviewSizeProperty); }
            set { SetValue(ZoomPreviewSizeProperty, value); }
        }

        /// <summary>
        /// Get or set if in the event the main bubble is not available, i.e off screen, should the zoom preview focus on the most interesting other element within the area. If this is false the preview will be removed while the main bubble is not available. This is a dependency property.
        /// </summary>
        public bool IfMainBubbleNotAvailablePreviewMostInterestingElement
        {
            get { return (bool)GetValue(IfMainBubbleNotAvailablePreviewMostInterestingElementProperty); }
            set { SetValue(IfMainBubbleNotAvailablePreviewMostInterestingElementProperty, value); }
        }

        /// <summary>
        /// Get or set if the options panel is shown. This is a dependency property.
        /// </summary>
        public bool ShowOptions
        {
            get { return (bool)GetValue(ShowOptionsProperty); }
            set { SetValue(ShowOptionsProperty, value); }
        }

        /// <summary>
        /// Get or set if the update is paused. This is a dependency property.
        /// </summary>
        public bool IsPaused
        {
            get { return (bool)GetValue(IsPausedProperty); }
            set { SetValue(IsPausedProperty, value); }
        }

        /// <summary>
        /// Get if this has been forced into tranparent mode. This can be called with PlanktonControl.ForceTransparentBackground, but cannot be undone. This is a dependency property.
        /// </summary>
        public bool IsForcedIntoTransparentMode
        {
            get { return (bool)GetValue(IsForcedIntoTransparentModeProperty); }
            private set { SetValue(IsForcedIntoTransparentModeProperty, value); }
        }

        /// <summary>
        /// Get or set if a random sea brush is generated and used. This is a dependency property.
        /// </summary>
        public bool GenerateAndUseRandomSeaBrush
        {
            get { return (bool)GetValue(GenerateAndUseRandomSeaBrushProperty); }
            set { SetValue(GenerateAndUseRandomSeaBrushProperty, value); }
        }

        /// <summary>
        /// Get or set if a random sea bed brush is generated and used. This is a dependency property.
        /// </summary>
        public bool GenerateAndUseRandomSeaBedBrush
        {
            get { return (bool)GetValue(GenerateAndUseRandomSeaBedBrushProperty); }
            set { SetValue(GenerateAndUseRandomSeaBedBrushProperty, value); }
        }

        /// <summary>
        /// Get or set if only standard brushes are used when random generating settings. This is a dependency property.
        /// </summary>
        public bool UseOnlyStandardBrushesWhenRandomGenerating
        {
            get { return (bool)GetValue(UseOnlyStandardBrushesWhenRandomGeneratingProperty); }
            set { SetValue(UseOnlyStandardBrushesWhenRandomGeneratingProperty, value); }
        }

        /// <summary>
        /// Get or set if this is in full screen mode. This is a dependency property.
        /// </summary>
        public bool IsInFullScreenMode
        {
            get { return (bool)GetValue(IsInFullScreenModeProperty); }
            set { SetValue(IsInFullScreenModeProperty, value); }
        }

        /// <summary>
        /// Get or set if arc segments are used in the sea bed path. This is a dependency property.
        /// </summary>
        public bool UseArcSegmentsInSeaBedPath
        {
            get { return (bool)GetValue(UseArcSegmentsInSeaBedPathProperty); }
            set { SetValue(UseArcSegmentsInSeaBedPathProperty, value); }
        }

        /// <summary>
        /// Get or set if line segments are used in the sea bed path. This is a dependency property.
        /// </summary>
        public bool UseLineSegmentsInSeaBedPath
        {
            get { return (bool)GetValue(UseLineSegmentsInSeaBedPathProperty); }
            set { SetValue(UseLineSegmentsInSeaBedPathProperty, value); }
        }

        /// <summary>
        /// Get or set if plankton brush is overridden with the PlanktonControl.CustomePlanktonBrush property. This is a dependency property.
        /// </summary>
        public bool OverridePlanktonBrushWithCustom
        {
            get { return (bool)GetValue(OverridePlanktonBrushWithCustomProperty); }
            set { SetValue(OverridePlanktonBrushWithCustomProperty, value); }
        }

        /// <summary>
        /// Get or set if background brush is overridden with the PlanktonControl.CustomeBackgroundBrush property. This is a dependency property.
        /// </summary>
        public bool OverrideBackgroundBrushWithCustom
        {
            get { return (bool)GetValue(OverrideBackgroundBrushWithCustomProperty); }
            set { SetValue(OverrideBackgroundBrushWithCustomProperty, value); }
        }

        /// <summary>
        /// Get or set if sea bed brush is overridden with the PlanktonControl.CustomeSeaBedBrush property. This is a dependency property.
        /// </summary>
        public bool OverrideSeaBedBrushWithCustom
        {
            get { return (bool)GetValue(OverrideSeaBedBrushWithCustomProperty); }
            set { SetValue(OverrideSeaBedBrushWithCustomProperty, value); }
        }

        /// <summary>
        /// Get or set the custom plankton brush. This is a dependency property
        /// </summary>
        public Brush CustomPlanktonBrush
        {
            get { return (Brush)GetValue(CustomPlanktonBrushProperty); }
            set { SetValue(CustomPlanktonBrushProperty, value); }
        }

        /// <summary>
        /// Get or set the custom background brush. This is a dependency property.
        /// </summary>
        public Brush CustomBackgroundBrush
        {
            get { return (Brush)GetValue(CustomBackgroundBrushProperty); }
            set { SetValue(CustomBackgroundBrushProperty, value); }
        }

        /// <summary>
        /// Get or set the custom sea bed brush. This is a dependency property.
        /// </summary>
        public Brush CustomSeaBedBrush
        {
            get { return (Brush)GetValue(CustomSeaBedBrushProperty); }
            set { SetValue(CustomSeaBedBrushProperty, value); }
        }

        /// <summary>
        /// Get or set if the current is being used. This is a dependency property.
        /// </summary>
        public bool UseCurrent
        {
            get { return (bool)GetValue(UseCurrentProperty); }
            set { SetValue(UseCurrentProperty, value); }
        }

        /// <summary>
        /// Get or set the current rate. This is a dependency property.
        /// </summary>
        public double CurrentRate
        {
            get { return (double)GetValue(CurrentRateProperty); }
            set { SetValue(CurrentRateProperty, value); }
        }

        /// <summary>
        /// Get or set the current variation, represented as a percentage. This is a dependency property.
        /// </summary>
        public double CurrentVariation
        {
            get { return (double)GetValue(CurrentVariationProperty); }
            set { SetValue(CurrentVariationProperty, value); }
        }

        /// <summary>
        /// Get or set the strength of the current. This is a dependency property.
        /// </summary>
        public double CurrentStrength
        {
            get { return (double)GetValue(CurrentStrengthProperty); }
            set { SetValue(CurrentStrengthProperty, value); }
        }

        /// <summary>
        /// Get or set the direction of the current, in degrees. This is a dependency property.
        /// </summary>
        public double CurrentDirection
        {
            get { return (double)GetValue(CurrentDirectionProperty); }
            set { SetValue(CurrentDirectionProperty, value); }
        }

        /// <summary>
        /// Get or set if a random current direction is used. This is a dependency property.
        /// </summary>
        public bool UseRandomCurrentDirection
        {
            get { return (bool)GetValue(UseRandomCurrentDirectionProperty); }
            set { SetValue(UseRandomCurrentDirectionProperty, value); }
        }

        /// <summary>
        /// Get the active current. This is a dependency property.
        /// </summary>
        public Current ActiveCurrent
        {
            get { return (Current)GetValue(ActiveCurrentProperty); }
            protected set { SetValue(ActiveCurrentProperty, value); }
        }

        /// <summary>
        /// Get or set the mode to use with the zoom preview locator. This is a dependency property.
        /// </summary>
        public ZoomPreviewLocaterMode ZoomPreviewLocatorMode
        {
            get { return (ZoomPreviewLocaterMode)GetValue(ZoomPreviewLocatorModeProperty); }
            set { SetValue(ZoomPreviewLocatorModeProperty, value); }
        }

        /// <summary>
        /// Get if the zoom preview locator is visible. This is a dependency property.
        /// </summary>
        public bool IsZoomPreviewLocatorVisible
        {
            get { return (bool)GetValue(IsZoomPreviewLocatorVisibleProperty); }
            private set { SetValue(IsZoomPreviewLocatorVisibleProperty, value); }
        }

        /// <summary>
        /// Get or set if a Z value is applied to the current. This is a dependency property.
        /// </summary>
        public bool UseZOnCurrent
        {
            get { return (bool)GetValue(UseZOnCurrentProperty); }
            set { SetValue(UseZOnCurrentProperty, value); }
        }

        /// <summary>
        /// Get or set the current Z step. This is a dependency property.
        /// </summary>
        public double CurrentZStep
        {
            get { return (double)GetValue(CurrentZStepProperty); }
            set { SetValue(CurrentZStepProperty, value); }
        }

        /// <summary>
        /// Get or set the current Z step variation, represented as a percentage. This is a dependency property.
        /// </summary>
        public double CurrentZStepVariation
        {
            get { return (double)GetValue(CurrentZStepVariationProperty); }
            set { SetValue(CurrentZStepVariationProperty, value); }
        }

        /// <summary>
        /// Get or set the current mode. This is a dependency property.
        /// </summary>
        public CurrentSwellStage CurrentMode
        {
            get { return (CurrentSwellStage)GetValue(CurrentModeProperty); }
            set { SetValue(CurrentModeProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'About' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowAboutTab
        {
            get { return (bool)GetValue(ShowAboutTabProperty); }
            set { SetValue(ShowAboutTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Quick Settings' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowQuickSettingsTab
        {
            get { return (bool)GetValue(ShowQuickSettingsTabProperty); }
            set { SetValue(ShowQuickSettingsTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Plankton' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowPlanktonTab
        {
            get { return (bool)GetValue(ShowPlanktonTabProperty); }
            set { SetValue(ShowPlanktonTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Bubbles' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowBubblesTab
        {
            get { return (bool)GetValue(ShowBubblesTabProperty); }
            set { SetValue(ShowBubblesTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Water' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowWaterTab
        {
            get { return (bool)GetValue(ShowWaterTabProperty); }
            set { SetValue(ShowWaterTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Sea Bed' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowSeaBedTab
        {
            get { return (bool)GetValue(ShowSeaBedTabProperty); }
            set { SetValue(ShowSeaBedTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Current' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowCurrentTab
        {
            get { return (bool)GetValue(ShowCurrentTabProperty); }
            set { SetValue(ShowCurrentTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Preview' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowPreviewTab
        {
            get { return (bool)GetValue(ShowPreviewTabProperty); }
            set { SetValue(ShowPreviewTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Other' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowOtherTab
        {
            get { return (bool)GetValue(ShowOtherTabProperty); }
            set { SetValue(ShowOtherTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Save/Load' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowSaveLoadTab
        {
            get { return (bool)GetValue(ShowSaveLoadTabProperty); }
            set { SetValue(ShowSaveLoadTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the 'Mass' tab is shown. This is a dependency property.
        /// </summary>
        public bool ShowMassTab
        {
            get { return (bool)GetValue(ShowMassTabProperty); }
            set { SetValue(ShowMassTabProperty, value); }
        }

        /// <summary>
        /// Get or set if the water viscosity is ignore when generating a current. This is a dependency property.
        /// </summary>
        public bool IgnoreWaterViscosityWhenGeneratingCurrent
        {
            get { return (bool)GetValue(IgnoreWaterViscosityWhenGeneratingCurrentProperty); }
            set { SetValue(IgnoreWaterViscosityWhenGeneratingCurrentProperty, value); }
        }

        /// <summary>
        /// Get or set the current acceleration. This is a dependency property.
        /// </summary>
        public double CurrentAcceleration
        {
            get { return (double)GetValue(CurrentAccelerationProperty); }
            set { SetValue(CurrentAccelerationProperty, value); }
        }

        /// <summary>
        /// Get or set the current deceleration. This is a dependency property.
        /// </summary>
        public double CurrentDeceleration
        {
            get { return (double)GetValue(CurrentDecelerationProperty); }
            set { SetValue(CurrentDecelerationProperty, value); }
        }

        /// <summary>
        /// Get if there is an active current. This is a dependency property.
        /// </summary>
        public bool IsCurrentActive
        {
            get { return (bool)GetValue(IsCurrentActiveProperty); }
            protected set { SetValue(IsCurrentActiveProperty, value); }
        }

        /// <summary>
        /// Get the direction of the active current. This is a dependency property.
        /// </summary>
        public double ActiveCurrentDirection
        {
            get { return (double)GetValue(ActiveCurrentDirectionProperty); }
            protected set { SetValue(ActiveCurrentDirectionProperty, value); }
        }

        /// <summary>
        /// Get or set if the current indicator is shown. This is a dependency property.
        /// </summary>
        public bool ShowCurrentIndicator
        {
            get { return (bool)GetValue(ShowCurrentIndicatorProperty); }
            set { SetValue(ShowCurrentIndicatorProperty, value); }
        }

        /// <summary>
        /// Get or set the brush used to render the current indicators background. This is a dependency property.
        /// </summary>
        public Brush CurrentIndicatorBackgroundBrush
        {
            get { return (Brush)GetValue(CurrentIndicatorBackgroundBrushProperty); }
            set { SetValue(CurrentIndicatorBackgroundBrushProperty, value); }
        }

        /// <summary>
        /// Get or set the brush used to render the current indicators foreground. This is a dependency property.
        /// </summary>
        public Brush CurrentIndicatorForegroundBrush
        {
            get { return (Brush)GetValue(CurrentIndicatorForegroundBrushProperty); }
            set { SetValue(CurrentIndicatorForegroundBrushProperty, value); }
        }

        /// <summary>
        /// Get or set the brush used to render the zoom preview. This is a dependency property.
        /// </summary>
        public Brush ZoomPreviewBrush
        {
            get { return (Brush)GetValue(ZoomPreviewBrushProperty); }
            set { SetValue(ZoomPreviewBrushProperty, value); }
        }

        /// <summary>
        /// Get or set if animation should be used. This is a dependency property.
        /// </summary>
        public bool UseAnimation
        {
            get { return (bool)GetValue(UseAnimationProperty); }
            set { SetValue(UseAnimationProperty, value); }
        }

        /// <summary>
        /// Get the duration to use when animating the options expander. This is a dependency property.
        /// </summary>
        public Duration OptionsExpanderAnimationDuration
        {
            get { return (Duration)GetValue(OptionsExpanderAnimationDurationProperty); }
            protected set { SetValue(OptionsExpanderAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Get if options breaking out to an external window is currently viable. This is a dependency property.
        /// </summary>
        public bool IsOptionsBreakoutViable
        {
            get { return (bool)GetValue(IsOptionsBreakoutViableProperty); }
            protected set { SetValue(IsOptionsBreakoutViableProperty, value); }
        }

        /// <summary>
        /// Get or set if standard physics are maintained when randomly generating settings. This is a dependency property.
        /// </summary>
        public bool MaintainStandardPhysicsWhenRandomGeneratingSettings
        {
            get { return (bool)GetValue(MaintainStandardPhysicsWhenRandomGeneratingSettingsProperty); }
            set { SetValue(MaintainStandardPhysicsWhenRandomGeneratingSettingsProperty, value); }
        }

        /// <summary>
        /// Get or set if the zoom preview blur effect is used. This is a dependency property.
        /// </summary>
        public bool UseZoomPreviewBlurEffect
        {
            get { return (bool)GetValue(UseZoomPreviewBlurEffectProperty); }
            set { SetValue(UseZoomPreviewBlurEffectProperty, value); }
        }

        /// <summary>
        /// Get or set the maximum zoom preview blur. This is a dependency property.
        /// </summary>
        public double MaximumZoomPreviewBlur
        {
            get { return (double)GetValue(MaximumZoomPreviewBlurProperty); }
            set { SetValue(MaximumZoomPreviewBlurProperty, value); }
        }

        /// <summary>
        /// Get or set the strength of the zoom preview blur. This is a dependency property.
        /// </summary>
        public double ZoomPreviewBlurStrength
        {
            get { return (double)GetValue(ZoomPreviewBlurStrengthProperty); }
            set { SetValue(ZoomPreviewBlurStrengthProperty, value); }
        }

        /// <summary>
        /// Get or set the speed of the zoom preview blur correction factor. This is a dependency property.
        /// </summary>
        public double ZoomPreviewBlurCorrection
        {
            get { return (double)GetValue(ZoomPreviewBlurCorrectionProperty); }
            set { SetValue(ZoomPreviewBlurCorrectionProperty, value); }
        }

        /// <summary>
        /// Get or set if gravity is used. This is a dependency property.
        /// </summary>
        public bool UseGravity
        {
            get { return (bool)GetValue(UseGravityProperty); }
            set { SetValue(UseGravityProperty, value); }
        }

        /// <summary>
        /// Get or set the density of elements. This is a dependency property.
        /// </summary>
        public double Density
        {
            get { return (double)GetValue(DensityProperty); }
            set { SetValue(DensityProperty, value); }
        }

        /// <summary>
        /// Get or set if the lighting effect is used. This is a dependency property.
        /// </summary>
        public bool UseLightingEffect
        {
            get { return (bool)GetValue(UseLightingEffectProperty); }
            set { SetValue(UseLightingEffectProperty, value); }
        }

        /// <summary>
        /// Get or set the strength of the lighting effect. This is a dependency property.
        /// </summary>
        public double LightingEffectStrength
        {
            get { return (double)GetValue(LightingEffectStrengthProperty); }
            set { SetValue(LightingEffectStrengthProperty, value); }
        }

        /// <summary>
        /// Get or set the speed ratio of the lighting effect. This is a dependency property.
        /// </summary>
        public double LightingEffectSpeedRatio
        {
            get { return (double)GetValue(LightingEffectSpeedRatioProperty); }
            set { SetValue(LightingEffectSpeedRatioProperty, value); }
        }

        /// <summary>
        /// Get or set a list of all plankton elements.
        /// </summary>
        private readonly List<MoveableElement> plankton = new List<MoveableElement>();
        
        #endregion

        #region DependencyProperties

        /// <summary>
        /// Identifies the PlanktonControl.Elements property.
        /// </summary>
        public static readonly DependencyProperty ElementsProperty = DependencyProperty.Register("Elements", typeof (int), typeof (PlanktonControl), new PropertyMetadata(750));

        /// <summary>
        /// Identifies the PlanktonControl.ElementsSize property.
        /// </summary>
        public static readonly DependencyProperty ElementsSizeProperty = DependencyProperty.Register("ElementsSize", typeof (double), typeof (PlanktonControl), new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the PlanktonControl.ElementsSizeVariation property.
        /// </summary>
        public static readonly DependencyProperty ElementsSizeVariationProperty = DependencyProperty.Register("ElementsSizeVariation", typeof (double), typeof (PlanktonControl), new PropertyMetadata(75d));

        /// <summary>
        /// Identifies the PlanktonControl.Travel property.
        /// </summary>
        public static readonly DependencyProperty TravelProperty = DependencyProperty.Register("Travel", typeof (double), typeof (PlanktonControl), new PropertyMetadata(3d));

        /// <summary>
        /// Identifies the PlanktonControl.IsUpdating property.
        /// </summary>
        public static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.Register("IsUpdating", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.Life property.
        /// </summary>
        public static readonly DependencyProperty LifeProperty = DependencyProperty.Register("Life", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0d));

        /// <summary>
        /// Identifies the PlanktonControl.BubbleSize property.
        /// </summary>
        public static readonly DependencyProperty BubbleSizeProperty = DependencyProperty.Register("BubbleSize", typeof (double), typeof (PlanktonControl), new PropertyMetadata(20d, OnBubbleSizePropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.UseChildBubbles property.
        /// </summary>
        public static readonly DependencyProperty UseChildBubblesProperty = DependencyProperty.Register("UseChildBubbles", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ChildBubbleBuoyancy property.
        /// </summary>
        public static readonly DependencyProperty ChildBubbleBuoyancyProperty = DependencyProperty.Register("ChildBubbleBuoyancy", typeof (double), typeof (PlanktonControl), new PropertyMetadata(1.0d));

        /// <summary>
        /// Identifies the PlanktonControl.ChildBubbleRate property.
        /// </summary>
        public static readonly DependencyProperty ChildBubbleRateProperty = DependencyProperty.Register("ChildBubbleRate", typeof (double), typeof (PlanktonControl), new PropertyMetadata(5d));

        /// <summary>
        /// Identifies the PlanktonControl.ChildBubbleSizeVariation property.
        /// </summary>
        public static readonly DependencyProperty ChildBubbleSizeVariationProperty = DependencyProperty.Register("ChildBubbleSizeVariation", typeof (double), typeof (PlanktonControl), new PropertyMetadata(75d));

        /// <summary>
        /// Identifies the PlanktonControl.UseZoomPreview property.
        /// </summary>
        public static readonly DependencyProperty UseZoomPreviewProperty = DependencyProperty.Register("UseZoomPreview", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true, OnUseZoomPreviewPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.UseAutoPanOnZoomPreview property.
        /// </summary>
        public static readonly DependencyProperty UseAutoPanOnZoomPreviewProperty = DependencyProperty.Register("UseAutoPanOnZoomPreview", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true, OnUseAutoPanOnZoomPreviewPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.MaximumZoom property.
        /// </summary>
        public static readonly DependencyProperty MaximumZoomProperty = DependencyProperty.Register("MaximumZoom", typeof (double), typeof (PlanktonControl), new PropertyMetadata(5d));

        /// <summary>
        /// Identifies the PlanktonControl.MinimumZoom property.
        /// </summary>
        public static readonly DependencyProperty MinimumZoomProperty = DependencyProperty.Register("MinimumZoom", typeof (double), typeof (PlanktonControl), new PropertyMetadata(2d));

        /// <summary>
        /// Identifies the PlanktonControl.AutoPanZoomSpeed property.
        /// </summary>
        public static readonly DependencyProperty AutoPanSpeedProperty = DependencyProperty.Register("AutoPanSpeed", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0.01d));

        /// <summary>
        /// Identifies the PlanktonControl.AutoPanSensitivity property.
        /// </summary>
        public static readonly DependencyProperty AutoPanSensitivityProperty = DependencyProperty.Register("AutoPanSensitivity", typeof (double), typeof (PlanktonControl), new PropertyMetadata(1d));

        /// <summary>
        /// Identifies the PlanktonControl.MaximumChildBubbles property.
        /// </summary>
        public static readonly DependencyProperty MaximumChildBubblesProperty = DependencyProperty.Register("MaximumChildBubbles", typeof (int), typeof (PlanktonControl), new PropertyMetadata(250));

        /// <summary>
        /// Identifies the PlanktonControl.UseSeaBed property.
        /// </summary>
        public static readonly DependencyProperty UseSeaBedProperty = DependencyProperty.Register("UseSeaBed", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true, OnUseSeaBedPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.SeaBedSmoothness property.
        /// </summary>
        public static readonly DependencyProperty SeaBedSmoothnessProperty = DependencyProperty.Register("SeaBedSmoothness", typeof (double), typeof (PlanktonControl), new PropertyMetadata(40d));

        /// <summary>
        /// Identifies the PlanktonControl.SeaBedMaxIncline property.
        /// </summary>
        public static readonly DependencyProperty SeaBedMaxInclineProperty = DependencyProperty.Register("SeaBedMaxIncline", typeof (double), typeof (PlanktonControl), new PropertyMetadata(10d));

        /// <summary>
        /// Identifies the PlanktonControl.LastSeaBedGeometry property.
        /// </summary>
        public static readonly DependencyProperty LastSeaBedGeometryProperty = DependencyProperty.Register("LastSeaBedGeometry", typeof (string), typeof (PlanktonControl), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the PlanktonControl.Version property.
        /// </summary>
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register("Version", typeof (string), typeof (PlanktonControl));

        /// <summary>
        /// Identifies the PlanktonControl.UsePlanktonAttraction property.
        /// </summary>
        public static readonly DependencyProperty UsePlanktonAttractionProperty = DependencyProperty.Register("UsePlanktonAttraction", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.InvertPlanktonAttraction property.
        /// </summary>
        public static readonly DependencyProperty InvertPlanktonAttractionProperty = DependencyProperty.Register("InvertPlanktonAttraction", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.PlanktonAttractToChildBubbles property.
        /// </summary>
        public static readonly DependencyProperty PlanktonAttractToChildBubblesProperty = DependencyProperty.Register("PlanktonAttractToChildBubbles", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.PlanktonAttractionStrength property.
        /// </summary>
        public static readonly DependencyProperty PlanktonAttractionStrengthProperty = DependencyProperty.Register("PlanktonAttractionStrength", typeof (double), typeof (PlanktonControl), new PropertyMetadata(5d));

        /// <summary>
        /// Identifies the PlanktonControl.PlanktonAttractionReach property.
        /// </summary>
        public static readonly DependencyProperty PlanktonAttractionReachProperty = DependencyProperty.Register("PlanktonAttractionReach", typeof (double), typeof (PlanktonControl), new PropertyMetadata(3d));

        /// <summary>
        /// Identifies the PlanktonControl.LastRefreshTime property.
        /// </summary>
        public static readonly DependencyProperty LastRefreshTimeProperty = DependencyProperty.Register("LastRefreshTime", typeof (int), typeof (PlanktonControl), new PropertyMetadata(0));

        /// <summary>
        /// Identifies the PlanktonControl.ActiveChildBubbles property.
        /// </summary>
        public static readonly DependencyProperty ActiveChildBubblesProperty = DependencyProperty.Register("ActiveChildBubbles", typeof (int), typeof (PlanktonControl), new PropertyMetadata(0));

        /// <summary>
        /// Identifies the PlanktonControl.MainBubbleCollisionsThisUpdate property.
        /// </summary>
        public static readonly DependencyProperty MainBubbleCollisionsThisUpdateProperty = DependencyProperty.Register("MainBubbleCollisionsThisUpdate", typeof (int), typeof (PlanktonControl), new PropertyMetadata(0));

        /// <summary>
        /// Identifies the PlanktonControl.RefreshTime property.
        /// </summary>
        public static readonly DependencyProperty RefreshTimeProperty = DependencyProperty.Register("RefreshTime", typeof (int), typeof (PlanktonControl), new PropertyMetadata(10, OnRefreshTimePropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.WaterViscosity property.
        /// </summary>
        public static readonly DependencyProperty WaterViscosityProperty = DependencyProperty.Register("WaterViscosity", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0.95d));

        /// <summary>
        /// Identifies the PlanktonControl.UseRandomElementFill property.
        /// </summary>
        public static readonly DependencyProperty UseRandomElementFillProperty = DependencyProperty.Register("UseRandomElementFill", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnUseRandomElementFillPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.GenerateRandomElementFill property.
        /// </summary>
        public static readonly DependencyProperty GenerateRandomElementFillProperty = DependencyProperty.Register("GenerateRandomElementFill", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnGenerateRandomElementFillPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.GenerateMultipleRandomElementFill property.
        /// </summary>
        public static readonly DependencyProperty GenerateMultipleRandomElementFillProperty = DependencyProperty.Register("GenerateMultipleRandomElementFill", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnGenerateMultipleRandomElementFillPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.GenerateRandomLuminousElementFill property.
        /// </summary>
        public static readonly DependencyProperty GenerateRandomLuminousElementFillProperty = DependencyProperty.Register("GenerateRandomLuminousElementFill", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnGenerateRandomLuminousElementFillPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.UseEfficientValuesWhenRandomGenerating property.
        /// </summary>
        public static readonly DependencyProperty UseEfficientValuesWhenRandomGeneratingProperty = DependencyProperty.Register("UseEfficientValuesWhenRandomGenerating", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.UseSmoothMousePositionUpdating property.
        /// </summary>
        public static readonly DependencyProperty UseSmoothMousePositionUpdatingProperty = DependencyProperty.Register("UseSmoothMousePositionUpdating", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.ZoomPreviewSize property.
        /// </summary>
        public static readonly DependencyProperty ZoomPreviewSizeProperty = DependencyProperty.Register("ZoomPreviewSize", typeof (double), typeof (PlanktonControl), new PropertyMetadata(100d));

        /// <summary>
        /// Identifies the PlanktonControl.IfMainBubbleNotAvailablePreviewMostInterestingElement property.
        /// </summary>
        public static readonly DependencyProperty IfMainBubbleNotAvailablePreviewMostInterestingElementProperty = DependencyProperty.Register("IfMainBubbleNotAvailablePreviewMostInterestingElement", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.ShowOptions property.
        /// </summary>
        public static readonly DependencyProperty ShowOptionsProperty = DependencyProperty.Register("ShowOptions", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.IsPaused property.
        /// </summary>
        public static readonly DependencyProperty IsPausedProperty = DependencyProperty.Register("IsPaused", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnIsPausedPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.IsForcedIntoTransparentMode property.
        /// </summary>
        public static readonly DependencyProperty IsForcedIntoTransparentModeProperty = DependencyProperty.Register("IsForcedIntoTransparentMode", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.GenerateAndUseRandomSeaBrush property.
        /// </summary>
        public static readonly DependencyProperty GenerateAndUseRandomSeaBrushProperty = DependencyProperty.Register("GenerateAndUseRandomSeaBrush", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnGenerateAndUseRandomSeaBrushPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.GenerateAndUseRandomSeaBedBrush property.
        /// </summary>
        public static readonly DependencyProperty GenerateAndUseRandomSeaBedBrushProperty = DependencyProperty.Register("GenerateAndUseRandomSeaBedBrush", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnGenerateAndUseRandomSeaBedBrushPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.UseOnlyStandardBrushesWhenRandomGenerating property.
        /// </summary>
        public static readonly DependencyProperty UseOnlyStandardBrushesWhenRandomGeneratingProperty = DependencyProperty.Register("UseOnlyStandardBrushesWhenRandomGenerating", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.IsInFullScreenMode property.
        /// </summary>
        public static readonly DependencyProperty IsInFullScreenModeProperty = DependencyProperty.Register("IsInFullScreenMode", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnIsInFullScreenModePropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.UseArcSegmentsInSeaBedPath property.
        /// </summary>
        public static readonly DependencyProperty UseArcSegmentsInSeaBedPathProperty = DependencyProperty.Register("UseArcSegmentsInSeaBedPath", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.UseLineSegmentsInSeaBedPath property.
        /// </summary>
        public static readonly DependencyProperty UseLineSegmentsInSeaBedPathProperty = DependencyProperty.Register("UseLineSegmentsInSeaBedPath", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.OverridePlanktonBrushWithCustom property.
        /// </summary>
        public static readonly DependencyProperty OverridePlanktonBrushWithCustomProperty = DependencyProperty.Register("OverridePlanktonBrushWithCustom", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnOverridePlanktonBrushWithCustomPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.OverrideBackgroundBrushWithCustom property.
        /// </summary>
        public static readonly DependencyProperty OverrideBackgroundBrushWithCustomProperty = DependencyProperty.Register("OverrideBackgroundBrushWithCustom", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnOverrideBackgroundBrushWithCustomPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.OverrideSeaBedBrushWithCustom property.
        /// </summary>
        public static readonly DependencyProperty OverrideSeaBedBrushWithCustomProperty = DependencyProperty.Register("OverrideSeaBedBrushWithCustom", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnOverrideSeaBedBrushWithCustomPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.CustomPlanktonBrush property.
        /// </summary>
        public static readonly DependencyProperty CustomPlanktonBrushProperty = DependencyProperty.Register("CustomPlanktonBrush", typeof (Brush), typeof (PlanktonControl), new PropertyMetadata(Brushes.White, OnCustomPlanktonBrushPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.CustomBackgroundBrush property.
        /// </summary>
        public static readonly DependencyProperty CustomBackgroundBrushProperty = DependencyProperty.Register("CustomBackgroundBrush", typeof (Brush), typeof (PlanktonControl), new PropertyMetadata(Brushes.White, OnCustomBackgroundBrushPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.CustomSeaBedBrush property.
        /// </summary>
        public static readonly DependencyProperty CustomSeaBedBrushProperty = DependencyProperty.Register("CustomSeaBedBrush", typeof (Brush), typeof (PlanktonControl), new PropertyMetadata(Brushes.White, OnCustomSeaBedBrushPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.UseCurrent property.
        /// </summary>
        public static readonly DependencyProperty UseCurrentProperty = DependencyProperty.Register("UseCurrent", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentRate property.
        /// </summary>
        public static readonly DependencyProperty CurrentRateProperty = DependencyProperty.Register("CurrentRate", typeof (double), typeof (PlanktonControl), new PropertyMetadata(5d));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentVariation property.
        /// </summary>
        public static readonly DependencyProperty CurrentVariationProperty = DependencyProperty.Register("CurrentVariation", typeof (double), typeof (PlanktonControl), new PropertyMetadata(50d));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentStrength property.
        /// </summary>
        public static readonly DependencyProperty CurrentStrengthProperty = DependencyProperty.Register("CurrentStrength", typeof (double), typeof (PlanktonControl), new PropertyMetadata(20d));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentDirection property.
        /// </summary>
        public static readonly DependencyProperty CurrentDirectionProperty = DependencyProperty.Register("CurrentDirection", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0d));

        /// <summary>
        /// Identifies the PlanktonControl.UseRandomCurrentDirection property.
        /// </summary>
        public static readonly DependencyProperty UseRandomCurrentDirectionProperty = DependencyProperty.Register("UseRandomCurrentDirection", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.ActiveCurrent property.
        /// </summary>
        public static readonly DependencyProperty ActiveCurrentProperty = DependencyProperty.Register("ActiveCurrent", typeof (Current), typeof (PlanktonControl), new PropertyMetadata(new Current()));

        /// <summary>
        /// Identifies the PlanktonControl.ZoomPreviewLocatorMode property.
        /// </summary>
        public static readonly DependencyProperty ZoomPreviewLocatorModeProperty = DependencyProperty.Register("ZoomPreviewLocatorMode", typeof (ZoomPreviewLocaterMode), typeof (PlanktonControl), new PropertyMetadata(ZoomPreviewLocaterMode.AnythingButMainBubble, OnZoomPreviewLocatorModePropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.IsZoomPreviewLocatorVisible property.
        /// </summary>
        public static readonly DependencyProperty IsZoomPreviewLocatorVisibleProperty = DependencyProperty.Register("IsZoomPreviewLocatorVisible", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.UseZOnCurrent property.
        /// </summary>
        public static readonly DependencyProperty UseZOnCurrentProperty = DependencyProperty.Register("UseZOnCurrent", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentZStep property.
        /// </summary>
        public static readonly DependencyProperty CurrentZStepProperty = DependencyProperty.Register("CurrentZStep", typeof (double), typeof (PlanktonControl), new PropertyMetadata(50d));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentZStepVariation property.
        /// </summary>
        public static readonly DependencyProperty CurrentZStepVariationProperty = DependencyProperty.Register("CurrentZStepVariation", typeof (double), typeof (PlanktonControl), new PropertyMetadata(50d));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentMode property.
        /// </summary>
        public static readonly DependencyProperty CurrentModeProperty = DependencyProperty.Register("CurrentMode", typeof (CurrentSwellStage), typeof (PlanktonControl), new PropertyMetadata(CurrentSwellStage.PreMainUp, OnCurrentModePropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.ShowAboutTab property.
        /// </summary>
        public static readonly DependencyProperty ShowAboutTabProperty = DependencyProperty.Register("ShowAboutTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowQuickSettingsTab property.
        /// </summary>
        public static readonly DependencyProperty ShowQuickSettingsTabProperty = DependencyProperty.Register("ShowQuickSettingsTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowPlanktonTab property.
        /// </summary>
        public static readonly DependencyProperty ShowPlanktonTabProperty = DependencyProperty.Register("ShowPlanktonTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowBubblesTab property.
        /// </summary>
        public static readonly DependencyProperty ShowBubblesTabProperty = DependencyProperty.Register("ShowBubblesTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowWaterTab property.
        /// </summary>
        public static readonly DependencyProperty ShowWaterTabProperty = DependencyProperty.Register("ShowWaterTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowSeaBedTab property.
        /// </summary>
        public static readonly DependencyProperty ShowSeaBedTabProperty = DependencyProperty.Register("ShowSeaBedTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowCurrentTab property.
        /// </summary>
        public static readonly DependencyProperty ShowCurrentTabProperty = DependencyProperty.Register("ShowCurrentTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowPreviewTab property.
        /// </summary>
        public static readonly DependencyProperty ShowPreviewTabProperty = DependencyProperty.Register("ShowPreviewTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowOtherTab property.
        /// </summary>
        public static readonly DependencyProperty ShowOtherTabProperty = DependencyProperty.Register("ShowOtherTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowSaveLoadTab property.
        /// </summary>
        public static readonly DependencyProperty ShowSaveLoadTabProperty = DependencyProperty.Register("ShowSaveLoadTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.ShowMassTab property.
        /// </summary>
        public static readonly DependencyProperty ShowMassTabProperty = DependencyProperty.Register("ShowMassTab", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.IgnoreWaterViscosityWhenGeneratingCurrent property.
        /// </summary>
        public static readonly DependencyProperty IgnoreWaterViscosityWhenGeneratingCurrentProperty = DependencyProperty.Register("IgnoreWaterViscosityWhenGeneratingCurrent", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentAcceleration property.
        /// </summary>
        public static readonly DependencyProperty CurrentAccelerationProperty = DependencyProperty.Register("CurrentAcceleration", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0.95d));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentDeceleration property.
        /// </summary>
        public static readonly DependencyProperty CurrentDecelerationProperty = DependencyProperty.Register("CurrentDeceleration", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0.97d));

        /// <summary>
        /// Identifies the PlanktonControl.IsCurrentActive property.
        /// </summary>
        public static readonly DependencyProperty IsCurrentActiveProperty = DependencyProperty.Register("IsCurrentActive", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the PlanktonControl.ActiveCurrentDirection property.
        /// </summary>
        public static readonly DependencyProperty ActiveCurrentDirectionProperty = DependencyProperty.Register("ActiveCurrentDirection", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0d, OnActiveCurrentDirectionPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.ShowCurrentIndicator property.
        /// </summary>
        public static readonly DependencyProperty ShowCurrentIndicatorProperty = DependencyProperty.Register("ShowCurrentIndicator", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true, OnShowCurrentIndicatorPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentIndicatorBackgroundBrush property.
        /// </summary>
        public static readonly DependencyProperty CurrentIndicatorBackgroundBrushProperty = DependencyProperty.Register("CurrentIndicatorBackgroundBrush", typeof (Brush), typeof (PlanktonControl), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// Identifies the PlanktonControl.CurrentIndicatorForegroundBrush property.
        /// </summary>
        public static readonly DependencyProperty CurrentIndicatorForegroundBrushProperty = DependencyProperty.Register("CurrentIndicatorForegroundBrush", typeof (Brush), typeof (PlanktonControl), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// Identifies the PlanktonControl.ZoomPreviewBrush property.
        /// </summary>
        public static readonly DependencyProperty ZoomPreviewBrushProperty = DependencyProperty.Register("ZoomPreviewBrush", typeof (Brush), typeof (PlanktonControl), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// Identifies the PlanktonControl.UseAnimation property.
        /// </summary>
        public static readonly DependencyProperty UseAnimationProperty = DependencyProperty.Register("UseAnimation", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true, OnUseAnimationPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.OptionsExpanderAnimationDuration property.
        /// </summary>
        public static readonly DependencyProperty OptionsExpanderAnimationDurationProperty = DependencyProperty.Register("OptionsExpanderAnimationDuration", typeof (Duration), typeof (PlanktonControl), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(250))));

        /// <summary>
        /// Identifies the PlanktonControl.IsOptionsBreakoutViable property.
        /// </summary>
        public static readonly DependencyProperty IsOptionsBreakoutViableProperty = DependencyProperty.Register("IsOptionsBreakoutViable", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.MaintainStandardPhysicsWhenRandomGeneratingSettings property.
        /// </summary>
        public static readonly DependencyProperty MaintainStandardPhysicsWhenRandomGeneratingSettingsProperty = DependencyProperty.Register("MaintainStandardPhysicsWhenRandomGeneratingSettings", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.UseZoomPreviewBlurEffect property.
        /// </summary>
        public static readonly DependencyProperty UseZoomPreviewBlurEffectProperty = DependencyProperty.Register("UseZoomPreviewBlurEffect", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true, OnUseZoomPreviewBlurEffectPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.MaximumZoomPreviewBlur property.
        /// </summary>
        public static readonly DependencyProperty MaximumZoomPreviewBlurProperty = DependencyProperty.Register("MaximumZoomPreviewBlur", typeof (double), typeof (PlanktonControl), new PropertyMetadata(7d));

        /// <summary>
        /// Identifies the PlanktonControl.ZoomPreviewBlurStrength property.
        /// </summary>
        public static readonly DependencyProperty ZoomPreviewBlurStrengthProperty = DependencyProperty.Register("ZoomPreviewBlurStrength", typeof (double), typeof (PlanktonControl), new PropertyMetadata(5d));

        /// <summary>
        /// Identifies the PlanktonControl.ZoomPreviewBlurCorrection property.
        /// </summary>
        public static readonly DependencyProperty ZoomPreviewBlurCorrectionProperty = DependencyProperty.Register("ZoomPreviewBlurCorrection", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0.15d));

        /// <summary>
        /// Identifies the PlanktonControl.UseGravity property.
        /// </summary>
        public static readonly DependencyProperty UseGravityProperty = DependencyProperty.Register("UseGravity", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the PlanktonControl.Density property.
        /// </summary>
        public static readonly DependencyProperty DensityProperty = DependencyProperty.Register("Density", typeof (double), typeof (PlanktonControl), new PropertyMetadata(1.0d));

        /// <summary>
        /// Identifies the PlanktonControl.UseLightingEffect property.
        /// </summary>
        public static readonly DependencyProperty UseLightingEffectProperty = DependencyProperty.Register("UseLightingEffect", typeof (bool), typeof (PlanktonControl), new PropertyMetadata(false, OnUseLightingEffectPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.LightingEffectStrength property.
        /// </summary>
        public static readonly DependencyProperty LightingEffectStrengthProperty = DependencyProperty.Register("LightingEffectStrength", typeof (double), typeof (PlanktonControl), new PropertyMetadata(0.5d, OnLightingEffectStrengthPropertyChanged));

        /// <summary>
        /// Identifies the PlanktonControl.LightingEffectSpeedRatio property.
        /// </summary>
        public static readonly DependencyProperty LightingEffectSpeedRatioProperty = DependencyProperty.Register("LightingEffectSpeedRatio", typeof (double), typeof (PlanktonControl), new PropertyMetadata(1d, OnLightingEffectSpeedRatioPropertyChanged));

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the PlanktonControl class.
        /// </summary>
        public PlanktonControl()
        {
            InitializeComponent();

            // set initial brushes
            Resources["PlanktonBrush"] = FindResource("PlanktonGreenBrush") as Brush;
            Resources["BackgroundBrush"] = FindResource("BlueBlackBackgroundBrush") as Brush;
            Resources["SeaBedBrush"] = FindResource("GraySeaBedBrush") as Brush;

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            zoomPreviewFactor = MaximumZoom;
            var bubbleStroke = new SolidColorBrush(Color.FromArgb(165, Colors.SteelBlue.R, Colors.SteelBlue.G, Colors.SteelBlue.B));
            bubblePen = new Pen(bubbleStroke, 0d);
        }

        /// <summary>
        /// Generate and populate a panel with elements.
        /// </summary>
        /// <param name="elements">Specify the amount of elements to use.</param>
        /// <param name="elementDiameter">Specify the diameter of each element.</param>
        /// <param name="elementsVariation">Specify the variation in size of each element.</param>
        /// <param name="travel">Speicfy the travel each element should apply along its vector on each update.</param>
        /// <param name="minX">Specify the minimum x location to generate each element within.</param>
        /// <param name="maxX">Specify the maximum x location to generate each element within.</param>
        /// <param name="minY">Specify the minimum y location to generate each element within.</param>
        /// <param name="maxY">Specify the maximum x location to generate each element within.</param>
        /// <param name="fillBrushes">Specify a collection of brushes that will randomly assigned as the fill of the elements.</param>
        protected virtual void Populate(int elements, double elementDiameter, int elementsVariation, double travel, int minX, int maxX, int minY, int maxY, params Brush[] fillBrushes)
        {
            var variation = 0.0d;
            var useRandomFill = ((fillBrushes != null) && (fillBrushes.Length > 0));
            var brush = ((fillBrushes != null) && (fillBrushes.Length > 0)) ? fillBrushes[0] : Brushes.Black;
            var pen = new Pen(Brushes.Black, 0d);

            for (var i = 0; i < elements; i++)
            {
                if (useRandomFill)
                    brush = fillBrushes[randomGenerator.Next(0, fillBrushes.Length)];

                if (Math.Abs(elementsVariation) > 0.0)
                    variation = randomGenerator.Next(0, elementsVariation);

                plankton.Add(MoveableElement.Create(new Point(randomGenerator.Next(minX, maxX), randomGenerator.Next(minY, maxY)), (elementDiameter / 2d) - ((elementDiameter / 200d) * variation), getRandomVector(travel), pen, brush));
            }
        }

        /// <summary>
        /// Clear all current action.
        /// </summary>
        protected virtual void ClearAllCurrent()
        {
            if (update != null)
            {
                update.Stop();
                update = null;
            }

            plankton.Clear();
            childBubbles.Clear();
            elementHost.RemoveAllDrawingVisuals();
        }

        /// <summary>
        /// Get a random vector.
        /// </summary>
        /// <param name="travel">Specify the maximum travel.</param>
        /// <returns>A new randomized vector.</returns>
        private Vector getRandomVector(double travel)
        {
            var travelX = randomGenerator.Next(0, (int)(travel * 10)) / 10d;
            var travelY = travel - travelX;

            // create the initial vector with randomized positive and negative for x and y
            return new Vector(randomGenerator.Next(1, 3) % 2 == 0 ? -travelX : travelX, randomGenerator.Next(1, 3) % 2 == 0 ? -travelY : travelY);
        }

        /// <summary>
        /// Generate a random linear gradient brush.
        /// </summary>
        /// <param name="startPoint">Specify the start point of the gradient.</param>
        /// <param name="endPoint">Specify the end point of the gradient.</param>
        /// <param name="minimumSteps">The minimum amount of steps in the gradient.</param>
        /// <param name="maximumSteps">The maximum amount of steps in the gradient.</param>
        /// <returns>The randomly generated brush.</returns>
        protected virtual Brush GenerateRandomLinearGradientBrush(Point startPoint, Point endPoint, int minimumSteps, int maximumSteps)
        {
            var lGB = new LinearGradientBrush
            {
                StartPoint = startPoint,
                EndPoint = endPoint
            };

            var steps = randomGenerator.Next(minimumSteps, maximumSteps);
            double segmentArea;

            if (steps > 2)
                segmentArea = 1.0d / (steps - 1);
            else
                segmentArea = double.NaN;

            var currentR = 0;
            var currentG = 0;
            var currentB = 0;

            int variance;

            if (steps > 1)
                variance = 256 / (steps - 1);
            else
                variance = 256;

            for (var index = 0; index < steps; index++)
            {
                if (index == 0)
                {
                    currentR = randomGenerator.Next(0, 256);
                    currentG = randomGenerator.Next(0, 256);
                    currentB = randomGenerator.Next(0, 256);
                }
                else
                {
                    currentR = randomGenerator.Next(Math.Max(currentR - variance, 0), Math.Min(currentR + variance, 256));
                    currentG = randomGenerator.Next(Math.Max(currentG - variance, 0), Math.Min(currentG + variance, 256));
                    currentB = randomGenerator.Next(Math.Max(currentB - variance, 0), Math.Min(currentB + variance, 256));
                }

                if (index == 0)
                    lGB.GradientStops.Add(new GradientStop(Color.FromArgb(255, (byte)currentR, (byte)currentG, (byte)currentB), 0.0d));
                else if (index == steps - 1)
                    lGB.GradientStops.Add(new GradientStop(Color.FromArgb(255, (byte)currentR, (byte)currentG, (byte)currentB), 1.0d));
                else
                    lGB.GradientStops.Add(new GradientStop(Color.FromArgb(255, (byte)currentR, (byte)currentG, (byte)currentB), segmentArea * index));
            }

            return lGB;
        }

        /// <summary>
        /// Handle showing the draw sea bed window.
        /// </summary>
        /// <param name="scale">The scale to display the drawing window at compared to the current area.</param>
        public virtual void OnShowDrawSeaBedWindow(double scale)
        {
            var isPaused = IsPaused;

            var drawSeaBedWindow = new DrawSeaBedWindow(new Size(ActualWidth * scale, ActualHeight * scale))
            {
                DrawingControl =
                {
                    SeaBackground = FindResource("BackgroundBrush") as Brush,
                    SeaBedBackground = FindResource("SeaBedBrush") as Brush,
                    SeaBedStroke = seaBedPath.Stroke
                }
            };

            var parent = Parent;

            while ((parent != null) && (!(parent is Window)))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
                drawSeaBedWindow.Owner = parent as Window;

            IsPaused = true;

            drawSeaBedWindow.Closed += (sender, e) => IsPaused = isPaused;

            drawSeaBedWindow.GeomertyAccepted += (sender, e) =>
            {
                var seaBedData = drawSeaBedWindow.DrawingControl.GenerateScalledGeometry(scale).ToString();
                RenderSeaBed(seaBedData);
                RegenerateWithCurrentSettings(true);
            };

            drawSeaBedWindow.ShowDialog();
        }

        /// <summary>
        /// Handle breaking out options to an external window.
        /// </summary>
        /// <returns>The ExternalOptionsWindow the options are broken out into if this is possible, else false.</returns>
        public virtual ExternalOptionsWindow OnBreakoutOptionsToExternalWindow()
        {
            try
            {
                if (!IsOptionsBreakoutViable)
                    return null;

                var externalOptionsWindow = new ExternalOptionsWindow { DataContext = DataContext };

                foreach (CommandBinding binding in CommandBindings)
                    externalOptionsWindow.CommandBindings.Add(binding);

                if (!optionsExpander.IsExpanded)
                    optionsExpander.IsExpanded = true;

                var control = optionsExpander;

                IsOptionsBreakoutViable = false;
                layoutGrid.Children.Remove(control);
                externalOptionsWindow.Content = control;

                externalOptionsWindow.Closing += (innerSender, innerE) =>
                {
                    var window = innerSender as ExternalOptionsWindow;

                    var expanderControl = window?.Content as FrameworkElement;

                    if (window != null)
                        window.Content = null;

                    if (expanderControl != null)
                        layoutGrid.Children.Add(expanderControl);

                    IsOptionsBreakoutViable = true;
                };

                externalOptionsWindow.Show();

                return externalOptionsWindow;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception caught breaking out options to external window: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Stop the lighting effect.
        /// </summary>
        protected virtual void StopLightingEffect()
        {
            var sb = FindResource("lightingEffectStoryboard") as Storyboard;
            if ((sb != null) && (sb.GetCurrentState() == ClockState.Active))
                sb.Stop();
        }

        /// <summary>
        /// Start the lighting effect.
        /// </summary>
        protected virtual void StartLightingEffect()
        {
            var sb = FindResource("lightingEffectStoryboard") as Storyboard;
            sb?.Begin();
        }

        /// <summary>
        /// Reset the lighting effect.
        /// </summary>
        protected virtual void ResetLightingEffect()
        {
            StopLightingEffect();
            StartLightingEffect();
        }

        /// <summary>
        /// Generate a Z step for a current.
        /// </summary>
        /// <param name="strength">Specify the strength of the step.</param>
        /// <param name="variationAsPercentage">Specify a random variation to apply to the step represented as a percentage.</param>
        /// <param name="maximum">The maximum overall step in value.</param>
        /// <param name="minimum">The minimum overall step out value.</param>
        /// <returns>A generate Z step to apply with a current.</returns>
        protected virtual double GenerateZStepForCurrent(double strength, double variationAsPercentage, double maximum, double minimum)
        {
            double zDirection;

            // adjust strength to be in a sensible range 
            strength *= 0.01;

            if (currentZAdjustemnt >= maximum)
                zDirection = -1;
            else if (currentZAdjustemnt <= minimum)
                zDirection = 1;
            else
            {
                if (currentZAdjustemnt > 0)
                    zDirection = randomGenerator.Next(0, 3) % 2 == 0 ? -1 : 1;
                else if (currentZAdjustemnt < 0)
                    zDirection = 1;
                else
                    zDirection = randomGenerator.Next(0, 2) % 2 == 0 ? -1 : 1;
            }

            return (strength - ((strength / 100d) * randomGenerator.Next(0, (int)variationAsPercentage))) * zDirection;
        }

        /// <summary>
        /// Trigger a current immediately.
        /// </summary>
        /// <param name="current">Specify the current to trigger.</param>
        public virtual void TriggerCurrent(Current current)
        {
            ActiveCurrent = current;
            current.Start();
        }

        /// <summary>
        /// Regenerate all elements with the current settings.
        /// </summary>
        protected virtual void RegenerateWithCurrentSettings()
        {
            RegenerateWithCurrentSettings(true);
        }

        /// <summary>
        /// Regenerate all elements with the current settings.
        /// </summary>
        /// <param name="maintainAnyGeneratedBrushes">Specify if any currently generated brushes ar maintained.</param>
        protected virtual void RegenerateWithCurrentSettings(bool maintainAnyGeneratedBrushes)
        {
            ClearAllCurrent();

            ActiveCurrent = new Current(0, 0)
            {
                MaximumZAdjustment = elementsSizeSlider.Maximum / ElementsSize,
                MinimumZAdjustment = -(ElementsSize - ((ElementsSize / 100d) * ElementsSizeVariation)) + 0.5d
            };

            currentZAdjustemnt = 0.0d;

            var area = elementHost;

            if (!area.IsMouseOver)
                bubble = null;

            lastGeneratedPlanktonBrushes = getPlanktonBrushesFromCurrentSettings(maintainAnyGeneratedBrushes);

            if ((GenerateAndUseRandomSeaBrush) && (!OverrideBackgroundBrushWithCustom))
            {
                if ((maintainAnyGeneratedBrushes) && (lastGeneratedBackgroundBrush != null))
                    Resources["BackgroundBrush"] = lastGeneratedBackgroundBrush;
                else
                {
                    var brush = GenerateRandomLinearGradientBrush(new Point(randomGenerator.Next(0, 100) / 100d, randomGenerator.Next(0, 100) / 100d), new Point(randomGenerator.Next(0, 100) / 100d, randomGenerator.Next(0, 100) / 100d), 1, randomGenerator.Next(2, UseEfficientValuesWhenRandomGenerating ? 32 : 128));
                    Resources["BackgroundBrush"] = brush;
                    lastGeneratedBackgroundBrush = brush;
                }
            }

            if ((GenerateAndUseRandomSeaBedBrush) && (UseSeaBed) && (!OverrideSeaBedBrushWithCustom))
            {
                if ((maintainAnyGeneratedBrushes) && (lastGeneratedSeaBedBrush != null))
                    Resources["SeaBedBrush"] = lastGeneratedSeaBedBrush;
                else
                {
                    var brush = GenerateRandomLinearGradientBrush(new Point(0.5d, 0.0d), new Point(0.5d, 1.0d), 2, 4);
                    Resources["SeaBedBrush"] = brush;
                    lastGeneratedSeaBedBrush = brush;
                }
            }

            var seaBedBounds = seaBedPath?.Data?.GetRenderBounds(new Pen(seaBedPath.Stroke, seaBedPath.StrokeThickness)) ?? new Rect(0, area.ActualHeight, area.ActualWidth, 0);
            var space = new Size(area.ActualWidth, Math.Max(0d, area.ActualHeight - seaBedBounds.Height));
            var radius = Math.Min(ElementsSize / 2d, Math.Min(space.Height, space.Width));
            Populate(Elements, ElementsSize, (int)ElementsSizeVariation, Travel / 10d, (int)radius, Math.Max((int)(area.ActualWidth - radius), (int)radius), (int)radius, Math.Max((int)(space.Height - radius), (int)radius), lastGeneratedPlanktonBrushes);
            var planktonElements = Elements;
            int currentBubbleElements;
            MoveableElement planktonElement;
            MoveableElement bubbleElement;
            bool useSeaBed;
            bool usePlanktonAttraction;
            bool invertPlanktonAttraction;
            bool planktonAttractToChildBubbles;
            double planktonAttractionStrength;
            double planktonAttractionReach;
            Vector planktonVector;
            Point mousePosition;
            var bubbleElementRectangle = new Rect();
            var planktonElementRectangle = new Rect();
            double actualTravel;
            int mainBubbleCollisions;
            double childBubbleBuoyancy;
            double currentZoom;
            double linearDecelerationValue;
            MoveableElement closestBubble;
            double closestBubbleProximity;
            double bubbleProximity;
            bool usePreview;
            bool useAutoPan;
            double maxAutoPanZoom;
            double minAutoPanZoom;
            double autoPanSpeed;
            double autoPanSensitity;
            double planktonLife;
            var seaBedHeight = seaBedGeometry?.Bounds.Height ?? 0d;
            Vector originalVectorOfPlanktonElement;
            double areaHeight;
            double areaWidth;
            var bubbleBrush = FindResource("BubbleBrush") as Brush;
            DrawingVisual planktonHostVisual;
            DrawingVisual bubbleHostVisual;
            DrawingVisual mainBubbleHostVisual = null;
            var centerPointOfElement = new Point(0, 0);
            var lastRenderdPlankton = 0;
            var lastRenderedBubbleElements = 0;
            var lastMainBubbleState = false;
            Point focusPointForPreview;
            var forceBubbleRerender = false;
            var slowestRefresh = refreshSlider.Maximum;
            Geometry seaBed;
            var consecutiveUpdateFails = 0;
            var useCurrent = false;
            double currentRate;
            Current current;
            bool useCurrentChanged;
            bool showLocatorLine;
            bool useZOnCurrent;
            CurrentSwellStage currentMode;
            IValueConverter currentIndicatorOpacityConverter = new CurrentToOpacityConverter();
            var culture = CultureInfo.CurrentCulture;
            bool useZoomPreviewBlurEffect;
            BlurEffect blurEffect;
            var zoomPreviewBlurCorrectionFactor = 0.0d;
            bool useGravity;
            double density;
            double massOfPlanktonElement;
            double effectOfMassOfPlanktonElement;
            double maxElementMass;
            double maxBubbleSize;
            double normalisedMassComparedToBiggest;
            double normalisedBubbleSizeComparedToBiggest;
            bool strokeContainsElement;
            bool fillContainsElement;

            // define update callback for timer
            EventHandler updateCallback = (s, args) =>
            {
                // if not updating already - too many elements on a slow processor could lock up if this is called too frequently
                if (IsUpdating)
                    return;
                
                var startTime = Environment.TickCount;
                IsUpdating = true;
                mousePosition = Mouse.GetPosition(area);
                actualTravel = Travel / 10d;
                mainBubbleCollisions = 0;
                childBubbleBuoyancy = ChildBubbleBuoyancy * WaterViscosity;
                currentZoom = zoomPreviewFactor;
                useSeaBed = ((seaBedGeometry != null) && (UseSeaBed));
                usePlanktonAttraction = UsePlanktonAttraction;
                invertPlanktonAttraction = InvertPlanktonAttraction;
                planktonAttractToChildBubbles = PlanktonAttractToChildBubbles;
                planktonAttractionStrength = PlanktonAttractionStrength / 10d;
                planktonAttractionReach = PlanktonAttractionReach;
                linearDecelerationValue = WaterViscosity;
                usePreview = UseZoomPreview;
                useAutoPan = UseAutoPanOnZoomPreview;
                maxAutoPanZoom = MaximumZoom;
                minAutoPanZoom = MinimumZoom;
                autoPanSpeed = AutoPanSpeed;
                autoPanSensitity = (zoomSensitivitySlider.Maximum + zoomSensitivitySlider.Minimum) - AutoPanSensitivity;
                planktonLife = Life;
                areaHeight = area.ActualHeight;
                areaWidth = area.ActualWidth;
                seaBed = seaBedGeometry;
                useCurrentChanged = UseCurrent != useCurrent;
                useCurrent = UseCurrent;
                currentRate = CurrentRate;
                current = ActiveCurrent;
                useZOnCurrent = UseZOnCurrent;
                currentMode = CurrentMode;
                useZoomPreviewBlurEffect = UseZoomPreviewBlurEffect;

                if (useZoomPreviewBlurEffect)
                {
                    blurEffect = previewAreaPresenter.Effect as BlurEffect;
                    zoomPreviewBlurCorrectionFactor = ZoomPreviewBlurCorrection;
                }
                else
                    blurEffect = null;

                useGravity = UseGravity;
                density = Density;
                maxElementMass = (ElementsSize * Math.PI) * Density;
                maxBubbleSize = (BubbleSize * Math.PI);

                if ((useCurrent) && (!current.IsActive) && (randomGenerator.Next(0, 1000) < currentRate))
                {
                    // set random current direction - just use between 20-160, and 200-340 to avoid annoying currents
                    current.Direction = UseRandomCurrentDirection ? randomGenerator.Next(0, 2) % 2 == 0 ? randomGenerator.Next(20, 160) : randomGenerator.Next(200, 340) : CurrentDirection;
                    ActiveCurrentDirection = Math.Round(current.Direction, 0d);
                    current.Strength = CurrentStrength - ((CurrentStrength / 100d) * randomGenerator.Next(0, (int)CurrentVariation));
                    current.Deceleration = IgnoreWaterViscosityWhenGeneratingCurrent ? CurrentDeceleration : linearDecelerationValue;
                    current.Acceleration = IgnoreWaterViscosityWhenGeneratingCurrent ? CurrentAcceleration : linearDecelerationValue;
                    current.ZAdjustmentPerStep = useZOnCurrent ? GenerateZStepForCurrent(CurrentZStep, CurrentZStepVariation, current.MaximumZAdjustment, current.MinimumZAdjustment) : 0.0d;
                    current.Start(currentMode);
                    IsCurrentActive = true;
                }
                else if ((useCurrent) && (current.IsActive))
                {
                    current.IncrementToNextStep();

                    if (ShowCurrentIndicator)
                        currentIndicatorMasterGrid.Opacity = UseAnimation ? (double)currentIndicatorOpacityConverter.Convert(ActiveCurrent, typeof (double), null, culture) : 1.0d;

                    currentZAdjustemnt += current.ActiveStep().Z;

                    // determine degrees - add 90 as Atan2 returns north as -90, east a 0, south as 90 and west as 180
                    ActiveCurrentDirection = Math.Round(90 + Math.Atan2(current.ActiveStep().Y, current.ActiveStep().X) * (180d / Math.PI), 0d);
                }
                else if ((useCurrent) && (!current.IsActive))
                {
                    IsCurrentActive = false;
                    ActiveCurrentDirection = Math.Round(current.Direction, 0d);
                }
                else if ((useCurrentChanged) && (current.IsActive))
                {
                    current.Stop();
                    IsCurrentActive = false;
                    ActiveCurrentDirection = Math.Round(current.Direction, 0d);
                }

                if (bubble != null)
                    bubble.Vector = mouseVector;

                // if generating child bubble - current count less than maximum, randomly generated (using 200 to half rate to a maximum of 50%), or left mouse button is down
                if ((area.IsMouseOver) &&
                    (((UseChildBubbles) && ((childBubbles.Count < MaximumChildBubbles) && ((randomGenerator.Next(0, 200) < ChildBubbleRate))) ||
                      ((Mouse.LeftButton == MouseButtonState.Pressed) && (childBubbles.Count < MaximumChildBubbles)))))
                {
                    // check not over sea bed
                    if ((useSeaBed) && (mousePosition.Y > areaHeight - seaBedHeight) && (seaBedGeometry.FillContains(mousePosition)))
                    {
                        // don't bother creating a bubble over the sea bed
                    }
                    else
                    {
                        var relativeChildBubbleDimension = BubbleSize / 2d;
                        relativeChildBubbleDimension -= (relativeChildBubbleDimension / 100d * (randomGenerator.Next(0, (int)ChildBubbleSizeVariation)));
                        childBubbles.Add(MoveableElement.Create(new Point(mousePosition.X - (relativeChildBubbleDimension / 2d), mousePosition.Y - (relativeChildBubbleDimension / 2d)), Math.Max(3, relativeChildBubbleDimension), new Vector(0d, -childBubbleBuoyancy), bubblePen, bubbleBrush), true);
                        forceBubbleRerender = true;
                    }
                }

                if (childBubbles.Count > 0)
                {
                    var bubblesToCheck = childBubbles.Keys.ToArray();
                    foreach (var childBubbleElement in bubblesToCheck)
                    {
                        normalisedBubbleSizeComparedToBiggest = (1d / maxBubbleSize) * (((childBubbleElement.Geometry.RadiusX + childBubbleElement.Geometry.RadiusY) / 2d) * Math.PI);

                        if (childBubbles[childBubbleElement])
                        {
                            if (useCurrent)
                            {
                                if ((useSeaBed) && (childBubbleElement.Geometry.Center.Y + childBubbleElement.Geometry.RadiusY + current.ActiveStep().Z + current.ActiveStep().Y - childBubbleBuoyancy > (areaHeight - seaBedHeight)) && (seaBedGeometry.FillContains(new Point(childBubbleElement.Geometry.Center.X + current.ActiveStep().Z + current.ActiveStep().X, childBubbleElement.Geometry.Center.Y + childBubbleElement.Geometry.RadiusY + current.ActiveStep().Z + current.ActiveStep().Y - childBubbleBuoyancy))))
                                {
                                    PopChildBubble(childBubbleElement);
                                    forceBubbleRerender = true;
                                }
                                else
                                {
                                    childBubbleElement.Geometry.RadiusX = Math.Max(0, Math.Min(bubbleSizeSlider.Maximum / 2d, childBubbleElement.Geometry.RadiusX + (current.ActiveStep().Z * (2d - normalisedBubbleSizeComparedToBiggest))));
                                    childBubbleElement.Geometry.RadiusY = Math.Max(0, Math.Min(bubbleSizeSlider.Maximum / 2d, childBubbleElement.Geometry.RadiusY + (current.ActiveStep().Z * (2d - normalisedBubbleSizeComparedToBiggest))));
                                    childBubbleElement.Geometry.Center = new Point(childBubbleElement.Geometry.Center.X + (current.ActiveStep().X * (2d - normalisedBubbleSizeComparedToBiggest)), childBubbleElement.Geometry.Center.Y - childBubbleBuoyancy + (current.ActiveStep().Y * (2d - normalisedBubbleSizeComparedToBiggest)));
                                }
                            }
                            else
                                childBubbleElement.Geometry.Center = new Point(childBubbleElement.Geometry.Center.X, childBubbleElement.Geometry.Center.Y - childBubbleBuoyancy);

                            // if bubble is still valid after current modification
                            if (!childBubbles[childBubbleElement])
                                continue;
                            
                            updateRectangle(ref bubbleElementRectangle, childBubbleElement.Geometry);

                            if (useSeaBed)
                            {
                                if ((childBubbleElement.Vector.Y < 0) || (current.ActiveStep().Y < 0))
                                    if (bubbleElementRectangle.Y + bubbleElementRectangle.Height <= 0)
                                        PopChildBubble(childBubbleElement);
                                    else if ((childBubbleElement.Vector.Y > 0) || (current.ActiveStep().Y > 0))
                                    {
                                        if (bubbleElementRectangle.Y > areaHeight)
                                        {
                                            PopChildBubble(childBubbleElement);
                                            forceBubbleRerender = true;
                                        }
                                        else if ((bubbleElementRectangle.Y + bubbleElementRectangle.Height >= (areaHeight - seaBedHeight)) &&
                                                 (seaBed.StrokeContainsWithDetail(seaBedPen, childBubbleElement.Geometry) != IntersectionDetail.Empty) ||
                                                 (seaBed.FillContainsWithDetail(childBubbleElement.Geometry) != IntersectionDetail.Empty))
                                        {
                                            PopChildBubble(childBubbleElement);
                                            forceBubbleRerender = true;
                                        }
                                    }
                            }
                            else
                            {
                                // if going up and off top of screen, or going down and off bottom of screen
                                if ((((childBubbleElement.Vector.Y < 0) || (current.ActiveStep().Y < 0)) && (bubbleElementRectangle.Y + bubbleElementRectangle.Height <= 0)) || (((childBubbleElement.Vector.Y > 0) || (current.ActiveStep().Y > 0)) && (bubbleElementRectangle.Y > areaHeight)))
                                    PopChildBubble(childBubbleElement);
                            }
                        }
                    }

                    var invalidBubbles = childBubbles.Keys.Where(x => !childBubbles[x]).ToArray();

                    if (invalidBubbles.Length > 0)
                        foreach (var eL in invalidBubbles)
                            childBubbles.Remove(eL);

                    currentBubbleElements = childBubbles.Count;
                }
                else
                    currentBubbleElements = 0;

                for (var planktonIndex = 0; planktonIndex < planktonElements; planktonIndex++)
                {
                    strokeContainsElement = false;
                    fillContainsElement = false;
                    planktonElement = plankton[planktonIndex];
                    planktonVector = planktonElement.Vector;
                    originalVectorOfPlanktonElement = planktonVector;
                    centerPointOfElement.X = planktonElement.Geometry.Center.X;
                    centerPointOfElement.Y = planktonElement.Geometry.Center.Y;
                    massOfPlanktonElement = (((planktonElement.Geometry.RadiusX + planktonElement.Geometry.RadiusY) / 2d) * Math.PI) * density;
                    effectOfMassOfPlanktonElement = ((massOfPlanktonElement / 1000d) * linearDecelerationValue);
                    normalisedMassComparedToBiggest = (1d / maxElementMass) * massOfPlanktonElement;

                    // if using current and no sea bed, or element is above sea bed - sea bed shelters from current
                    if ((useCurrent) && ((!useSeaBed) || (planktonElement.Geometry.Center.Y + planktonElement.Geometry.RadiusY < (areaHeight - seaBedHeight))))
                    {
                        planktonElement.Geometry.RadiusX = Math.Max(0, Math.Min(elementsSizeSlider.Maximum / 2d, planktonElement.Geometry.RadiusX + (current.ActiveStep().Z * (2d - normalisedMassComparedToBiggest))));
                        planktonElement.Geometry.RadiusY = Math.Max(0, Math.Min(elementsSizeSlider.Maximum / 2d, planktonElement.Geometry.RadiusY + (current.ActiveStep().Z * (2d - normalisedMassComparedToBiggest))));
                        centerPointOfElement.X = Math.Max(0 + planktonElement.Geometry.RadiusX, Math.Min(area.ActualWidth - planktonElement.Geometry.RadiusX, planktonElement.Geometry.Center.X + (current.ActiveStep().X * (2d - normalisedMassComparedToBiggest))));
                        centerPointOfElement.Y = Math.Max(0 + planktonElement.Geometry.RadiusY, Math.Min(area.ActualHeight - planktonElement.Geometry.RadiusY, planktonElement.Geometry.Center.Y + (current.ActiveStep().Y * (2d - normalisedMassComparedToBiggest))));
                        planktonElement.Geometry.Center = centerPointOfElement;
                    }

                    updateRectangle(ref planktonElementRectangle, planktonElement.Geometry);

                    if ((Math.Abs(planktonVector.X) + (Math.Abs(planktonVector.Y)) > actualTravel))
                    {
                        // reduce by unit - linear deceleration
                        planktonVector.X *= linearDecelerationValue;
                        planktonVector.Y *= linearDecelerationValue;
                    }

                    closestBubble = null;
                    closestBubbleProximity = int.MaxValue;

                    for (var bubbleIndex = bubble != null ? -1 : 0; bubbleIndex < currentBubbleElements; bubbleIndex++)
                    {
                        bubbleElement = bubbleIndex == -1 ? bubble : childBubbles.Keys.ElementAt(bubbleIndex);
                        updateRectangle(ref bubbleElementRectangle, bubbleElement.Geometry);

                        // if plankton is fully inside the bubble
                        if (DoRegularCirclesOverlap(bubbleElementRectangle.Left, bubbleElementRectangle.Top, bubbleElementRectangle.Width / 2d, planktonElementRectangle.Left, planktonElementRectangle.Top, planktonElementRectangle.Width / 2d))
                        {
                            // get angle from center of bubble to center of area
                            var angle = Math.Round(90 + Math.Atan((areaWidth / 2d) - (planktonElementRectangle.X + (planktonElementRectangle.Width / 2d))) / ((areaHeight / 2d) - (planktonElementRectangle.Y + (planktonElementRectangle.Height / 2d))) * (180d / Math.PI), 0d);

                            // specify ejection vector
                            planktonVector.X = (actualTravel * 5d) * Math.Sin(angle);
                            planktonVector.Y = (actualTravel * 5d) * Math.Cos(angle);
                        }
                        else if (DoRegularCirclesIntersect(bubbleElementRectangle.Left, bubbleElementRectangle.Top, bubbleElementRectangle.Width, bubbleElementRectangle.Height, planktonElementRectangle.Left, planktonElementRectangle.Top, planktonElementRectangle.Width, planktonElementRectangle.Height))
                        {
                            // if in left side of bubble and moving left or right side of bubble and moving right
                            if ((planktonElementRectangle.Left + (planktonElementRectangle.Width / 2d) <= bubbleElementRectangle.Left + (bubbleElementRectangle.Width / 2d) && (planktonVector.X > 0)) || (planktonElementRectangle.Left + (planktonElementRectangle.Width / 2d) > bubbleElementRectangle.Left + (bubbleElementRectangle.Width / 2d) && (planktonVector.X < 0)))
                                planktonVector.X = -planktonVector.X;
                            else if (Math.Abs(bubbleElement.Vector.X) > 0.0)
                                planktonVector.X = bubbleElement.Vector.X;

                            // if in top side of bubble and moving down or bottom side of bubble and moving up
                            if ((planktonElementRectangle.Top + (planktonElementRectangle.Height / 2d) <= bubbleElementRectangle.Top + (bubbleElementRectangle.Height / 2d) && (planktonVector.Y > 0)) || (planktonElementRectangle.Top + (planktonElementRectangle.Height / 2d) > bubbleElementRectangle.Top + (bubbleElementRectangle.Height / 2d) && (planktonVector.Y < 0)))
                                planktonVector.Y = -planktonVector.Y;
                            else if (Math.Abs(bubbleElement.Vector.Y) > 0.0)
                                planktonVector.Y = bubbleElement.Vector.Y;

                            // if first, therefore main bubble increment collisions
                            if (bubbleElement == bubble)
                                mainBubbleCollisions++;

                            break;
                        }
                        else if ((usePlanktonAttraction) &&
                                 ((bubbleElement == bubble) || ((planktonAttractToChildBubbles) && (bubbleElement != bubble))))
                        {
                            bubbleProximity = DetermineDistanceBetweenTwoPoints(bubbleElementRectangle.Left + (bubbleElementRectangle.Width / 2d), bubbleElementRectangle.Top + (bubbleElementRectangle.Height / 2d), planktonElementRectangle.Left + (planktonElementRectangle.Width / 2d), planktonElementRectangle.Top + (planktonElementRectangle.Height / 2d));

                            // else if within radius of attraction
                            if (bubbleProximity <= (bubbleElementRectangle.Width / 2d) * planktonAttractionReach)
                            {
                                // if closer than previous closest bubble, or no previous close bubble
                                if ((closestBubble == null) || (closestBubbleProximity > bubbleProximity))
                                {
                                    closestBubble = bubbleElement;
                                    closestBubbleProximity = bubbleProximity;
                                }
                            }
                        }

                        // if a closest bubble found
                        if (closestBubble == null)
                            continue;

                        updateRectangle(ref bubbleElementRectangle, closestBubble.Geometry);
                        planktonVector.X = Math.Min(Math.Max(Math.Abs((bubbleElementRectangle.Left + (bubbleElementRectangle.Width / 2d)) - (planktonElementRectangle.Left + (planktonElementRectangle.Width / 2d))) - ((planktonElementRectangle.Width / 2d) + (bubbleElementRectangle.Width / 2d)), 0), Math.Max(actualTravel, planktonAttractionStrength)) * ((bubbleElementRectangle.Left + (bubbleElementRectangle.Width / 2d) < planktonElementRectangle.Left + (planktonElementRectangle.Width / 2d)) ? -1 : 1) * (invertPlanktonAttraction ? -1 : 1);
                        planktonVector.Y = Math.Min(Math.Max(Math.Abs((bubbleElementRectangle.Top + (bubbleElementRectangle.Height / 2d)) - (planktonElementRectangle.Top + (planktonElementRectangle.Height / 2d))) - ((planktonElementRectangle.Height / 2d) + (bubbleElementRectangle.Height / 2d)), 0), Math.Max(actualTravel, planktonAttractionStrength)) * ((bubbleElementRectangle.Top + (bubbleElementRectangle.Height / 2d) < planktonElementRectangle.Top + (planktonElementRectangle.Height / 2d)) ? -1 : 1) * (invertPlanktonAttraction ? -1 : 1);
                    }

                    while (bubbleCollisionHistory.Count == 10)
                        bubbleCollisionHistory.Dequeue();

                    bubbleCollisionHistory.Enqueue(mainBubbleCollisions);

                    // if using life, vector hasn't changed, and random generator decides that vector should be changed, and not moving at an accelerated speed
                    if ((planktonLife > 0) && ((Math.Abs(originalVectorOfPlanktonElement.X - planktonVector.X) < 0.0) && (Math.Abs(originalVectorOfPlanktonElement.Y - planktonVector.Y) < 0.0)) && (randomGenerator.Next(1, 100) <= planktonLife) && (planktonVector.Length <= actualTravel))
                        planktonVector = getRandomVector(actualTravel);

                    if (useGravity)
                        planktonVector.Y += effectOfMassOfPlanktonElement;

                    // if off left of screen - for example a resize or error ocured
                    if (planktonElementRectangle.Left + planktonElementRectangle.Width < 0)
                    {
                        // set center point so elements just off of screen
                        centerPointOfElement.X = -(planktonElementRectangle.Width / 2d);

                        // set vector to make element come back on to screen
                        planktonVector.X = Math.Max(5d, actualTravel);

                        // apply center to element
                        planktonElement.Geometry.Center = centerPointOfElement;
                    }
                    else if (planktonElementRectangle.Left > area.ActualWidth)
                    {
                        // set center point so elements just off of screen
                        centerPointOfElement.X = area.ActualWidth + (planktonElementRectangle.Width / 2d);

                        // set vector to make element come back on to screen
                        planktonVector.X = -Math.Max(5d, actualTravel);

                        // apply center to element
                        planktonElement.Geometry.Center = centerPointOfElement;
                    }
                    else if (((planktonElementRectangle.X + planktonElementRectangle.Width + planktonVector.X > areaWidth) && ((planktonVector.X > 0) || (current.ActiveStep().X > 0))) || ((planktonElementRectangle.X + planktonVector.X < 0) && ((planktonVector.X < 0) || (current.ActiveStep().X < 0))))
                    {
                        // colliding with left or right of area

                        // invert vector x
                        planktonVector.X = -planktonVector.X;
                    }

                    // if off top of screen - for example a resize or error ocured
                    if (planktonElementRectangle.Top + planktonElementRectangle.Height < 0)
                    {
                        // set center point so elements just off of screen
                        centerPointOfElement.Y = -(planktonElementRectangle.Height / 2d);

                        // set vector to make element come back on to screen
                        planktonVector.Y = Math.Max(5d, actualTravel);

                        // apply center to element
                        planktonElement.Geometry.Center = centerPointOfElement;
                    }
                    else if (planktonElementRectangle.Top > area.ActualHeight)
                    {
                        // set center point so elements just off of screen
                        centerPointOfElement.Y = area.ActualHeight + (planktonElementRectangle.Height / 2d);

                        // set vector to make element come back on to screen
                        planktonVector.Y = -Math.Max(5d, actualTravel);

                        // apply center to element
                        planktonElement.Geometry.Center = centerPointOfElement;
                    }
                    else if (((planktonElementRectangle.Y + planktonElementRectangle.Height + planktonVector.Y > areaHeight) && ((planktonVector.Y > 0) || (current.ActiveStep().Y > 0))) || ((planktonElementRectangle.Y + planktonVector.Y < 0) && ((planktonVector.Y < 0) || (current.ActiveStep().Y < 0))))
                    {
                        // colliding with top or bottom of area

                        if (useGravity)
                            planktonVector.Y = planktonElementRectangle.Y + (planktonElementRectangle.Height / 2d) >= (areaHeight / 2d) ? 0d : effectOfMassOfPlanktonElement;
                        else
                            planktonVector.Y = -planktonVector.Y;
                    }

                    // if using sea bed and in a collision range
                    if ((useSeaBed) && (planktonElementRectangle.Top + planktonElementRectangle.Height >= (areaHeight - seaBedHeight)))
                    {
                        fillContainsElement = seaBed.FillContainsWithDetail(planktonElement.Geometry) != IntersectionDetail.Empty;
                        strokeContainsElement = seaBed.StrokeContainsWithDetail(seaBedPen, planktonElement.Geometry) != IntersectionDetail.Empty;

                        // if fill contains element - i.e somewhere there is a glitch and the element has become embedded in the sea bed
                        if ((fillContainsElement) && (!strokeContainsElement))
                        {
                            // encpsulated within the sea bed, uh oh, eject!

                            centerPointOfElement.X = planktonElement.Geometry.Center.X + planktonVector.X;
                            centerPointOfElement.Y = areaHeight - seaBedHeight - (planktonElementRectangle.Height / 2d);
                            planktonElement.Geometry.Center = centerPointOfElement;
                            planktonVector.X = 0d;
                            planktonVector.Y = -Math.Max(Math.Abs(planktonVector.Y), actualTravel);
                        }
                        else if (strokeContainsElement)
                        {
                            // if collided with the sea bed

                            planktonVector.X = -planktonVector.X;

                            if (useGravity)
                                planktonVector.Y = planktonVector.Y < 0d ? planktonVector.Y : Math.Abs(planktonVector.Y) > 0.0 ? -(Math.Abs(planktonVector.Y * (1.1d - normalisedMassComparedToBiggest))) : 0d;
                            else
                                planktonVector.Y = -Math.Abs(planktonVector.Y);
                        }
                    }

                    planktonElement.Vector = planktonVector;
                    centerPointOfElement.X = planktonElement.Geometry.Center.X + planktonVector.X;
                    centerPointOfElement.Y = planktonElement.Geometry.Center.Y + planktonVector.Y;
                    planktonElement.Geometry.Center = centerPointOfElement;
                }

                if (usePreview)
                {
                    if (useZoomPreviewBlurEffect)
                    {
                        if (blurEffect != null)
                        {
                            lock (blurEffect)
                            {
                                if (Math.Abs(blurEffect.Radius - nextZoomPreviewBlurRadius) > 0.0)
                                    blurEffect.Radius = nextZoomPreviewBlurRadius;

                                if (nextZoomPreviewBlurRadius > 0.0d)
                                    nextZoomPreviewBlurRadius = Math.Max(0, nextZoomPreviewBlurRadius - zoomPreviewBlurCorrectionFactor);
                            }
                        }
                    }

                    if (useAutoPan)
                    {
                        var averageCollision = bubbleCollisionHistory.Average(i => i);

                        if (averageCollision > 7d * autoPanSensitity)
                            zoomPreviewVector.Z = currentZoom < minAutoPanZoom ? autoPanSpeed : 0.0d;
                        else if (averageCollision > 5d * autoPanSensitity)
                            zoomPreviewVector.Z = currentZoom < 4d ? autoPanSpeed : 0.0d;
                        else if (averageCollision > 3d * autoPanSensitity)
                            zoomPreviewVector.Z = currentZoom < 3d ? autoPanSpeed : 0.0d;
                        else
                            zoomPreviewVector.Z = currentZoom > minAutoPanZoom ? -autoPanSpeed : 0.0d;
                    }
                    else
                        zoomPreviewVector.Z = currentZoom > maxAutoPanZoom ? -autoPanSpeed : 0.0d;

                    if ((Math.Abs(zoomPreviewVector.Z) > 0.0) && (bubble != null))
                    {
                        zoomPreviewFactor += zoomPreviewVector.Z;
                        UpdateZoomPreview(mousePosition, GetPreviewFocusElement(ZoomPreviewLocatorMode, out showLocatorLine), area, zoomPreviewFactor, showLocatorLine);
                    }
                    else if (IfMainBubbleNotAvailablePreviewMostInterestingElement)
                    {
                        // using alternate focus and main bubble has become hidden

                        zoomPreviewFactor = MinimumZoom;
                        var focusElement = GetPreviewFocusElement(ZoomPreviewLocatorMode, out showLocatorLine);
                        focusPointForPreview = focusElement?.Geometry.Center ?? new Point(areaWidth / 2d, areaHeight / 2d);
                        UpdateZoomPreview(focusPointForPreview, focusElement, area, zoomPreviewFactor, showLocatorLine);
                    }
                }

                if (planktonElements != lastRenderdPlankton)
                {
                    lastRenderdPlankton = plankton.Count;
                    planktonHostVisual = new DrawingVisual();
                    elementHost.SpecifyPlanktonDrawingVisual(planktonHostVisual);
                    elementHost.AddPlanktonElements(plankton.ToArray());
                }

                // if forcing bubble rerender, new bubbles added, or got down to no rendered bubbles but there were before - we don't want to re render everything when a bubble goes off screen
                if ((forceBubbleRerender) || ((currentBubbleElements == 0) && (lastRenderedBubbleElements > 0)))
                {
                    lastRenderedBubbleElements = currentBubbleElements;
                    bubbleHostVisual = new DrawingVisual();
                    elementHost.SpecifyBubbleDrawingVisual(bubbleHostVisual);
                    elementHost.AddBubbleElements(childBubbles.Keys.ToArray());
                    forceBubbleRerender = false;
                }

                if (bubble != null)
                {
                    lock (bubble)
                    {
                        if ((mainBubbleHostVisual == null) || ((bubble != null) && (!lastMainBubbleState)))
                        {
                            mainBubbleHostVisual = new DrawingVisual();
                            elementHost.SpecifyMainBubbleDrawingVisual(mainBubbleHostVisual);
                            elementHost.AddMainBubbleElement(bubble);
                        }
                    }
                }
                else if (lastMainBubbleState)
                {
                    // need to remove the main bubble visusal to not leave a hanging element
                    mainBubbleHostVisual = new DrawingVisual();
                    elementHost.SpecifyMainBubbleDrawingVisual(mainBubbleHostVisual);
                }

                lastMainBubbleState = bubble != null;
                LastRefreshTime = Environment.TickCount - startTime;

                if (LastRefreshTime > slowestRefresh)
                    consecutiveUpdateFails++;
                else
                    consecutiveUpdateFails = 0;

                if (consecutiveUpdateFails == 10)
                {
                    IsPaused = true;
                    var r = MessageBox.Show("The amount of elements being rendered is set too high for the processor to handle smoothly. Please try reducing the amount of plankton elements, turning child bubbles off and using the 'Dark' background and 'Quick' plankton colour, with no sea bed. Do you want to default to these options now?", "Process Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (r == MessageBoxResult.Yes)
                    {
                        Elements = 100;
                        UseChildBubbles = false;
                        performancePlanktonRadioButton.IsChecked = true;
                        UseRandomElementFill = false;
                        UseSeaBed = false;
                        darkBackgroundRadioButton.IsChecked = true;
                        OverrideBackgroundBrushWithCustom = false;
                        OverridePlanktonBrushWithCustom = false;
                        OverrideSeaBedBrushWithCustom = false;

                        RegenerateWithCurrentSettings(maintainAnyGeneratedBrushes);
                    }

                    IsPaused = false;
                }

                ActiveChildBubbles = currentBubbleElements;
                MainBubbleCollisionsThisUpdate = mainBubbleCollisions;
                IsUpdating = false;
            };

            update = new DispatcherTimer(TimeSpan.FromMilliseconds(RefreshTime), DispatcherPriority.Send, updateCallback, Dispatcher) { IsEnabled = !IsPaused };

            if (!IsPaused)
                update.Start();
        }

        /// <summary>
        /// Update the zoom preview element locator lines location.
        /// </summary>
        /// <param name="element">The element to locate.</param>
        protected virtual void UpdateZoomPreviewLocatorLinePositions(MoveableElement element)
        {
            if (element == null)
                return;

            previewToElementLine.X1 = element.Geometry.Center.X;
            previewToElementLine.Y1 = element.Geometry.Center.Y;
            previewToElementLine.X2 = areaCanvas.ActualWidth - previewBorder.Margin.Right - (previewBorder.ActualWidth / 2d);
            previewToElementLine.Y2 = previewBorder.Margin.Top + (previewBorder.ActualHeight / 2d);
        }

        /// <summary>
        /// Get a MoveableElement for the zoom preview to focus on.
        /// </summary>
        /// <param name="locatorMode">Specify the locator mode to use for the zoom preview.</param>
        /// <param name="showLocator">Returns if the locator should be shown.</param>
        /// <returns>The logical element for the focus to remain on.</returns>
        protected virtual MoveableElement GetPreviewFocusElement(ZoomPreviewLocaterMode locatorMode, out bool showLocator)
        {
            if (bubble != null)
            {
                zoomPreviewFocusedPlankton = null;

                switch (locatorMode)
                {
                    case (ZoomPreviewLocaterMode.Always):
                    case (ZoomPreviewLocaterMode.AnythingButPlankton):
                    case (ZoomPreviewLocaterMode.OnlyMainBubble):
                        showLocator = true;
                        break;
                    default:
                        showLocator = false;
                        break;
                }

                return bubble;
            }

            if (childBubbles.Count > 0)
            {
                lock (childBubbles)
                {
                    for (var index = 0; index < childBubbles.Keys.Count; index++)
                    {
                        if (!(childBubbles.Keys.ElementAt(index).Geometry.Bounds.Width >= 3))
                            continue;

                        zoomPreviewFocusedPlankton = null;

                        switch (locatorMode)
                        {
                            case (ZoomPreviewLocaterMode.Always):
                            case (ZoomPreviewLocaterMode.AnythingButMainBubble):
                            case (ZoomPreviewLocaterMode.AnythingButPlankton):
                            case (ZoomPreviewLocaterMode.OnlyChildBubbles):
                                showLocator = true;
                                break;
                            default:
                                showLocator = false;
                                break;
                        }

                        return childBubbles.Keys.ElementAt(index);
                    }
                }
            }

            // try plankton elements - try and used the focused plankton, if it's staionary or valid any more find the next fastest
            if (((zoomPreviewFocusedPlankton == null) || (!plankton.Contains(zoomPreviewFocusedPlankton)) || (Math.Abs(zoomPreviewFocusedPlankton.Vector.Length) < 0.0)) && (plankton.Count > 0))
            {
                MoveableElement fastestPlankton = null;

                lock (plankton)
                {
                    foreach (var eL in plankton)
                    {
                        // if no previous plankton or faster found - exclude ones smaller than 1 as these mess the preview up
                        if ((eL.Geometry.Bounds.Width >= 1) && ((fastestPlankton == null) || (eL.Vector.Length > fastestPlankton.Vector.Length)))
                            fastestPlankton = eL;
                    }
                }

                zoomPreviewFocusedPlankton = fastestPlankton;

                switch (locatorMode)
                {
                    case (ZoomPreviewLocaterMode.Always):
                    case (ZoomPreviewLocaterMode.AnythingButMainBubble):
                    case (ZoomPreviewLocaterMode.OnlyPlankton):
                        showLocator = true;
                        break;
                    default:
                        showLocator = false;
                        break;
                }


                return zoomPreviewFocusedPlankton;
            }

            if (zoomPreviewFocusedPlankton != null)
            {
                switch (locatorMode)
                {
                    case (ZoomPreviewLocaterMode.Always):
                    case (ZoomPreviewLocaterMode.AnythingButMainBubble):
                    case (ZoomPreviewLocaterMode.OnlyPlankton):
                        showLocator = true;
                        break;
                    default:
                        showLocator = false;
                        break;
                }


                return zoomPreviewFocusedPlankton;
            }

            showLocator = false;
            return null;
        }

        /// <summary>
        /// Update the zoom preview.
        /// </summary>
        /// <param name="targetPoint">The point to target in the preview. If this exceeds the area bounds of the visual source it will snap to the bound(s) it exceeded.</param>
        /// <param name="target">The target element to focus on.</param>
        /// <param name="visualSource">The visual source the zoom preview relates to.</param>
        /// <param name="zoomFactor">The zoom factor to apply where 1.0d specifies that the bubble should fill the preview.</param>
        /// <param name="showLocatorLine">Specify if the locator line should be shown.</param>
        protected virtual void UpdateZoomPreview(Point targetPoint, MoveableElement target, FrameworkElement visualSource, double zoomFactor, bool showLocatorLine)
        {
            var bounds = target?.Geometry.Bounds ?? new Rect(0, 0, 100, 100);
            var targetWidth = target != null ? bounds.Width : 100;
            var targetHeight = target != null ? bounds.Height : 100;
            var left = Math.Max(0d, Math.Min(targetPoint.X - ((targetWidth / 2d) * zoomFactor), visualSource.ActualWidth - (targetWidth * zoomFactor)));
            var top = Math.Max(0d, Math.Min(targetPoint.Y - ((targetHeight / 2d) * zoomFactor), visualSource.ActualHeight - (targetHeight * zoomFactor)));
            var width = targetWidth * zoomFactor;
            var height = targetHeight * zoomFactor;

            // update viewbox to show bubble but only to edges of the area
            zoomBrush.Viewbox = new Rect(left, top, width, height);

            if (target != null)
            {
                // if underneath preview
                if ((targetPoint.X >= areaCanvas.ActualWidth - previewBorder.Margin.Right - previewBorder.ActualWidth) &&
                    (targetPoint.X <= areaCanvas.ActualWidth - areaCanvas.Margin.Right) &&
                    (targetPoint.Y >= previewBorder.Margin.Top) &&
                    (targetPoint.Y <= previewBorder.Margin.Top + previewBorder.ActualHeight))
                {
                    IsZoomPreviewLocatorVisible = false;
                    previewBorder.Opacity = 0.25d;
                }
                else
                {
                    previewBorder.Opacity = 1.0d;

                    if (showLocatorLine)
                        UpdateZoomPreviewLocatorLinePositions(target);

                    IsZoomPreviewLocatorVisible = showLocatorLine;
                }

                if (UseZoomPreviewBlurEffect)
                    nextZoomPreviewBlurRadius = Math.Max(nextZoomPreviewBlurRadius, Math.Min(MaximumZoomPreviewBlur, DetermineDistanceBetweenTwoPoints(lastZoomPreviewVisualLocation, targetPoint) / (15d - ZoomPreviewBlurStrength)));
            }
            else
                IsZoomPreviewLocatorVisible = false;

            lastZoomPreviewVisualLocation = targetPoint;
        }

        /// <summary>
        /// Generate a random texture texture.
        /// </summary>
        /// <param name="background">The brush to use for the texture background.</param>
        /// <param name="foreground">The pen to use for dawing foreground features on the texture.</param>
        /// <param name="dimensions">The dimesions of the texture.</param>
        /// <param name="polyLines">The amount of polylines to add to the texture.</param>
        /// <param name="segmentsInEachPolyLine">The number of segments that make up each polyline.</param>
        /// <param name="speckles">The number of speckles to add to the texture.</param>
        /// <param name="speckleRadius">The radius of each speckle.</param>
        /// <returns>A generated texture returned as a visual.</returns>
        public virtual Visual GenerateTexture(Brush background, Pen foreground, Size dimensions, int polyLines, int segmentsInEachPolyLine, int speckles, int speckleRadius)
        {
            var drawing = new DrawingVisual();
            var tileArea = new Rect(new Point(0, 0), dimensions);
            using (var dC = drawing.RenderOpen())
            {
                dC.DrawRectangle(background, new Pen(Brushes.Transparent, 0d), tileArea);

                for (var index = 0; index < 10; index++)
                {
                    var multiLineLastPoint = new Point(randomGenerator.Next(0, (int)dimensions.Width), randomGenerator.Next(0, (int)dimensions.Height));

                    for (var segmentPointIndex = 0; segmentPointIndex < 10; segmentPointIndex++)
                    {
                        var multiLineCurrentPoint = new Point(randomGenerator.Next((int)Math.Max(0, multiLineLastPoint.X - (dimensions.Width / 10)), (int)Math.Min(dimensions.Width, multiLineLastPoint.X + dimensions.Width / 10)), randomGenerator.Next((int)Math.Max(0, multiLineLastPoint.Y - (dimensions.Height / 10)), (int)Math.Min(dimensions.Height, multiLineLastPoint.Y + dimensions.Height / 10)));
                        dC.DrawLine(foreground, multiLineLastPoint, multiLineCurrentPoint);
                        multiLineLastPoint = multiLineCurrentPoint;
                    }
                }

                for (var index = 0; index < speckles; index++)
                    dC.DrawEllipse(foreground.Brush, foreground, new Point(randomGenerator.Next(0 + speckleRadius, (int)dimensions.Width - speckleRadius), randomGenerator.Next(0 + speckleRadius, (int)dimensions.Height - speckleRadius)), speckleRadius, speckleRadius);
            }

            return drawing;
        }

        /// <summary>
        /// Show the save settings file dialog.
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public virtual void ShowSaveSettingsFileDialog()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Plankton Settings File (*.xml)|*.xml",
                Title = "Save Settings",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            saveDialog.FileOk += (s, e) =>
            {
                try
                {
                    var file = new PlanktonSettingsFile { FullPath = saveDialog.FileName };

                    int planktonBrushIndex;

                    if (greenPlanktonRadioButton.IsChecked.Value)
                        planktonBrushIndex = 0;
                    else if (redPlanktonRadioButton.IsChecked.Value)
                        planktonBrushIndex = 1;
                    else if (whitePlanktonRadioButton.IsChecked.Value)
                        planktonBrushIndex = 2;
                    else if (transparentPlanktonRadioButton.IsChecked.Value)
                        planktonBrushIndex = 3;
                    else if (gunkPlanktonRadioButton.IsChecked.Value)
                        planktonBrushIndex = 4;
                    else if (performancePlanktonRadioButton.IsChecked.Value)
                        planktonBrushIndex = 5;
                    else planktonBrushIndex = 0;

                    int seaBedBrushIndex;

                    if (rockSeaBedRadioButton.IsChecked.Value)
                        seaBedBrushIndex = 0;
                    else if (sandSeaBedRadioButton.IsChecked.Value)
                        seaBedBrushIndex = 1;
                    else if (slateSeaBedRadioButton.IsChecked.Value)
                        seaBedBrushIndex = 2;
                    else if (iceSeaBedRadioButton.IsChecked.Value)
                        seaBedBrushIndex = 3;
                    else if (strangeSeaBedRadioButton.IsChecked.Value)
                        seaBedBrushIndex = 4;
                    else seaBedBrushIndex = 0;

                    int seaBrushIndex;

                    if (seaBackgroundRadioButton.IsChecked.Value)
                        seaBrushIndex = 0;
                    else if (pondBackgroundRadioButton.IsChecked.Value)
                        seaBrushIndex = 1;
                    else if (darkBackgroundRadioButton.IsChecked.Value)
                        seaBrushIndex = 2;
                    else if (lightBackgroundRadioButton.IsChecked.Value)
                        seaBrushIndex = 3;
                    else if (strangeBackgroundRadioButton.IsChecked.Value)
                        seaBrushIndex = 4;
                    else seaBrushIndex = 0;

                    file.BubbleSize = BubbleSize;
                    file.ChildBubbleBuoyancy = ChildBubbleBuoyancy;
                    file.ChildBubbleRate = ChildBubbleRate;
                    file.ChildBubbleSizeVariation = ChildBubbleSizeVariation;
                    file.CurrentAcceleration = CurrentAcceleration;
                    file.CurrentDeceleration = CurrentDeceleration;
                    file.CurrentDirection = CurrentDirection;
                    file.CurrentMode = CurrentMode;
                    file.CurrentRate = CurrentRate;
                    file.CurrentStrength = CurrentStrength;
                    file.CurrentVariation = CurrentVariation;
                    file.CurrentZStep = CurrentZStep;
                    file.CurrentZStepVariation = CurrentZStepVariation;
                    file.Density = Density;
                    file.Elements = Elements;
                    file.ElementsSize = ElementsSize;
                    file.ElementsSizeVariation = ElementsSizeVariation;
                    file.GenerateAndUseRandomSeaBedBrush = GenerateAndUseRandomSeaBedBrush;
                    file.GenerateAndUseRandomSeaBrush = GenerateAndUseRandomSeaBrush;
                    file.GenerateMultipleRandomElementFill = GenerateMultipleRandomElementFill;
                    file.GenerateRandomElementFill = GenerateRandomElementFill;
                    file.GenerateRandomLuminousElementFill = GenerateRandomLuminousElementFill;
                    file.IgnoreWaterViscosityWhenGeneratingCurrent = IgnoreWaterViscosityWhenGeneratingCurrent;
                    file.InvertPlanktonAttraction = InvertPlanktonAttraction;
                    file.Life = Life;
                    file.LightingEffectSpeedRatio = LightingEffectSpeedRatio;
                    file.LightingEffectStrength = LightingEffectStrength;
                    file.PlanktonAttractionReach = PlanktonAttractionReach;
                    file.PlanktonAttractionStrength = PlanktonAttractionStrength;
                    file.PlanktonAttractToChildBubbles = PlanktonAttractToChildBubbles;
                    file.PlanktonBrushIndex = planktonBrushIndex;
                    file.SeaBedBrushIndex = seaBedBrushIndex;
                    file.SeaBrushIndex = seaBrushIndex;
                    file.SeaBedMaxIncline = SeaBedMaxIncline;
                    file.SeaBedSmoothness = SeaBedSmoothness;
                    file.Travel = Travel;
                    file.UseArcSegmentsInSeaBedPath = UseArcSegmentsInSeaBedPath;
                    file.UseChildBubbles = UseChildBubbles;
                    file.UseCurrent = UseCurrent;
                    file.UseGravity = UseGravity;
                    file.UseLightingEffect = UseLightingEffect;
                    file.UseLineSegmentsInSeaBedPath = UseLineSegmentsInSeaBedPath;
                    file.UsePlanktonAttraction = UsePlanktonAttraction;
                    file.UseRandomCurrentDirection = UseRandomCurrentDirection;
                    file.UseRandomElementFill = UseRandomElementFill;
                    file.UseSeaBed = UseSeaBed;
                    file.UseZOnCurrent = UseZOnCurrent;
                    file.WaterViscosity = WaterViscosity;

                    file.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception caught saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            saveDialog.ShowDialog(Parent as Window);
        }

        /// <summary>
        /// Show the load settings file dialog.
        /// </summary>
        public virtual void ShowLoadSettingsFileDialog()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Plankton Settings File (*.xml)|*.xml",
                Multiselect = false,
                Title = "Load Settings",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            openDialog.FileOk += (s, e) =>
            {
                try
                {
                    var file = PlanktonSettingsFile.Open(openDialog.FileName);
                    ApplySettingsFromFile(file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception caught loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            openDialog.ShowDialog(Parent as Window);
        }

        /// <summary>
        /// Show the load image file dialog and return an image.
        /// </summary>
        /// <returns>A BitmapFrame contianing the image loaded by this file, else null.</returns>
        public virtual BitmapFrame ShowLoadImageFileDialog()
        {
            BitmapFrame image = null;

            var openDialog = new OpenFileDialog
            {
                Filter = "Image files (*.bmp, *.gif, *.jpg, *.jpeg, *.png, *.tif, *.tiff) | *.bmp; *.gif; *.jpg; *.jpeg; *.png; *.tif; *.tiff",
                Multiselect = false,
                Title = "Load Image File",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            openDialog.FileOk += (s, e) =>
            {
                try
                {
                    BitmapDecoder decoder;
                    var extention = openDialog.FileName.Substring(openDialog.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1);

                    switch (extention.ToUpper())
                    {
                        case ("BMP"):
                            decoder = new BmpBitmapDecoder(new Uri(openDialog.FileName, UriKind.Absolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        case ("GIF"):
                            decoder = new GifBitmapDecoder(new Uri(openDialog.FileName, UriKind.Absolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        case ("JPG"):
                        case ("JPEG"):
                            decoder = new JpegBitmapDecoder(new Uri(openDialog.FileName, UriKind.Absolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        case ("PNG"):
                            decoder = new PngBitmapDecoder(new Uri(openDialog.FileName, UriKind.Absolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        case ("TIF"):
                        case ("TIFF"):
                            decoder = new TiffBitmapDecoder(new Uri(openDialog.FileName, UriKind.Absolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                            break;
                        default: throw new NotImplementedException();
                    }

                    image = decoder.Frames[0];
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception caught loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            openDialog.ShowDialog(Parent as Window);

            return image;
        }

        /// <summary>
        /// Apply settings from a file.
        /// </summary>
        /// <param name="file">The file to apply settings from.</param>
        public virtual void ApplySettingsFromFile(PlanktonSettingsFile file)
        {
            var wasPaused = IsPaused;
            IsPaused = true;

            ClearAllCurrent();
            RemoveSeaBed();

            preventRegenerationOfPlanktonOnColourChange = true;
            BubbleSize = file.BubbleSize;
            ChildBubbleBuoyancy = file.ChildBubbleBuoyancy;
            ChildBubbleRate = file.ChildBubbleRate;
            ChildBubbleSizeVariation = file.ChildBubbleSizeVariation;
            Density = file.Density;
            Elements = file.Elements;
            ElementsSize = file.ElementsSize;
            ElementsSizeVariation = file.ElementsSizeVariation;
            GenerateMultipleRandomElementFill = file.GenerateMultipleRandomElementFill;
            GenerateRandomElementFill = file.GenerateRandomElementFill;
            GenerateRandomLuminousElementFill = file.GenerateRandomLuminousElementFill;
            InvertPlanktonAttraction = file.InvertPlanktonAttraction;
            Life = file.Life;
            LightingEffectSpeedRatio = file.LightingEffectSpeedRatio;
            LightingEffectStrength = file.LightingEffectStrength;
            PlanktonAttractionReach = file.PlanktonAttractionReach;
            PlanktonAttractionStrength = file.PlanktonAttractionStrength;
            PlanktonAttractToChildBubbles = file.PlanktonAttractToChildBubbles;
            SeaBedMaxIncline = file.SeaBedMaxIncline;
            SeaBedSmoothness = file.SeaBedSmoothness;
            Travel = file.Travel;
            UseChildBubbles = file.UseChildBubbles;
            UseGravity = file.UseGravity;
            UsePlanktonAttraction = file.UsePlanktonAttraction;
            UseRandomElementFill = file.UseRandomElementFill;
            UseSeaBed = file.UseSeaBed;
            UseArcSegmentsInSeaBedPath = file.UseArcSegmentsInSeaBedPath;
            UseLightingEffect = file.UseLightingEffect;
            UseLineSegmentsInSeaBedPath = file.UseLineSegmentsInSeaBedPath;
            WaterViscosity = file.WaterViscosity;

            switch (file.PlanktonBrushIndex)
            {
                case (0):
                    greenPlanktonRadioButton.IsChecked = true;
                    break;
                case (1):
                    whitePlanktonRadioButton.IsChecked = true;
                    break;
                case (2):
                    redPlanktonRadioButton.IsChecked = true;
                    break;
                case (3):
                    transparentPlanktonRadioButton.IsChecked = true;
                    break;
                case (4):
                    gunkPlanktonRadioButton.IsChecked = true;
                    break;
                default:
                    performancePlanktonRadioButton.IsChecked = true;
                    break;
            }

            switch (file.SeaBrushIndex)
            {
                case (0):
                    seaBackgroundRadioButton.IsChecked = true;
                    break;
                case (1):
                    pondBackgroundRadioButton.IsChecked = true;
                    break;
                case (2):
                    darkBackgroundRadioButton.IsChecked = true;
                    break;
                case (3):
                    lightBackgroundRadioButton.IsChecked = true;
                    break;
                default:
                    strangeBackgroundRadioButton.IsChecked = true;
                    break;
            }

            switch (file.SeaBedBrushIndex)
            {
                case (0):
                    rockSeaBedRadioButton.IsChecked = true;
                    break;
                case (1):
                    sandSeaBedRadioButton.IsChecked = true;
                    break;
                case (2):
                    slateSeaBedRadioButton.IsChecked = true;
                    break;
                case (3):
                    iceSeaBedRadioButton.IsChecked = true;
                    break;
                default:
                    strangeSeaBedRadioButton.IsChecked = true;
                    break;
            }

            GenerateAndUseRandomSeaBedBrush = file.GenerateAndUseRandomSeaBedBrush;
            GenerateAndUseRandomSeaBrush = file.GenerateAndUseRandomSeaBrush;
            OverridePlanktonBrushWithCustom = false;
            OverrideBackgroundBrushWithCustom = false;
            OverrideSeaBedBrushWithCustom = false;
            UseCurrent = file.UseCurrent;
            CurrentDirection = file.CurrentDirection;
            ActiveCurrentDirection = file.CurrentDirection;
            CurrentRate = file.CurrentRate;
            CurrentStrength = file.CurrentStrength;
            CurrentVariation = file.CurrentVariation;
            UseRandomCurrentDirection = file.UseRandomCurrentDirection;
            UseZOnCurrent = file.UseZOnCurrent;
            CurrentMode = file.CurrentMode;
            CurrentZStep = file.CurrentZStep;
            CurrentZStepVariation = file.CurrentZStepVariation;
            IgnoreWaterViscosityWhenGeneratingCurrent = file.IgnoreWaterViscosityWhenGeneratingCurrent;
            CurrentAcceleration = file.CurrentAcceleration;
            CurrentDeceleration = file.CurrentDeceleration;

            if (UseSeaBed)
                RenderSeaBed(UseLineSegmentsInSeaBedPath, UseArcSegmentsInSeaBedPath);

            preventRegenerationOfPlanktonOnColourChange = false;
            RegenerateWithCurrentSettings(false);

            if (!wasPaused)
                IsPaused = false;
        }

        /// <summary>
        /// Remove the current sea bed.
        /// </summary>
        protected virtual void RemoveSeaBed()
        {
            seaBedPath.Data = null;
        }

        /// <summary>
        /// Render the sea bed.
        /// </summary>
        /// <param name="useLine">Specify if lines are used to make up the sea bed.</param>
        /// <param name="useArc">Specify if arcs are used to make up the sea bed.</param>
        protected virtual void RenderSeaBed(bool useLine, bool useArc)
        {
            RemoveSeaBed();

            FrameworkElement area = areaCanvas;
            var pG = new PathGeometry();
            var segments = new List<PathSegment>();
            var relativeSmoothness = (int)((area.ActualWidth / 100d) * SeaBedSmoothness);
            var relativeIncline = (int)((area.ActualHeight / 100d) * SeaBedMaxIncline);
            var initialPoint = new Point(0, area.ActualHeight - randomGenerator.Next(relativeIncline, Math.Max((int)(area.ActualHeight / 4d), relativeIncline)));
            var startPoint = initialPoint;
            var endPoint = startPoint;
            var segmentTypes = new List<Type>();

            if (useLine)
                segmentTypes.Add(typeof (LineSegment));

            if (useArc)
                segmentTypes.Add(typeof (ArcSegment));

            if (segmentTypes.Count == 0)
                throw new ArgumentException("Either arcs or lines must be specified as segment types");
            
            while (endPoint.X < area.ActualWidth)
            {
                endPoint = new Point(Math.Min(area.ActualWidth, Math.Min(startPoint.X + Math.Max(1, randomGenerator.Next((int)Math.Sqrt(relativeSmoothness), relativeSmoothness)), area.ActualWidth)), Math.Min(area.ActualHeight, Math.Max(0, startPoint.Y + randomGenerator.Next(-relativeIncline, relativeIncline))));
                var type = segmentTypes[randomGenerator.Next(0, segmentTypes.Count)];

                if (type == typeof (LineSegment))
                    segments.Add(new LineSegment(endPoint, true));
                else if (type == typeof (ArcSegment))
                    segments.Add(new ArcSegment(endPoint, new Size((Math.Max(endPoint.X, startPoint.X) - Math.Min(endPoint.X, startPoint.X)) * 2d, (Math.Max(endPoint.Y, startPoint.Y) - Math.Min(endPoint.Y, startPoint.Y)) * 2d), 0d, false, randomGenerator.Next(0, 2) % 2 == 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));
                else
                    throw new InvalidOperationException("Invalid type selected");

                startPoint = endPoint;
            }

            segments.Add(new LineSegment(new Point(startPoint.X, area.ActualHeight), true));
            segments.Add(new LineSegment(new Point(0, area.ActualHeight), true));
            pG.Figures.Add(new PathFigure(initialPoint, segments, true));
            seaBedPath.Data = pG;
            seaBedGeometry = pG;
            LastSeaBedGeometry = seaBedGeometry.ToString();
            seaBedPen = new Pen(seaBedPath.Stroke, seaBedPath.StrokeThickness);
        }

        /// <summary>
        /// Render the sea bed.
        /// </summary>
        /// <param name="data">The data used to generate the sea bed as a string.</param>
        protected virtual bool RenderSeaBed(string data)
        {
            RemoveSeaBed();

            try
            {
                var streamGeomerty = Geometry.Parse(data) as StreamGeometry;
                seaBedPath.Data = streamGeomerty;
                if (streamGeomerty != null) seaBedGeometry = streamGeomerty.GetFlattenedPathGeometry();

                if (seaBedGeometry == null)
                    throw new NullReferenceException();

                LastSeaBedGeometry = data;
                seaBedPen = new Pen(seaBedPath.Stroke, seaBedPath.StrokeThickness);
                return true;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("The data was not a valid geometry", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (FormatException)
            {
                MessageBox.Show("The data was not a valid geometry", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("The data was not a valid geometry", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Re-render a previous sea bed geometry at a new size.
        /// </summary>
        /// <param name="geometry">The geometry to re-render.</param>
        /// <param name="previousSizeOfGeometry">The previous size used for the geometry.</param>
        /// <param name="newSizeOfGeomety">The new size to use for the geometry.</param>
        protected virtual bool ReRenderPreviousSeaBedAtNewSize(PathGeometry geometry, Size previousSizeOfGeometry, Size newSizeOfGeomety)
        {
            try
            {
                RemoveSeaBed();

                var xScale = newSizeOfGeomety.Width / previousSizeOfGeometry.Width;
                var yScale = newSizeOfGeomety.Height / previousSizeOfGeometry.Height;

                if (geometry != null)
                {
                    var pG = new PathGeometry();

                    foreach (var pF in geometry.Figures)
                    {
                        var newFigure = new PathFigure { StartPoint = new Point(pF.StartPoint.X * xScale, pF.StartPoint.Y * yScale) };

                        foreach (var pS in pF.Segments)
                        {
                            if (pS is ArcSegment)
                            {
                                var aS = pS as ArcSegment;
                                newFigure.Segments.Add(new ArcSegment(new Point(aS.Point.X * xScale, aS.Point.Y * yScale), new Size(aS.Size.Width * xScale, aS.Size.Height * yScale), aS.RotationAngle, aS.IsLargeArc, aS.SweepDirection, aS.IsStroked));
                            }
                            else if (pS is BezierSegment)
                            {
                                var bS = pS as BezierSegment;
                                newFigure.Segments.Add(new BezierSegment(new Point(bS.Point1.X * xScale, bS.Point1.Y * yScale), new Point(bS.Point2.X * xScale, bS.Point2.Y * yScale), new Point(bS.Point3.X * xScale, bS.Point3.Y * yScale), bS.IsStroked));
                            }
                            else if (pS is LineSegment)
                            {
                                var lS = pS as LineSegment;
                                newFigure.Segments.Add(new LineSegment(new Point(lS.Point.X * xScale, lS.Point.Y * yScale), lS.IsStroked));
                            }
                            else if (pS is PolyBezierSegment)
                            {
                                var pBS = pS as PolyBezierSegment;
                                var points = new Point[pBS.Points.Count];
                                for (var i = 0; i < pBS.Points.Count; i++)
                                    points[i] = new Point(pBS.Points[i].X * xScale, pBS.Points[i].Y * yScale);

                                newFigure.Segments.Add(new PolyBezierSegment(points, pBS.IsStroked));
                            }
                            else if (pS is PolyLineSegment)
                            {
                                var pLS = pS as PolyLineSegment;
                                var points = new Point[pLS.Points.Count];

                                for (var i = 0; i < pLS.Points.Count; i++)
                                    points[i] = new Point(pLS.Points[i].X * xScale, pLS.Points[i].Y * yScale);

                                newFigure.Segments.Add(new PolyLineSegment(points, pLS.IsStroked));
                            }
                            else if (pS is PolyQuadraticBezierSegment)
                            {
                                var pQBS = pS as PolyQuadraticBezierSegment;
                                var points = new Point[pQBS.Points.Count];

                                for (var i = 0; i < pQBS.Points.Count; i++)
                                    points[i] = new Point(pQBS.Points[i].X * xScale, pQBS.Points[i].Y * yScale);

                                newFigure.Segments.Add(new PolyQuadraticBezierSegment(points, pQBS.IsStroked));
                            }
                            else if (pS is QuadraticBezierSegment)
                            {
                                var qBS = pS as QuadraticBezierSegment;
                                newFigure.Segments.Add(new QuadraticBezierSegment(new Point(qBS.Point1.X * xScale, qBS.Point1.Y * yScale), new Point(qBS.Point2.X * xScale, qBS.Point2.Y * yScale), qBS.IsStroked));
                            }
                            else
                                throw new NotImplementedException();
                        }

                        pG.Figures.Add(newFigure);
                    }

                    seaBedPath.Data = pG;
                    seaBedGeometry = pG;
                    LastSeaBedGeometry = seaBedGeometry.ToString();
                    seaBedPen = new Pen(seaBedPath.Stroke, seaBedPath.StrokeThickness);
                }
                else
                    RenderSeaBed(UseLineSegmentsInSeaBedPath, UseArcSegmentsInSeaBedPath);

                return true;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("The data was not a valid geometry", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (FormatException)
            {
                MessageBox.Show("The data was not a valid geometry", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("The data was not a valid geometry", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Apply a predefined group of quick settings.
        /// </summary>
        /// <param name="setting">The group of quick settings to apply.</param>
        internal virtual void ApplyQuickSettings(QuickSetting setting)
        {
            PlanktonSettingsFile file;

            switch (setting)
            {
                case (QuickSetting.AntiGravity):
                    file = PlanktonSettingsFile.AntiGravity;
                    break;
                case (QuickSetting.Attraction):
                    file = PlanktonSettingsFile.Attraction;
                    break;
                case (QuickSetting.Default):
                    file = PlanktonSettingsFile.Default;
                    break;
                case (QuickSetting.Dense):
                    file = PlanktonSettingsFile.Dense;
                    break;
                case (QuickSetting.Gunk):
                    file = PlanktonSettingsFile.Gunk;
                    break;
                case (QuickSetting.LuminousRandomStartup):
                    file = PlanktonSettingsFile.LuminousRandomStartup;
                    break;
                case (QuickSetting.Original):
                    file = PlanktonSettingsFile.Original;
                    break;
                case (QuickSetting.Performance):
                    file = PlanktonSettingsFile.Performance;
                    break;
                case (QuickSetting.Random):
                    file = new PlanktonSettingsFile();

                    if (UseEfficientValuesWhenRandomGenerating)
                        file.Elements = (int)amountSlider.Ticks[randomGenerator.Next(0, amountSlider.Ticks.Count / 2)];
                    else
                        file.Elements = (int)amountSlider.Ticks[randomGenerator.Next(0, amountSlider.Ticks.Count)];

                    file.ElementsSize = elementsSizeSlider.Ticks[randomGenerator.Next(0, elementsSizeSlider.Ticks.Count)];
                    file.ElementsSizeVariation = randomGenerator.Next(0, 100);
                    file.WaterViscosity = MaintainStandardPhysicsWhenRandomGeneratingSettings ? 0.98d : viscositySlider.Ticks[randomGenerator.Next(0, viscositySlider.Ticks.Count)];
                    file.Travel = travelSlider.Ticks[randomGenerator.Next(0, travelSlider.Ticks.Count)];
                    file.Life = randomGenerator.Next(0, 100);
                    file.UseRandomElementFill = randomGenerator.Next(0, 100) % 2 == 0;
                    file.GenerateRandomElementFill = ((!UseOnlyStandardBrushesWhenRandomGenerating) && (file.UseRandomElementFill) && (randomGenerator.Next(0, 100) % 2 == 0));
                    file.Density = densitySlider.Ticks[randomGenerator.Next(0, densitySlider.Ticks.Count)];
                    file.UseGravity = randomGenerator.Next(0, 100) % 2 == 0;
                    file.LightingEffectSpeedRatio = lightingEffectSpeedRatioSlider.Ticks[randomGenerator.Next(0, lightingEffectSpeedRatioSlider.Ticks.Count)];
                    file.LightingEffectStrength = lightingEffectStrengthSlider.Ticks[randomGenerator.Next(0, lightingEffectStrengthSlider.Ticks.Count)];
                    file.UseLightingEffect = randomGenerator.Next(0, 100) % 2 == 0;

                    if (UseEfficientValuesWhenRandomGenerating)
                        file.GenerateMultipleRandomElementFill = false;
                    else
                        file.GenerateMultipleRandomElementFill = ((file.UseRandomElementFill) && (randomGenerator.Next(0, 100) % 2 == 0));

                    file.GenerateRandomLuminousElementFill = ((!UseOnlyStandardBrushesWhenRandomGenerating) && (file.UseRandomElementFill) && (file.GenerateRandomElementFill) && (randomGenerator.Next(0, 100) % 2 == 0));
                    file.BubbleSize = randomGenerator.Next(3, 50);
                    file.UseChildBubbles = randomGenerator.Next(0, 100) % 2 == 0;
                    file.ChildBubbleBuoyancy = MaintainStandardPhysicsWhenRandomGeneratingSettings ? 1.0d : childBubbleBuoyancySlider.Ticks[randomGenerator.Next(0, childBubbleBuoyancySlider.Ticks.Count)];
                    file.ChildBubbleRate = randomGenerator.Next(0, UseEfficientValuesWhenRandomGenerating ? 20 : 100);
                    file.ChildBubbleSizeVariation = randomGenerator.Next(0, 100);
                    file.UseSeaBed = randomGenerator.Next(0, 100) % 2 == 0;
                    file.UseArcSegmentsInSeaBedPath = randomGenerator.Next(0, 100) % 2 == 0;
                    file.UseLineSegmentsInSeaBedPath = (!file.UseArcSegmentsInSeaBedPath || randomGenerator.Next(0, 100) % 2 == 0);
                    file.SeaBedSmoothness = randomGenerator.Next(UseEfficientValuesWhenRandomGenerating ? 75 : 1, 100);

                    if (UseEfficientValuesWhenRandomGenerating)
                    {
                        file.SeaBedSmoothness = randomGenerator.Next(75, 100);
                        file.SeaBedMaxIncline = randomGenerator.Next(1, 5);
                    }
                    else
                    {
                        file.SeaBedSmoothness = randomGenerator.Next(1, 100);
                        file.SeaBedMaxIncline = randomGenerator.Next(1, 50);
                    }

                    file.UsePlanktonAttraction = randomGenerator.Next(0, 100) % 2 == 0;
                    file.InvertPlanktonAttraction = ((file.UsePlanktonAttraction) && (randomGenerator.Next(0, 100) % 2 == 0));
                    file.PlanktonAttractionReach = randomGenerator.Next(1, 25);
                    file.PlanktonAttractionStrength = randomGenerator.Next((int)Math.Ceiling(file.Travel), 110);
                    file.PlanktonAttractToChildBubbles = randomGenerator.Next(0, 100) % 2 == 0;
                    file.GenerateAndUseRandomSeaBedBrush = ((!UseOnlyStandardBrushesWhenRandomGenerating) && (randomGenerator.Next(0, 100) % 2 == 0));
                    file.GenerateAndUseRandomSeaBrush = ((!UseOnlyStandardBrushesWhenRandomGenerating) && (randomGenerator.Next(0, 100) % 2 == 0));
                    file.PlanktonBrushIndex = randomGenerator.Next(0, 6);
                    file.SeaBrushIndex = randomGenerator.Next(0, 5);
                    file.SeaBedBrushIndex = randomGenerator.Next(0, 5);
                    file.UseCurrent = randomGenerator.Next(0, 100) % 2 == 0;
                    file.CurrentDirection = randomGenerator.Next(0, 2) % 2 == 0 ? randomGenerator.Next(20, 160) : randomGenerator.Next(200, 340);
                    file.CurrentRate = randomGenerator.Next(0, 100);
                    file.CurrentStrength = randomGenerator.Next(1, 100);
                    file.CurrentVariation = randomGenerator.Next(0, 100);
                    file.UseRandomCurrentDirection = randomGenerator.Next(0, 100) % 2 == 0;
                    file.UseZOnCurrent = randomGenerator.Next(0, 100) % 2 == 0;
                    file.CurrentZStep = randomGenerator.Next(1, 100);
                    file.CurrentZStepVariation = randomGenerator.Next(0, 100);
                    file.CurrentMode = randomGenerator.Next(0, 2) == 0 ? CurrentSwellStage.PreMainUp : CurrentSwellStage.MainUp;
                    file.IgnoreWaterViscosityWhenGeneratingCurrent = !MaintainStandardPhysicsWhenRandomGeneratingSettings && randomGenerator.Next(0, 100) % 2 == 0;
                    file.CurrentAcceleration = MaintainStandardPhysicsWhenRandomGeneratingSettings ? 0.95d : currentAccelerationSlider.Ticks[randomGenerator.Next(0, currentAccelerationSlider.Ticks.Count)];
                    file.CurrentDeceleration = MaintainStandardPhysicsWhenRandomGeneratingSettings ? 0.97d : currentDecelerationSlider.Ticks[randomGenerator.Next(0, currentDecelerationSlider.Ticks.Count)];

                    break;
                case (QuickSetting.Spikey):
                    file = PlanktonSettingsFile.Spikey;
                    break;
                default: throw new NotImplementedException();
            }

            ApplySettingsFromFile(file);
        }

        /// <summary>
        /// Generate the main bubble.
        /// </summary>
        /// <param name="center">The center point of the main bubble.</param>
        /// <param name="vector">The vector of the main bubble.</param>
        /// <returns>The main bubble.</returns>
        protected virtual MoveableElement GenerateMainBubble(Point center, Vector vector)
        {
            var b = FindResource("BubbleBrush") as Brush;

            if (b != null)
                b.Opacity = 0.65d;

            return MoveableElement.Create(center, BubbleSize / 2d, vector, bubblePen, b);
        }

        /// <summary>
        /// Update the mouse position relative to the PlanktonControl.areaCanvas panel.
        /// </summary>
        protected virtual void UpdateMousePosition()
        {
            if ((isHandlingMousePositionUpdate) && (!UseSmoothMousePositionUpdating))
                return;

            isHandlingMousePositionUpdate = true;
            currentMousePoint = Mouse.GetPosition(areaCanvas);
            mouseVector.X = currentMousePoint.X - previousMousePoint.X;
            mouseVector.Y = currentMousePoint.Y - previousMousePoint.Y;

            if (bubble == null)
                bubble = GenerateMainBubble(currentMousePoint, mouseVector);
            else
                bubble.Geometry.Center = currentMousePoint;

            previousMousePoint = currentMousePoint;

            if (UseZoomPreview)
            {
                zoomPreviewFactor += zoomPreviewVector.Z;
                bool showLocator;

                switch (ZoomPreviewLocatorMode)
                {
                    case (ZoomPreviewLocaterMode.Always):
                    case (ZoomPreviewLocaterMode.AnythingButPlankton):
                    case (ZoomPreviewLocaterMode.OnlyMainBubble):
                        showLocator = true;
                        break;
                    default:
                        showLocator = false;
                        break;
                }

                UpdateZoomPreview(currentMousePoint, bubble, areaCanvas, zoomPreviewFactor, showLocator);
            }

            isHandlingMousePositionUpdate = false;
        }

        /// <summary>
        /// Pop a child bubble.
        /// </summary>
        /// <param name="child">The bubble to pop.</param>
        public virtual void PopChildBubble(MoveableElement child)
        {
            childBubbles[child] = false;
        }

        /// <summary>
        /// Pop all child bubbles.
        /// </summary>
        public virtual void PopChildBubbles()
        {
            for (var i = 0; i < childBubbles.Count; i++)
                PopChildBubble(childBubbles.Keys.ElementAt(i));
        }

        /// <summary>
        /// Force a transparent background onto this control.
        /// </summary>
        public virtual void ForceTransparentBackgound()
        {
            if (!IsForcedIntoTransparentMode)
                return;

            areaCanvas.Background = Brushes.Transparent;
            IsForcedIntoTransparentMode = true;
        }

        /// <summary>
        /// Set the background brush from the current settings.
        /// </summary>
        /// <param name="maintainLastGeneratedBackgroundBrushIfPossible">Specify if the last generated sea bed brush should be maintained if possible.</param>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void setBackgroundBrushFromCurrentSettings(bool maintainLastGeneratedBackgroundBrushIfPossible)
        {
            if (OverrideBackgroundBrushWithCustom)
                Resources["BackgroundBrush"] = CustomBackgroundBrush;
            else if (GenerateAndUseRandomSeaBrush)
            {
                if ((maintainLastGeneratedBackgroundBrushIfPossible) && (lastGeneratedBackgroundBrush != null))
                    Resources["BackgroundBrush"] = lastGeneratedBackgroundBrush;
                else
                {
                    var brush = GenerateRandomLinearGradientBrush(new Point(randomGenerator.Next(0, 100) / 100d, randomGenerator.Next(0, 100) / 100d), new Point(randomGenerator.Next(0, 100) / 100d, randomGenerator.Next(0, 100) / 100d), 1, randomGenerator.Next(2, UseEfficientValuesWhenRandomGenerating ? 32 : 128));
                    Resources["BackgroundBrush"] = brush;
                    lastGeneratedBackgroundBrush = brush;
                }
            }
            else
            {
                Brush brush;
                if (seaBackgroundRadioButton.IsChecked.Value)
                    brush = seaBackgroundRadioButton.CommandParameter as Brush;
                else if (pondBackgroundRadioButton.IsChecked.Value)
                    brush = pondBackgroundRadioButton.CommandParameter as Brush;
                else if (darkBackgroundRadioButton.IsChecked.Value)
                    brush = darkBackgroundRadioButton.CommandParameter as Brush;
                else if (lightBackgroundRadioButton.IsChecked.Value)
                    brush = lightBackgroundRadioButton.CommandParameter as Brush;
                else if (strangeBackgroundRadioButton.IsChecked.Value)
                    brush = strangeBackgroundRadioButton.CommandParameter as Brush;
                else brush = seaBackgroundRadioButton.CommandParameter as Brush;

                Resources["BackgroundBrush"] = brush;
            }
        }

        /// <summary>
        /// Set the sea bed brush from the current settings.
        /// </summary>
        /// <param name="maintainLastGeneratedSeaBedBrushIfPossible">Specify if the last generated sea bed brush should be maintained if possible.</param>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void setSeaBedBrushFromCurrentSettings(bool maintainLastGeneratedSeaBedBrushIfPossible)
        {
            if (OverrideSeaBedBrushWithCustom)
                Resources["SeaBedBrush"] = CustomSeaBedBrush;
            else if (GenerateAndUseRandomSeaBrush)
            {
                if ((maintainLastGeneratedSeaBedBrushIfPossible) && (lastGeneratedSeaBedBrush != null))
                    Resources["SeaBedBrush"] = lastGeneratedSeaBedBrush;
                else
                {
                    var brush = GenerateRandomLinearGradientBrush(new Point(0.5d, 0.0d), new Point(0.5d, 1.0d), 2, 4);
                    Resources["SeaBedBrush"] = brush;
                    lastGeneratedSeaBedBrush = brush;
                }
            }
            else
            {
                Brush brush;
                if (rockSeaBedRadioButton.IsChecked.Value)
                    brush = rockSeaBedRadioButton.CommandParameter as Brush;
                else if (sandSeaBedRadioButton.IsChecked.Value)
                    brush = sandSeaBedRadioButton.CommandParameter as Brush;
                else if (slateSeaBedRadioButton.IsChecked.Value)
                    brush = slateSeaBedRadioButton.CommandParameter as Brush;
                else if (iceSeaBedRadioButton.IsChecked.Value)
                    brush = iceSeaBedRadioButton.CommandParameter as Brush;
                else if (strangeSeaBedRadioButton.IsChecked.Value)
                    brush = strangeSeaBedRadioButton.CommandParameter as Brush;
                else brush = rockSeaBedRadioButton.CommandParameter as Brush;

                Resources["SeaBedBrush"] = brush;
            }
        }

        /// <summary>
        /// Get plankton brushes from the current settings.
        /// </summary>
        /// <param name="maintainAnyGeneratedPlanktonBrushes">Specify if any randomly generated brushes should be maintained.</param>
        /// <returns>An array containing all generated plankton brushes.</returns>
        private Brush[] getPlanktonBrushesFromCurrentSettings(bool maintainAnyGeneratedPlanktonBrushes)
        {
            Brush[] planktonBrushes;

            if (OverridePlanktonBrushWithCustom)
                planktonBrushes = new[] { CustomPlanktonBrush };
            else if (UseRandomElementFill)
            {
                if (GenerateRandomElementFill)
                {
                    if ((maintainAnyGeneratedPlanktonBrushes) && (lastGeneratedPlanktonBrushes != null) && (lastGeneratedPlanktonBrushes.Length > 0))
                        planktonBrushes = lastGeneratedPlanktonBrushes;
                    else
                    {
                        planktonBrushes = new Brush[randomGenerator.Next(1, Math.Max(1, GenerateMultipleRandomElementFill ? Elements > 100 ? Elements / 10 : Elements : 1))];

                        for (var index = 0; index < planktonBrushes.Length; index++)
                        {
                            var rGB = new RadialGradientBrush();

                            if (GenerateRandomLuminousElementFill)
                            {
                                rGB.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0d));
                                rGB.GradientStops.Add(new GradientStop(Color.FromArgb(255, (byte)randomGenerator.Next(0, 256), (byte)randomGenerator.Next(0, 256), (byte)randomGenerator.Next(0, 256)), 0.2d));
                                rGB.GradientStops.Add(new GradientStop(Colors.White, 0.0d));
                            }
                            else
                            {
                                for (var stopIndex = 0; stopIndex < 5; stopIndex++)
                                {
                                    double offset;
                                    switch (stopIndex)
                                    {
                                        case (0):
                                            offset = 1.0d;
                                            break;
                                        case (1):
                                            offset = 0.9d;
                                            break;
                                        case (2):
                                            offset = randomGenerator.Next(50, 80) / 10d;
                                            break;
                                        case (3):
                                            offset = randomGenerator.Next(20, 40) / 10d;
                                            break;
                                        case (4):
                                            offset = 0.0d;
                                            break;
                                        default: throw new NotImplementedException();
                                    }

                                    rGB.GradientStops.Add(stopIndex == 1 ? new GradientStop(rGB.GradientStops[0].Color, offset) : new GradientStop(Color.FromArgb((byte)randomGenerator.Next(127, 256), (byte)randomGenerator.Next(0, 256), (byte)randomGenerator.Next(0, 256), (byte)randomGenerator.Next(0, 256)), offset));
                                }
                            }
                            planktonBrushes[index] = rGB;
                        }
                    }
                }
                else
                {
                    if (GenerateMultipleRandomElementFill)
                    {
                        planktonBrushes = new Brush[4];
                        planktonBrushes[0] = FindResource("PlanktonGreenBrush") as Brush;
                        planktonBrushes[1] = FindResource("PlanktonWhiteBrush") as Brush;
                        planktonBrushes[2] = FindResource("PlanktonRedBrush") as Brush;
                        planktonBrushes[3] = FindResource("PlanktonTransparentBrush") as Brush;
                    }
                    else
                    {
                        planktonBrushes = new Brush[1];

                        switch (randomGenerator.Next(0, 6))
                        {
                            case (0):
                                planktonBrushes[0] = FindResource("PlanktonGreenBrush") as Brush;
                                break;
                            case (1):
                                planktonBrushes[0] = FindResource("PlanktonWhiteBrush") as Brush;
                                break;
                            case (2):
                                planktonBrushes[0] = FindResource("PlanktonRedBrush") as Brush;
                                break;
                            case (3):
                                planktonBrushes[0] = FindResource("PlanktonTransparentBrush") as Brush;
                                break;
                            case (4):
                                planktonBrushes[0] = FindResource("PlanktonGunkBrush") as Brush;
                                break;
                            default:
                                planktonBrushes[0] = FindResource("PlanktonPerformanceBrush") as Brush;
                                break;
                        }
                    }
                }
            }
            else
                planktonBrushes = new[] { FindResource("PlanktonBrush") as Brush };

            return planktonBrushes;
        }

        /// <summary>
        /// Handle GenerateSeaBedTextureCommand callbacks.
        /// </summary>
        protected virtual void OnGenerateSeaBedTextureCommand()
        {
            var tileDimensions = new Size(50, 50);
            var startColor = Color.FromRgb((byte)randomGenerator.Next(50, 156), (byte)randomGenerator.Next(50, 156), (byte)randomGenerator.Next(50, 156));
            var textureDetail = Color.FromRgb((byte)(startColor.R - 50), (byte)(startColor.G - 50), (byte)(startColor.B - 50));
            Brush textureBrush = new SolidColorBrush(textureDetail);
            var texturePen = new Pen(textureBrush, 1);
            Brush backgroundBrush = new SolidColorBrush(startColor);
            var drawing = GenerateTexture(backgroundBrush, texturePen, tileDimensions, 10, 10, 50, 1);
            CustomSeaBedBrush = new VisualBrush(drawing)
            {
                TileMode = TileMode.Tile,
                Stretch = Stretch.None,
                Viewport = new Rect(0, 0, tileDimensions.Width - 0.5d, tileDimensions.Height - 0.5d),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        /// <summary>
        /// Handle DefaultZoomCommand callbacks.
        /// </summary>
        protected virtual void OnDefaultZoomCommand()
        {
            UseZoomPreview = true;
            ZoomPreviewSize = 100d;
            MaximumZoom = 5.0d;
            MinimumZoom = 2.0d;
            UseAutoPanOnZoomPreview = true;
            AutoPanSpeed = 0.01d;
            AutoPanSensitivity = 1d;
            IfMainBubbleNotAvailablePreviewMostInterestingElement = false;
            ZoomPreviewLocatorMode = ZoomPreviewLocaterMode.AnythingButMainBubble;
            UseZoomPreviewBlurEffect = true;
            MaximumZoomPreviewBlur = 7d;
            ZoomPreviewBlurStrength = 5d;
            ZoomPreviewBlurCorrection = 0.15d;
        }

        /// <summary>
        /// Handle DefaultOptionsCommand callbacks.
        /// </summary>
        protected virtual void OnDefaultOptionsCommand()
        {
            RefreshTime = 10;
            UseEfficientValuesWhenRandomGenerating = true;
            UseOnlyStandardBrushesWhenRandomGenerating = false;
            UseSmoothMousePositionUpdating = false;
            UseAnimation = true;
        }

        /// <summary>
        /// Repopulate plankton after a colour change.
        /// </summary>
        private void repopulatePlanktonAfterColourChange()
        {
            if (preventRegenerationOfPlanktonOnColourChange)
                return;

            update?.Stop();
            elementHost.RemovePlanktonDrawingVisual();
            lastGeneratedPlanktonBrushes = getPlanktonBrushesFromCurrentSettings(false);

            foreach (var element in plankton)
                element.Fill = lastGeneratedPlanktonBrushes[randomGenerator.Next(0, lastGeneratedPlanktonBrushes.Length)];

            elementHost.SpecifyPlanktonDrawingVisual(new DrawingVisual());
            elementHost.AddPlanktonElements(plankton.ToArray());

            if ((update != null) && (!IsPaused))
                update.Start();
        }

        #endregion

        #region PropertyCallbacks

        /// <summary>
        /// Handle PlanktonControl.BubbleSize property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnBubbleSizePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if (control.UseZoomPreview)
                control.UpdateMousePosition();
        }

        /// <summary>
        /// Handle PlanktonControl.UseZoomPreview property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnUseZoomPreviewPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if ((bool)args.NewValue)
                control.UpdateMousePosition();
        }

        /// <summary>
        /// Handle PlanktonControl.UseAutoPanOnZoomPreview property changes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnUseAutoPanOnZoomPreviewPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if (control.UseZoomPreview)
                control.UpdateMousePosition();
        }

        /// <summary>
        /// Handle PlanktonControl.UseSeaBed property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnUseSeaBedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // get control
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if ((bool)args.NewValue)
            {
                control.RenderSeaBed(control.UseLineSegmentsInSeaBedPath, control.UseArcSegmentsInSeaBedPath);
                control.RegenerateWithCurrentSettings(true);
            }
            else
                control.RemoveSeaBed();
        }

        /// <summary>
        /// Handle PlanktonControl.RefreshTime property changes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnRefreshTimePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control?.update == null)
                return;

            var wasRunning = ((control.update.IsEnabled) && (!control.IsPaused));

            if (control.update.IsEnabled)
                control.update.Stop();

            control.update.Interval = TimeSpan.FromMilliseconds((int)args.NewValue);

            if (wasRunning)
                control.update.Start();
        }

        /// <summary>
        /// Handle PlanktonControl.IsPaused property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnIsPausedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            var isPaused = (bool)args.NewValue;

            if (control.update == null)
                return;

            control.update.IsEnabled = !isPaused;

            if (isPaused)
                control.update.Stop();
            else
                control.update.Start();
        }

        /// <summary>
        /// Handle PlanktonControl.LightingEffectStrength property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnLightingEffectStrengthPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if (control.UseLightingEffect)
                control.ResetLightingEffect();
        }

        /// <summary>
        /// Handle PlanktonControl.LightingEffectSpeedRatio property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnLightingEffectSpeedRatioPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if (control.UseLightingEffect)
                control.ResetLightingEffect();
        }

        /// <summary>
        /// Handle PlanktonControl.GenerateAndUseRandomSeaBedBrush property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnGenerateAndUseRandomSeaBedBrushPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.setSeaBedBrushFromCurrentSettings(false);
        }

        /// <summary>
        /// Handle PlanktonControl.GenerateAndUseRandomSeaBrush property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnGenerateAndUseRandomSeaBrushPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.setBackgroundBrushFromCurrentSettings(false);
        }

        /// <summary>
        /// Handle PlanktonControl.GenerateRandomLuminousElementFill property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnGenerateRandomLuminousElementFillPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.repopulatePlanktonAfterColourChange();
        }

        /// <summary>
        /// Handle PlanktonControl.GenerateMultipleRandomElementFill property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnGenerateMultipleRandomElementFillPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.repopulatePlanktonAfterColourChange();            
        }

        /// <summary>
        /// Handle PlanktonControl.UseRandomElementFill property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnUseRandomElementFillPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.repopulatePlanktonAfterColourChange();
        }

        /// <summary>
        /// Handle PlanktonControl.GenerateRandomElementFill property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnGenerateRandomElementFillPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.repopulatePlanktonAfterColourChange();
        }

        /// <summary>
        /// Handle PlanktonControl.OverridePlanktonBrushWithCustom property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnOverridePlanktonBrushWithCustomPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.repopulatePlanktonAfterColourChange();
        }

        /// <summary>
        /// Handle PlanktonControl.CustomPlanktonBrush property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnCustomPlanktonBrushPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.repopulatePlanktonAfterColourChange();
        }

        /// <summary>
        /// Handle PlanktonControl.IsInFullScreenMode property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnIsInFullScreenModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            var handler = (bool)args.NewValue ? control.EnterFullScreenMode : control.ExitFullScreenMode;
            handler?.Invoke(control, new EventArgs());
        }

        /// <summary>
        /// Handle PlanktonControl.OverrideBackgroundBrushWithCustom property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnOverrideBackgroundBrushWithCustomPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.setBackgroundBrushFromCurrentSettings(false);
        }

        /// <summary>
        /// Handle PlanktonControl.OverrideSeaBedBrushWithCustom property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnOverrideSeaBedBrushWithCustomPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.setSeaBedBrushFromCurrentSettings(false);
        }

        /// <summary>
        /// Handle PlanktonControl.CustomBackgroundBrush property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnCustomBackgroundBrushPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.setBackgroundBrushFromCurrentSettings(false);
        }

        /// <summary>
        /// Handle PlanktonControl.CustomSeaBedBrush property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnCustomSeaBedBrushPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;
            control?.setSeaBedBrushFromCurrentSettings(false);
        }

        /// <summary>
        /// Handle PlanktonControl.CurrentMode property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnCurrentModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            var direction = (CurrentSwellStage)args.NewValue;

            switch (direction)
            {
                case (CurrentSwellStage.PreMainUp):
                    control.preMainUpCurrentModeRadioButton.IsChecked = true;
                    break;
                case (CurrentSwellStage.MainUp):
                    control.mainUpCurrentModeRadioButton.IsChecked = true;
                    break;
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Handle PlanktonControl.ZoomPreviewLocatorMode property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnZoomPreviewLocatorModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            var mode = (ZoomPreviewLocaterMode)args.NewValue;

            switch (mode)
            {
                case (ZoomPreviewLocaterMode.Always):
                    control.locatorAlwaysRadioButton.IsChecked = true;
                    break;
                case (ZoomPreviewLocaterMode.AnythingButMainBubble):
                    control.locatorPlanktonAndBubblesRadioButton.IsChecked = true;
                    break;
                case (ZoomPreviewLocaterMode.AnythingButPlankton):
                    control.locatorAnythingButPlanktonRadioButton.IsChecked = true;
                    break;
                case (ZoomPreviewLocaterMode.Never):
                    control.locatorNeverRadioButton.IsChecked = true;
                    break;
                case (ZoomPreviewLocaterMode.OnlyChildBubbles):
                    control.locatorBubblesRadioButton.IsChecked = true;
                    break;
                case (ZoomPreviewLocaterMode.OnlyMainBubble):
                    control.locatorMainBubbleRadioButton.IsChecked = true;
                    break;
                case (ZoomPreviewLocaterMode.OnlyPlankton):
                    control.locatorPlanktonRadioButton.IsChecked = true;
                    break;
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Handle PlanktonControl.ActiveCurrentDirection property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnActiveCurrentDirectionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if (!control.ShowCurrentIndicator)
                return;

            var newValue = (double)args.NewValue;

            if (control.UseAnimation)
            {
                var anim = new DoubleAnimation();

                // get the normalised (0-360) angle right now - could be higher if shortest path mod was applied
                var normalisedAngleRightNow = control.currentIndicatorRotateTransform.Angle % 360;
                anim.From = ((Math.Max(normalisedAngleRightNow, newValue) - Math.Min(normalisedAngleRightNow, newValue) > 180) && (normalisedAngleRightNow < newValue)) ? normalisedAngleRightNow + 360 : normalisedAngleRightNow;
                anim.To = ((Math.Max(normalisedAngleRightNow, newValue) - Math.Min(normalisedAngleRightNow, newValue) > 180) && (normalisedAngleRightNow > newValue)) ? newValue + 360 : newValue;
                anim.Duration = new Duration(TimeSpan.FromMilliseconds(200));
                control.currentIndicatorRotateTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
            }
            else
                control.currentIndicatorRotateTransform.Angle = newValue;
        }

        /// <summary>
        /// Handle PlanktonControl.ShowCurrentIndicator property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnShowCurrentIndicatorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if ((bool)args.NewValue)
                control.currentIndicatorMasterGrid.Opacity = 0.25d;
        }

        /// <summary>
        /// Handle PlanktonControl.UseAnimation property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnUseAnimationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if ((bool)args.NewValue)
                control.OptionsExpanderAnimationDuration = new Duration(TimeSpan.FromMilliseconds(250));
            else
                control.OptionsExpanderAnimationDuration = new Duration(TimeSpan.FromMilliseconds(0));
        }

        /// <summary>
        /// Handle PlanktonControl.UseZoomPreviewBlurEffect property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnUseZoomPreviewBlurEffectPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if ((bool)args.NewValue)
            {
                if (control.Effect != null)
                    control.Effect = null;

                control.Effect = new BlurEffect
                {
                    KernelType = KernelType.Box,
                    RenderingBias = RenderingBias.Performance,
                    Radius = control.nextZoomPreviewBlurRadius
                };
            }
            else
            {
                if (control.Effect != null)
                    control.Effect = null;

                control.nextZoomPreviewBlurRadius = 0.0d;
            }
        }

        /// <summary>
        /// Handle PlanktonControl.UseLightingEffect property changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void OnUseLightingEffectPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = obj as PlanktonControl;

            if (control == null)
                return;

            if ((bool)args.NewValue)
                control.StartLightingEffect();
            else
                control.StopLightingEffect();
        }

        #endregion

        #region StaticMethods

        /// <summary>
        /// Determine the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The distance between the two points.</returns>
        public static double DetermineDistanceBetweenTwoPoints(Point a, Point b)
        {
            return Math.Abs(Math.Sqrt(((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y))));
        }

        /// <summary>
        /// Determine the distance between two points.
        /// </summary>
        /// <param name="aX">Point a x location.</param>
        /// <param name="aY">Point a y location.</param>
        /// <param name="bX">Point b x location.</param>
        /// <param name="bY">Point b y location.</param>
        /// <returns>The distance between the two points.</returns>
        public static double DetermineDistanceBetweenTwoPoints(double aX, double aY, double bX, double bY)
        {
            return Math.Abs(Math.Sqrt(((bX - aX) * (bX - aX)) + ((bY - aY) * (bY - aY))));
        }

        /// <summary>
        /// Determine if two regular circles intesect each other.
        /// </summary>
        /// <param name="aLeft">The a left position.</param>
        /// <param name="aTop">The a top position.</param>
        /// <param name="aWidth">The a width.</param>
        /// <param name="aHeight">The a height.</param>
        /// <param name="bLeft">The b left position.</param>
        /// <param name="bTop">The b top position.</param>
        /// <param name="bWidth">The be width.</param>
        /// <param name="bHeight">The b height.</param>
        /// <returns>True if the ellipses intersect or touch, else false.</returns>
        public static bool DoRegularCirclesIntersect(double aLeft, double aTop, double aWidth, double aHeight, double bLeft, double bTop, double bWidth, double bHeight)
        {
            if ((Math.Abs(aWidth - aHeight) > 0.0) || (Math.Abs(bWidth - bHeight) > 0.0))
                return new Rect(aLeft, aTop, aWidth, aHeight).IntersectsWith(new Rect(bLeft, bTop, bWidth, bHeight));

            return DetermineDistanceBetweenTwoPoints(aLeft + (aWidth / 2d), aTop + (aHeight / 2d), bLeft + (bWidth / 2d), bTop + (bHeight / 2d)) <= ((aWidth / 2d) + (bWidth / 2d));
        }

        /// <summary>
        /// Determine if two regular circles intesect each other other on a path.
        /// </summary>
        /// <param name="endALeft">The a left position.</param>
        /// <param name="endATop">The a top position.</param>
        /// <param name="startALeft">The start left position of the a ellispe.</param>
        /// <param name="startATop">The start top position of the a ellispe.</param>
        /// <param name="aWidth">The a width.</param>
        /// <param name="aHeight">The a height.</param>
        /// <param name="endBLeft">The b left position.</param>
        /// <param name="endBTop">The b top position.</param>
        /// <param name="bWidth">The be width.</param>
        /// <param name="bHeight">The b height.</param>
        /// <param name="startBLeft">The start left position of the a ellispe.</param>
        /// <param name="startBTop">The start top position of the a ellispe.</param>
        /// <param name="steps">The amount of steps to check on the vector path.</param>
        /// <returns>True if the ellipses intersect or touch, else false.</returns>
        public static bool DoRegularCirclesIntersectOnVectorPath(double endALeft, double endATop, double startALeft, double startATop, double aWidth, double aHeight, double endBLeft, double endBTop, double startBLeft, double startBTop, double bWidth, double bHeight, int steps)
        {
            var appliedSteps = Math.Min(((Math.Abs(endALeft - startALeft) > 0.0) || (Math.Abs(endATop - startATop) > 0.0) || (Math.Abs(endBLeft - startBLeft) > 0.0) || (Math.Abs(endBTop - startBTop) > 0.0)) ? steps : 1, 10);

            for (var index = 0; index < appliedSteps; index++)
                if (DoRegularCirclesIntersect(endALeft - (((startALeft - endALeft) / appliedSteps) * (appliedSteps - index)), endATop - (((startATop - endATop) / appliedSteps) * (appliedSteps - index)), aWidth, aHeight, endBLeft - (((startBLeft - endBLeft) / appliedSteps) * (appliedSteps - index)), endBTop - (((startBTop - endBTop) / appliedSteps) * (appliedSteps - index)), bWidth, bHeight))
                    return true;

            return false;
        }

        /// <summary>
        /// Determine if two regular circles fully overlap each other.
        /// </summary>
        /// <param name="aLeft">The a lift position.</param>
        /// <param name="aTop">The a top position.</param>
        /// <param name="aRadius">The a radius.</param>
        /// <param name="bLeft">The b left position.</param>
        /// <param name="bTop">The b top position.</param>
        /// <param name="bRadius">The b radius.</param>
        /// <returns>True if the ellipses fully overlap, else false.</returns>
        public static bool DoRegularCirclesOverlap(double aLeft, double aTop, double aRadius, double bLeft, double bTop, double bRadius)
        {
            return DetermineDistanceBetweenTwoPoints(aLeft + aRadius, aTop + aRadius, bLeft + bRadius, bTop + bRadius) <= Math.Max(aRadius, bRadius) - Math.Min(aRadius, bRadius);
        }

        /// <summary>
        /// Update a rectangle to border the permiter of a FrameworkElement.
        /// </summary>
        /// <param name="rectangle">The rectangle to update.</param>
        /// <param name="element">The FrameworkElement to update the rectangle property to.</param>
        private static void updateRectangle(ref Rect rectangle, Geometry element)
        {
            // set bounds
            rectangle = element.Bounds;
        }

        /// <summary>
        /// Determine a projected collision point for an Ellipse once a vector has been applied to it.
        /// </summary>
        /// <param name="ellipse">The rectangle that bounds a virtual Ellipse to use for determing the collision point.</param>
        /// <param name="vector">The vector of the ellipse.</param>
        /// <returns>The 2D point describing where to use for testing a projected collision.</returns>
        public static Point DetermineProjectedCollisionPoint(Rect ellipse, Vector vector)
        {
            // -calculate center point of ellipse
            // -using vector to get angle find the point on the ellipse that is going to connect first
            // -add VectorX and VectorY to projected point

            var angle = Math.Atan(vector.X / vector.Y);
            var collisionPoint = new Point(ellipse.Left + (ellipse.Width / 2d), ellipse.Top + (ellipse.Height / 2d));
            collisionPoint.X += Math.Sin(angle) * (ellipse.Width / 2d);
            collisionPoint.Y += Math.Cos(angle) * (ellipse.Height / 2d);
            collisionPoint.X += vector.X;
            collisionPoint.Y += vector.Y;
            return collisionPoint;
        }

        #endregion

        #region EventHandlers

        #region UICallbacks

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UseSeaBed)
                RegenerateWithCurrentSettings(true);

            preventRegenerationOfPlanktonOnColourChange = false;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            update.Stop();
        }

        private void areaCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateMousePosition();
        }

        private void areaCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            bubble = null;
        }

        private void areaCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (bubble == null)
                bubble = GenerateMainBubble(e.GetPosition(areaCanvas), mouseVector);
        }

        private void planktonBrushRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Resources["PlanktonBrush"] = ((RadioButton)sender).CommandParameter as Brush;
            repopulatePlanktonAfterColourChange();
        }

        private void backgroundBrushRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Resources["BackgroundBrush"] = ((RadioButton)sender).CommandParameter as Brush;
        }

        private void seaBedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Resources["SeaBedBrush"] = ((RadioButton)sender).CommandParameter as Brush;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (UseSeaBed)
            {
                newestSizeRenderRequest = e.NewSize;

                if (!hasResizeProcessBeenRequested)
                {
                    hasResizeProcessBeenRequested = true;

                    ClearAllCurrent();
                    RemoveSeaBed();

                    DispatcherTimer dT = null;

                    EventHandler repopulation = (s, a) =>
                    {

                        if (NativeMethods.IsLeftMouseButtonDown())
                            return;

                        RenderSeaBed(UseLineSegmentsInSeaBedPath, UseArcSegmentsInSeaBedPath);
                        RegenerateWithCurrentSettings(true);
                        dT?.Stop();
                        dT = null;
                        hasResizeProcessBeenRequested = false;
                    };

                    dT = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.ApplicationIdle, repopulation, Dispatcher);
                    dT.Start();
                }
            }

            if (UseZoomPreview)
            {
                var heightConverter = FindResource("zoomPreviewMaxHeightConverter") as IValueConverter;

                if (heightConverter != null)
                {
                    var maxZoomPreview = (double)heightConverter.Convert(newestSizeRenderRequest.Height, typeof (double), null, CultureInfo.CurrentCulture);

                    if (ZoomPreviewSize > maxZoomPreview)
                        ZoomPreviewSize = maxZoomPreview;
                }
            }

            if (!optionsExpander.IsExpanded)
                return;
            
            optionsExpander.IsExpanded = false;
            optionsExpander.IsExpanded = true;
        }

        private void copySeaBedGeometryToClipBoardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(LastSeaBedGeometry);
        }

        private void pasteSeaBedGeometryToClipBoardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LastSeaBedGeometry = Clipboard.GetText();
        }

        private void lineCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((!UseArcSegmentsInSeaBedPath) && (!UseLineSegmentsInSeaBedPath))
                UseLineSegmentsInSeaBedPath = true;
        }

        private void arcCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((!UseArcSegmentsInSeaBedPath) && (!UseLineSegmentsInSeaBedPath))
                UseArcSegmentsInSeaBedPath = true;
        }

        private void websiteLabel_MouseDown(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.wordpress.com/0xdfx/Plankton");
        }

        private void feedbackLabel_MouseDown(object sender, RoutedEventArgs e)
        {
            Process.Start($@"mailto:planktonfeedback@hotmail.com?subject=Plankton Feedback v{Version}");
        }

        #endregion

        #region CommandBindingCallbacks

        private void RegenerateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RegenerateWithCurrentSettings(true);
        }

        private void RandomCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplyQuickSettings(QuickSetting.Random);
        }

        private void TogglePreviewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UseZoomPreview = !UseZoomPreview;
        }

        private void QuickIncreasePlanktonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        { 
            if (!(Elements < amountSlider.Maximum))
                return;

            var ticks = amountSlider.Ticks.ToList();
            var index = ticks.IndexOf(Elements);
            Elements = (int)ticks[index + 1];

            RegenerateWithCurrentSettings(true);
        }

        private void QuickDecreasePlanktonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(Elements > amountSlider.Minimum))
                return;

            var ticks = amountSlider.Ticks.ToList();
            var index = ticks.IndexOf(Elements);
            Elements = (int)ticks[index - 1];

            RegenerateWithCurrentSettings(true);
        }

        private void PopChildBubblesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PopChildBubbles();
        }

        private void ToggleAttractionCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UsePlanktonAttraction = !UsePlanktonAttraction;
        }

        private void ToggleChildBubblesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UseChildBubbles = !UseChildBubbles;
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowSaveSettingsFileDialog();
        }

        private void LoadCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowLoadSettingsFileDialog();
        }

        private void DefaultOptionsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnDefaultOptionsCommand();
        }

        private void DefaultZoomCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnDefaultZoomCommand();
        }

        private void ExitFullScreenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsInFullScreenMode = false;
        }

        private void FullScreenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsInFullScreenMode = true;
        }

        private void GenerateGeometryCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RenderSeaBed(UseLineSegmentsInSeaBedPath, UseArcSegmentsInSeaBedPath);
            RegenerateWithCurrentSettings(true);
        }

        private void GenerateGeometryFromStringCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (RenderSeaBed(LastSeaBedGeometry))
                RegenerateWithCurrentSettings(true);
        }

        private void GenerateGeometryRealtimeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnShowDrawSeaBedWindow(0.5d);
        }

        private void ImportPlanktonBrushFromImageFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var i = ShowLoadImageFileDialog();

            if (i != null)
                CustomPlanktonBrush = new ImageBrush(i);
        }

        private void ImportBackgroundBrushFromImageFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var i = ShowLoadImageFileDialog();

            if (i == null)
                return;

            var iB = new ImageBrush(i) { Stretch = Stretch.Fill };
            CustomBackgroundBrush = iB;
        }

        private void ImportSeaBedBrushFromImageFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var i = ShowLoadImageFileDialog();

            if (i == null)
                return;

            var iB = new ImageBrush(i) { Stretch = Stretch.Fill };
            CustomSeaBedBrush = iB;
        }

        private void GenerateSeaBedTextureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnGenerateSeaBedTextureCommand();
        }

        private void ApplyQuickSettingCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplyQuickSettings((QuickSetting)e.Parameter);
        }

        private void ToggleCurrentCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UseCurrent = !UseCurrent;
        }

        private void TriggerCurrentCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = new Current(CurrentStrength, UseRandomCurrentDirection ? randomGenerator.Next(0, 2) % 2 == 0 ? randomGenerator.Next(20, 160) : randomGenerator.Next(200, 340) : CurrentDirection, UseZOnCurrent ? GenerateZStepForCurrent(CurrentZStep, CurrentZStepVariation, elementsSizeSlider.Maximum / ElementsSize, -(ElementsSize - ((ElementsSize / 100d) * ElementsSizeVariation)) + 0.5d) : 0.0d, CurrentMode)
            {
                Deceleration = IgnoreWaterViscosityWhenGeneratingCurrent ? CurrentDeceleration : WaterViscosity,
                Acceleration = IgnoreWaterViscosityWhenGeneratingCurrent ? CurrentAcceleration : WaterViscosity
            };

            TriggerCurrent(c);
        }

        private void ChangeZoomPreviewLocatorModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomPreviewLocatorMode = (ZoomPreviewLocaterMode)e.Parameter;
        }

        private void ChangeCurrentModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentMode = (CurrentSwellStage)e.Parameter;
        }

        private void ToggleCurrentIndicatorCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowCurrentIndicator = !ShowCurrentIndicator;
        }

        private void GenerateNewRandomColourPlanktonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            repopulatePlanktonAfterColourChange();
        }

        private void GenerateNewRandomColourBackgroundCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            setBackgroundBrushFromCurrentSettings(false);
        }

        private void GenerateNewRandomColourSeaBedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            setSeaBedBrushFromCurrentSettings(false);
        }

        private void BreakoutOptionsToExternalWindowCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnBreakoutOptionsToExternalWindow();
        }

        #endregion

        #endregion
    }
}