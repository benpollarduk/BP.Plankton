using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using BP.Plankton.Classes;
using Plankton.Classes;

namespace Plankton.IO
{
    /// <summary>
    /// Represents a settings file for saving and loading Plankton settings.
    /// </summary>
    public sealed class PlanktonSettingsFile
    {
        #region StaticProperties

        /// <summary>
        /// Get the anti-gravity settings.
        /// </summary>
        public static PlanktonSettingsFile AntiGravity => new PlanktonSettingsFile
        {
            Elements = 750,
            ElementsSize = 20,
            ElementsSizeVariation = 0,
            WaterViscosity = 1,
            Travel = 0,
            Life = 0,
            UseRandomElementFill = true,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = true,
            GenerateMultipleRandomElementFill = true,
            GenerateRandomLuminousElementFill = true,
            BubbleSize = 35,
            UseChildBubbles = true,
            ChildBubbleBuoyancy = 0,
            ChildBubbleRate = 3,
            ChildBubbleSizeVariation = 100,
            UseSeaBed = false,
            UsePlanktonAttraction = false,
            PlanktonBrushIndex = 3,
            SeaBrushIndex = 2,
            UseCurrent = false,
            CurrentDirection = 90,
            CurrentRate = 5,
            CurrentStrength = 20,
            CurrentZStepVariation = 25,
            UseRandomCurrentDirection = false,
            CurrentMode = ECurrentSwellStage.MainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 1.0d,
            CurrentDeceleration = 1.0d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the attraction settings.
        /// </summary>
        public static PlanktonSettingsFile Attraction => new PlanktonSettingsFile
        {
            Elements = 500,
            ElementsSize = 10,
            ElementsSizeVariation = 0,
            WaterViscosity = 0.85d,
            Travel = 5,
            Life = 20,
            UseRandomElementFill = false,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = false,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = false,
            BubbleSize = 20,
            UseChildBubbles = true,
            ChildBubbleBuoyancy = 1,
            ChildBubbleRate = 0,
            ChildBubbleSizeVariation = 0,
            UseSeaBed = false,
            UsePlanktonAttraction = true,
            InvertPlanktonAttraction = false,
            PlanktonAttractionReach = 5,
            PlanktonAttractionStrength = 15,
            PlanktonAttractToChildBubbles = true,
            PlanktonBrushIndex = 2,
            SeaBrushIndex = 3,
            SeaBedBrushIndex = 2,
            UseCurrent = true,
            CurrentDirection = 90,
            CurrentRate = 1,
            CurrentStrength = 5,
            CurrentVariation = 100,
            UseZOnCurrent = false,
            CurrentZStep = 50,
            CurrentZStepVariation = 50,
            UseRandomCurrentDirection = true,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the default settings.
        /// </summary>
        public static PlanktonSettingsFile Default => new PlanktonSettingsFile
        {
            Elements = 750,
            ElementsSize = 10,
            ElementsSizeVariation = 75,
            WaterViscosity = 0.95d,
            Travel = 3,
            Life = 0,
            UseRandomElementFill = false,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = false,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = false,
            BubbleSize = 20,
            UseChildBubbles = true,
            ChildBubbleBuoyancy = 1,
            ChildBubbleRate = 5,
            ChildBubbleSizeVariation = 75,
            UseSeaBed = true,
            SeaBedSmoothness = 40,
            SeaBedMaxIncline = 10,
            UseArcSegmentsInSeaBedPath = false,
            UseLineSegmentsInSeaBedPath = true,
            UsePlanktonAttraction = false,
            PlanktonBrushIndex = 0,
            SeaBrushIndex = 0,
            SeaBedBrushIndex = 0,
            UseCurrent = true,
            CurrentDirection = 90,
            CurrentRate = 5,
            CurrentStrength = 20,
            CurrentVariation = 25,
            UseZOnCurrent = false,
            CurrentZStep = 25,
            CurrentZStepVariation = 100,
            UseRandomCurrentDirection = false,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the dense settings.
        /// </summary>
        public static PlanktonSettingsFile Dense => new PlanktonSettingsFile
        {
            Elements = 7500,
            ElementsSize = 5,
            ElementsSizeVariation = 100,
            WaterViscosity = 0.85d,
            Travel = 15,
            Life = 10,
            UseRandomElementFill = false,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = false,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = false,
            BubbleSize = 20,
            UseChildBubbles = false,
            ChildBubbleBuoyancy = 1,
            ChildBubbleRate = 1,
            ChildBubbleSizeVariation = 0,
            UseSeaBed = false,
            UsePlanktonAttraction = true,
            InvertPlanktonAttraction = false,
            PlanktonAttractionReach = 10,
            PlanktonAttractionStrength = 15,
            PlanktonAttractToChildBubbles = false,
            PlanktonBrushIndex = 5,
            SeaBrushIndex = 2,
            SeaBedBrushIndex = 0,
            UseCurrent = false,
            CurrentDirection = 90,
            CurrentRate = 5,
            CurrentStrength = 20,
            CurrentVariation = 25,
            UseZOnCurrent = false,
            CurrentZStep = 50,
            CurrentZStepVariation = 50,
            UseRandomCurrentDirection = false,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the gunk settings.
        /// </summary>
        public static PlanktonSettingsFile Gunk => new PlanktonSettingsFile
        {
            Elements = 500,
            ElementsSize = 75,
            ElementsSizeVariation = 0,
            WaterViscosity = 0.7d,
            Travel = 1,
            Life = 3,
            UseRandomElementFill = false,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = false,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = false,
            BubbleSize = 3,
            UseChildBubbles = false,
            ChildBubbleBuoyancy = 1,
            UseSeaBed = false,
            PlanktonBrushIndex = 4,
            SeaBrushIndex = 2,
            SeaBedBrushIndex = 0,
            UseCurrent = false,
            CurrentDirection = 90,
            CurrentRate = 5,
            CurrentStrength = 20,
            CurrentVariation = 25,
            UseZOnCurrent = false,
            CurrentZStep = 50,
            CurrentZStepVariation = 50,
            UseRandomCurrentDirection = false,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the luminous random start up settings.
        /// </summary>
        public static PlanktonSettingsFile LuminousRandomStartup => new PlanktonSettingsFile
        {
            Elements = 350,
            ElementsSize = 30,
            ElementsSizeVariation = 100,
            WaterViscosity = 0.97d,
            Travel = 10,
            Life = 5,
            UseRandomElementFill = true,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = true,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = true,
            BubbleSize = 25,
            UseChildBubbles = true,
            ChildBubbleBuoyancy = 1,
            ChildBubbleRate = 10,
            ChildBubbleSizeVariation = 100,
            UseSeaBed = true,
            SeaBedSmoothness = 20,
            SeaBedMaxIncline = 3,
            UseArcSegmentsInSeaBedPath = true,
            UseLineSegmentsInSeaBedPath = true,
            UsePlanktonAttraction = false,
            PlanktonAttractionReach = 8,
            PlanktonAttractionStrength = 15,
            PlanktonBrushIndex = 0,
            SeaBrushIndex = 0,
            SeaBedBrushIndex = 0,
            UseCurrent = true,
            CurrentDirection = 90,
            CurrentRate = 10,
            CurrentStrength = 20,
            CurrentVariation = 75,
            UseZOnCurrent = false,
            CurrentZStep = 10,
            CurrentZStepVariation = 75,
            UseRandomCurrentDirection = true,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = true,
            LightingEffectStrength = 0.2d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the original settings.
        /// </summary>
        public static PlanktonSettingsFile Original => new PlanktonSettingsFile
        {
            Elements = 750,
            ElementsSize = 10,
            ElementsSizeVariation = 0,
            WaterViscosity = 0.95d,
            Travel = 5,
            Life = 5,
            UseRandomElementFill = false,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = false,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = false,
            BubbleSize = 20,
            UseChildBubbles = false,
            ChildBubbleBuoyancy = 1,
            UseSeaBed = false,
            UsePlanktonAttraction = false,
            PlanktonBrushIndex = 0,
            SeaBrushIndex = 0,
            SeaBedBrushIndex = 0,
            UseCurrent = false,
            CurrentDirection = 90,
            CurrentRate = 5,
            CurrentStrength = 20,
            CurrentVariation = 25,
            UseZOnCurrent = false,
            CurrentZStep = 50,
            CurrentZStepVariation = 50,
            UseRandomCurrentDirection = false,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the performance settings.
        /// </summary>
        public static PlanktonSettingsFile Performance => new PlanktonSettingsFile
        {
            Elements = 250,
            ElementsSize = 8,
            ElementsSizeVariation = 0,
            WaterViscosity = 0.75d,
            Travel = 5,
            Life = 0,
            UseRandomElementFill = false,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = false,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = false,
            BubbleSize = 20,
            UseChildBubbles = false,
            ChildBubbleBuoyancy = 1,
            UseSeaBed = false,
            UsePlanktonAttraction = false,
            PlanktonBrushIndex = 5,
            SeaBrushIndex = 2,
            SeaBedBrushIndex = 0,
            UseCurrent = false,
            CurrentDirection = 90,
            CurrentRate = 5,
            CurrentStrength = 20,
            CurrentVariation = 25,
            UseZOnCurrent = false,
            CurrentZStep = 50,
            CurrentZStepVariation = 50,
            UseRandomCurrentDirection = false,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        /// <summary>
        /// Get the spikey settings.
        /// </summary>
        public static PlanktonSettingsFile Spikey => new PlanktonSettingsFile
        {
            Elements = 750,
            ElementsSize = 3,
            ElementsSizeVariation = 100,
            WaterViscosity = 0.95d,
            Travel = 15,
            Life = 25,
            UseRandomElementFill = false,
            GenerateAndUseRandomSeaBedBrush = false,
            GenerateAndUseRandomSeaBrush = false,
            GenerateRandomElementFill = false,
            GenerateMultipleRandomElementFill = false,
            GenerateRandomLuminousElementFill = false,
            BubbleSize = 20,
            UseChildBubbles = false,
            ChildBubbleBuoyancy = 1,
            UseSeaBed = true,
            SeaBedSmoothness = 3,
            SeaBedMaxIncline = 10,
            UseArcSegmentsInSeaBedPath = false,
            UseLineSegmentsInSeaBedPath = true,
            UsePlanktonAttraction = false,
            PlanktonBrushIndex = 1,
            SeaBrushIndex = 3,
            SeaBedBrushIndex = 2,
            UseCurrent = false,
            CurrentDirection = 90,
            CurrentRate = 5,
            CurrentStrength = 20,
            CurrentVariation = 25,
            UseZOnCurrent = false,
            CurrentZStep = 50,
            CurrentZStepVariation = 50,
            UseRandomCurrentDirection = false,
            CurrentMode = ECurrentSwellStage.PreMainUp,
            IgnoreWaterViscosityWhenGeneratingCurrent = false,
            CurrentAcceleration = 0.95d,
            CurrentDeceleration = 0.97d,
            UseGravity = false,
            Density = 1.0d,
            UseLightingEffect = false,
            LightingEffectStrength = 0.5d,
            LightingEffectSpeedRatio = 1d
        };

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the full path of this file.
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Get or set the number of elements that should be used.
        /// </summary>
        public int Elements { get; set; }

        /// <summary>
        /// Get or set the size of the elements represented as equal with and height.
        /// </summary>
        public double ElementsSize { get; set; }

        /// <summary>
        /// Get or set the elements size variation, represented as a percentage.
        /// </summary>
        public double ElementsSizeVariation { get; set; }

        /// <summary>
        /// Get or set the amount of travel each element should make along it's vector on each update.
        /// </summary>
        public double Travel { get; set; }

        /// <summary>
        /// Get or set the amount of random life each element has.
        /// </summary>
        public double Life { get; set; }

        /// <summary>
        /// Get or set the size of the bubble represented as equal with and height.
        /// </summary>
        public double BubbleSize { get; set; }

        /// <summary>
        /// Get or set if child bubbles are being used.
        /// </summary>
        public bool UseChildBubbles { get; set; }

        /// <summary>
        /// Get or set the child bubbles buoyancy value.
        /// </summary>
        public double ChildBubbleBuoyancy { get; set; }

        /// <summary>
        /// Get or set the child bubbles rate value.
        /// </summary>
        public double ChildBubbleRate { get; set; }

        /// <summary>
        /// Get or set the child bubble size variation, represented as a percentage.
        /// </summary>
        public double ChildBubbleSizeVariation { get; set; }

        /// <summary>
        /// Get or set if using a sea bed.
        /// </summary>
        public bool UseSeaBed { get; set; }

        /// <summary>
        /// Get or set the sea bed smoothness.
        /// </summary>
        public double SeaBedSmoothness { get; set; }

        /// <summary>
        /// Get or set the sea bed maximum incline.
        /// </summary>
        public double SeaBedMaxIncline { get; set; }

        /// <summary>
        /// Get or set if using plankton attraction to bubbles.
        /// </summary>
        public bool UsePlanktonAttraction { get; set; }

        /// <summary>
        /// Get or set if using plankton attraction is inverted.
        /// </summary>
        public bool InvertPlanktonAttraction { get; set; }

        /// <summary>
        /// Get or set if using plankton attract to child bubbles
        /// </summary>
        public bool PlanktonAttractToChildBubbles { get; set; }

        /// <summary>
        /// Get or set the plankton attraction strength.
        /// </summary>
        public double PlanktonAttractionStrength { get; set; }

        /// <summary>
        /// Get or set the plankton attraction reach.
        /// </summary>
        public double PlanktonAttractionReach { get; set; }

        /// <summary>
        /// Get or set the viscocity of the water.
        /// </summary>
        public double WaterViscosity { get; set; }

        /// <summary>
        /// Get or set if using random element fill.
        /// </summary>
        public bool UseRandomElementFill { get; set; }

        /// <summary>
        /// Get or set if random element fills are generated from random.
        /// </summary>
        public bool GenerateRandomElementFill { get; set; }

        /// <summary>
        /// Get or set if multiple random element fills are generated from random. If this is false just a single fill is generated.
        /// </summary>
        public bool GenerateMultipleRandomElementFill { get; set; }

        /// <summary>
        /// Get or set if luminous random element fills are generated from random.
        /// </summary>
        public bool GenerateRandomLuminousElementFill { get; set; }

        /// <summary>
        /// Get or set the brush index used to render the plankton elements.
        /// </summary>
        public int PlanktonBrushIndex { get; set; }

        /// <summary>
        /// Get or set the brush index used to render the sea bed.
        /// </summary>
        public int SeaBedBrushIndex { get; set; }

        /// <summary>
        /// Get or set the brush index used to render the sea.
        /// </summary>
        public int SeaBrushIndex { get; set; }

        /// <summary>
        /// Get or set if a random sea bed brush is generated and used.
        /// </summary>
        public bool GenerateAndUseRandomSeaBedBrush { get; set; }

        /// <summary>
        /// Get or set if a random sea brush is generated and used.
        /// </summary>
        public bool GenerateAndUseRandomSeaBrush { get; set; }

        /// <summary>
        /// Get or set if arc segements are used in sea bed paths.
        /// </summary>
        public bool UseArcSegmentsInSeaBedPath { get; set; }

        /// <summary>
        /// Get or set if line segements are used in sea bed paths.
        /// </summary>
        public bool UseLineSegmentsInSeaBedPath { get; set; }

        /// <summary>
        /// Get or set if the current is being used.
        /// </summary>
        public bool UseCurrent { get; set; }

        /// <summary>
        /// Get or set the current rate.
        /// </summary>
        public double CurrentRate { get; set; }

        /// <summary>
        /// Get or set the current variation, represented as a percentage.
        /// </summary>
        public double CurrentVariation { get; set; }

        /// <summary>
        /// Get or set the strength of the current.
        /// </summary>
        public double CurrentStrength { get; set; }

        /// <summary>
        /// Get or set the direction of the current, in degrees.
        /// </summary>
        public double CurrentDirection { get; set; }

        /// <summary>
        /// Get or set if a random current direction is used.
        /// </summary>
        public bool UseRandomCurrentDirection { get; set; }

        /// <summary>
        /// Get or set if a Z value is applied to the current.
        /// </summary>
        public bool UseZOnCurrent { get; set; }

        /// <summary>
        /// Get or set the current Z step.
        /// </summary>
        public double CurrentZStep { get; set; }

        /// <summary>
        /// Get or set the current Z step variation, represented as a percentage.
        /// </summary>
        public double CurrentZStepVariation { get; set; }

        /// <summary>
        /// Get or set the current mode.
        /// </summary>
        public ECurrentSwellStage CurrentMode { get; set; } = ECurrentSwellStage.PreMainUp;

        /// <summary>
        /// Get or set if the water viscosity is ignore when generating a current.
        /// </summary>
        public bool IgnoreWaterViscosityWhenGeneratingCurrent { get; set; }

        /// <summary>
        /// Get or set the current acceleration.
        /// </summary>
        public double CurrentAcceleration { get; set; }

        /// <summary>
        /// Get or set the current deceleration.
        /// </summary>
        public double CurrentDeceleration { get; set; }

        /// <summary>
        /// Get or set if gravity is used.
        /// </summary>
        public bool UseGravity { get; set; }

        /// <summary>
        /// Get or set the density of elements.
        /// </summary>
        public double Density { get; set; }

        /// <summary>
        /// Get or set if the lighting effect is used.
        /// </summary>
        public bool UseLightingEffect { get; set; }

        /// <summary>
        /// Get or set the strength of the lighting effect.
        /// </summary>
        public double LightingEffectStrength { get; set; }

        /// <summary>
        /// Get or set the speed ratio of the lighting effect.
        /// </summary>
        public double LightingEffectSpeedRatio { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Save this PlanktonSettingsFile.
        /// </summary>
        public void Save()
        {
            var doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlNode settingsNode = doc.CreateElement("Settings");
            XmlNode planktonNode = doc.CreateElement("Plankton");
            XmlNode bubblesNode = doc.CreateElement("Bubbles");
            XmlNode massNode = doc.CreateElement("Mass");
            XmlNode currentNode = doc.CreateElement("Current");
            XmlNode environmentNode = doc.CreateElement("Environment");

            if ((settingsNode == null) ||
                (planktonNode == null) ||
                (bubblesNode == null) ||
                (massNode == null) ||
                (currentNode == null) ||
                (environmentNode == null))
            {
                throw new NullReferenceException();
            }

            var attribute = doc.CreateAttribute("Elements");
            attribute.Value = Elements.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("ElementsSize");
            attribute.Value = ElementsSize.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("ElementsSizeVariation");
            attribute.Value = ElementsSizeVariation.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("Travel");
            attribute.Value = Travel.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("Life");
            attribute.Value = Life.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UsePlanktonAttraction");
            attribute.Value = UsePlanktonAttraction.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("InvertPlanktonAttraction");
            attribute.Value = InvertPlanktonAttraction.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("PlanktonAttractToChildBubbles");
            attribute.Value = PlanktonAttractToChildBubbles.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("PlanktonAttractionReach");
            attribute.Value = PlanktonAttractionReach.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("PlanktonAttractionStrength");
            attribute.Value = PlanktonAttractionStrength.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseRandomElementFill");
            attribute.Value = UseRandomElementFill.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("GenerateMultipleRandomElementFill");
            attribute.Value = GenerateMultipleRandomElementFill.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("GenerateRandomElementFill");
            attribute.Value = GenerateRandomElementFill.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("GenerateRandomLuminousElementFill");
            attribute.Value = GenerateRandomLuminousElementFill.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("PlanktonBrushIndex");
            attribute.Value = PlanktonBrushIndex.ToString(CultureInfo.InvariantCulture);
            planktonNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("BubbleSize");
            attribute.Value = BubbleSize.ToString(CultureInfo.InvariantCulture);
            bubblesNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseChildBubbles");
            attribute.Value = UseChildBubbles.ToString(CultureInfo.InvariantCulture);
            bubblesNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("ChildBubbleBuoyancy");
            attribute.Value = ChildBubbleBuoyancy.ToString(CultureInfo.InvariantCulture);
            bubblesNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("ChildBubbleRate");
            attribute.Value = ChildBubbleRate.ToString(CultureInfo.InvariantCulture);
            bubblesNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("ChildBubbleSizeVariation");
            attribute.Value = ChildBubbleSizeVariation.ToString(CultureInfo.InvariantCulture);
            bubblesNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseGravity");
            attribute.Value = UseGravity.ToString(CultureInfo.InvariantCulture);
            massNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("Density");
            attribute.Value = Density.ToString(CultureInfo.InvariantCulture);
            massNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseCurrent");
            attribute.Value = UseCurrent.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentDirection");
            attribute.Value = CurrentDirection.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentRate");
            attribute.Value = CurrentRate.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentStrength");
            attribute.Value = CurrentStrength.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentVariation");
            attribute.Value = CurrentVariation.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseRandomCurrentDirection");
            attribute.Value = UseRandomCurrentDirection.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseZOnCurrent");
            attribute.Value = UseZOnCurrent.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentZStep");
            attribute.Value = CurrentZStep.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentZStepVariation");
            attribute.Value = CurrentZStepVariation.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentMode");
            attribute.Value = CurrentMode.ToString();
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("IgnoreWaterViscosityWhenGeneratingCurrent");
            attribute.Value = IgnoreWaterViscosityWhenGeneratingCurrent.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentAcceleration");
            attribute.Value = CurrentAcceleration.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("CurrentDeceleration");
            attribute.Value = CurrentDeceleration.ToString(CultureInfo.InvariantCulture);
            currentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseSeaBed");
            attribute.Value = UseSeaBed.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("SeaBedSmoothness");
            attribute.Value = SeaBedSmoothness.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("SeaBedMaxIncline");
            attribute.Value = SeaBedMaxIncline.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseArcSegmentsInSeaBedPath");
            attribute.Value = UseArcSegmentsInSeaBedPath.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseLineSegmentsInSeaBedPath");
            attribute.Value = UseLineSegmentsInSeaBedPath.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("SeaBedBrushIndex");
            attribute.Value = SeaBedBrushIndex.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("SeaBrushIndex");
            attribute.Value = SeaBrushIndex.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("GenerateAndUseRandomSeaBedBrush");
            attribute.Value = GenerateAndUseRandomSeaBedBrush.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("GenerateAndUseRandomSeaBrush");
            attribute.Value = GenerateAndUseRandomSeaBrush.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("WaterViscosity");
            attribute.Value = WaterViscosity.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("UseLightingEffect");
            attribute.Value = UseLightingEffect.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("LightingEffectStrength");
            attribute.Value = LightingEffectStrength.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            attribute = doc.CreateAttribute("LightingEffectSpeedRatio");
            attribute.Value = LightingEffectSpeedRatio.ToString(CultureInfo.InvariantCulture);
            environmentNode.Attributes?.Append(attribute);

            // append child nodes
            settingsNode.AppendChild(planktonNode);
            settingsNode.AppendChild(bubblesNode);
            settingsNode.AppendChild(currentNode);
            settingsNode.AppendChild(environmentNode);


            doc.AppendChild(settingsNode);

            doc.Save(FullPath);
        }

        #endregion

        #region StaticMethods

        /// <summary>
        /// Open an existing PlanktonSettingsFile.
        /// </summary>
        /// <param name="path">The path of the file to open.</param>
        /// <returns>The loaded PlanktonSettingsFile.</returns>
        public static PlanktonSettingsFile Open(string path)
        {
            string data;

            using (var reader = new StreamReader(path))
                data = reader.ReadToEnd();

            var doc = new XmlDocument();
            doc.LoadXml(data);

            var file = new PlanktonSettingsFile();
            if (AttributeExists(doc, "Plankton", "Elements"))
                file.Elements = int.Parse(GetAttribute(doc, "Plankton", "Elements").Value);

            if (AttributeExists(doc, "Plankton", "ElementsSize"))
                file.ElementsSize = double.Parse(GetAttribute(doc, "Plankton", "ElementsSize").Value);

            if (AttributeExists(doc, "Plankton", "ElementsSizeVariation"))
                file.ElementsSizeVariation = double.Parse(GetAttribute(doc, "Plankton", "ElementsSizeVariation").Value);

            if (AttributeExists(doc, "Plankton", "Travel"))
                file.Travel = double.Parse(GetAttribute(doc, "Plankton", "Travel").Value);

            if (AttributeExists(doc, "Plankton", "Life"))
                file.Life = double.Parse(GetAttribute(doc, "Plankton", "Life").Value);

            if (AttributeExists(doc, "Plankton", "UsePlanktonAttraction"))
                file.UsePlanktonAttraction = bool.Parse(GetAttribute(doc, "Plankton", "UsePlanktonAttraction").Value);

            if (AttributeExists(doc, "Plankton", "InvertPlanktonAttraction"))
                file.InvertPlanktonAttraction = bool.Parse(GetAttribute(doc, "Plankton", "InvertPlanktonAttraction").Value);

            if (AttributeExists(doc, "Plankton", "PlanktonAttractToChildBubbles"))
                file.PlanktonAttractToChildBubbles = bool.Parse(GetAttribute(doc, "Plankton", "PlanktonAttractToChildBubbles").Value);

            if (AttributeExists(doc, "Plankton", "PlanktonAttractionReach"))
                file.PlanktonAttractionReach = double.Parse(GetAttribute(doc, "Plankton", "PlanktonAttractionReach").Value);

            if (AttributeExists(doc, "Plankton", "PlanktonAttractionStrength"))
                file.PlanktonAttractionStrength = double.Parse(GetAttribute(doc, "Plankton", "PlanktonAttractionStrength").Value);

            if (AttributeExists(doc, "Plankton", "UseRandomElementFill"))
                file.UseRandomElementFill = bool.Parse(GetAttribute(doc, "Plankton", "UseRandomElementFill").Value);

            if (AttributeExists(doc, "Plankton", "GenerateMultipleRandomElementFill"))
                file.GenerateMultipleRandomElementFill = bool.Parse(GetAttribute(doc, "Plankton", "GenerateMultipleRandomElementFill").Value);

            if (AttributeExists(doc, "Plankton", "GenerateRandomElementFill"))
                file.GenerateRandomElementFill = bool.Parse(GetAttribute(doc, "Plankton", "GenerateRandomElementFill").Value);

            if (AttributeExists(doc, "Plankton", "GenerateRandomLuminousElementFill"))
                file.GenerateRandomLuminousElementFill = bool.Parse(GetAttribute(doc, "Plankton", "GenerateRandomLuminousElementFill").Value);

            if (AttributeExists(doc, "Plankton", "PlanktonBrushIndex"))
                file.PlanktonBrushIndex = int.Parse(GetAttribute(doc, "Plankton", "PlanktonBrushIndex").Value);

            if (AttributeExists(doc, "Bubbles", "BubbleSize"))
                file.BubbleSize = double.Parse(GetAttribute(doc, "Bubbles", "BubbleSize").Value);

            if (AttributeExists(doc, "Bubbles", "UseChildBubbles"))
                file.UseChildBubbles = bool.Parse(GetAttribute(doc, "Bubbles", "UseChildBubbles").Value);

            if (AttributeExists(doc, "Bubbles", "ChildBubbleBuoyancy"))
                file.ChildBubbleBuoyancy = double.Parse(GetAttribute(doc, "Bubbles", "ChildBubbleBuoyancy").Value);

            if (AttributeExists(doc, "Bubbles", "ChildBubbleRate"))
                file.ChildBubbleRate = double.Parse(GetAttribute(doc, "Bubbles", "ChildBubbleRate").Value);

            if (AttributeExists(doc, "Bubbles", "ChildBubbleSizeVariation"))
                file.ChildBubbleSizeVariation = double.Parse(GetAttribute(doc, "Bubbles", "ChildBubbleSizeVariation").Value);

            if (AttributeExists(doc, "Mass", "UseGravity"))
                file.UseGravity = bool.Parse(GetAttribute(doc, "Mass", "UseGravity").Value);

            if (AttributeExists(doc, "Mass", "Density"))
                file.Density = double.Parse(GetAttribute(doc, "Mass", "Density").Value);

            if (AttributeExists(doc, "Current", "UseCurrent"))
                file.UseCurrent = bool.Parse(GetAttribute(doc, "Current", "UseCurrent").Value);

            if (AttributeExists(doc, "Current", "CurrentDirection"))
                file.CurrentDirection = double.Parse(GetAttribute(doc, "Current", "CurrentDirection").Value);

            if (AttributeExists(doc, "Current", "CurrentRate"))
                file.CurrentRate = double.Parse(GetAttribute(doc, "Current", "CurrentRate").Value);

            if (AttributeExists(doc, "Current", "CurrentStrength"))
                file.CurrentStrength = double.Parse(GetAttribute(doc, "Current", "CurrentStrength").Value);

            if (AttributeExists(doc, "Current", "CurrentVariation"))
                file.CurrentVariation = double.Parse(GetAttribute(doc, "Current", "CurrentVariation").Value);

            if (AttributeExists(doc, "Current", "UseRandomCurrentDirection"))
                file.UseRandomCurrentDirection = bool.Parse(GetAttribute(doc, "Current", "UseRandomCurrentDirection").Value);

            if (AttributeExists(doc, "Current", "UseZOnCurrent"))
                file.UseZOnCurrent = bool.Parse(GetAttribute(doc, "Current", "UseZOnCurrent").Value);

            if (AttributeExists(doc, "Current", "CurrentZStep"))
                file.CurrentZStep = double.Parse(GetAttribute(doc, "Current", "CurrentZStep").Value);

            if (AttributeExists(doc, "Current", "CurrentZStepVariation"))
                file.CurrentZStepVariation = double.Parse(GetAttribute(doc, "Current", "CurrentZStepVariation").Value);

            if (AttributeExists(doc, "Current", "CurrentMode"))
                file.CurrentMode = (ECurrentSwellStage)Enum.Parse(typeof (ECurrentSwellStage), GetAttribute(doc, "Current", "CurrentMode").Value);

            if (AttributeExists(doc, "Current", "IgnoreWaterViscosityWhenGeneratingCurrent"))
                file.IgnoreWaterViscosityWhenGeneratingCurrent = bool.Parse(GetAttribute(doc, "Current", "IgnoreWaterViscosityWhenGeneratingCurrent").Value);

            if (AttributeExists(doc, "Current", "CurrentAcceleration"))
                file.CurrentAcceleration = double.Parse(GetAttribute(doc, "Current", "CurrentAcceleration").Value);

            if (AttributeExists(doc, "Current", "CurrentDeceleration"))
                file.CurrentDeceleration = double.Parse(GetAttribute(doc, "Current", "CurrentDeceleration").Value);

            if (AttributeExists(doc, "Environment", "UseSeaBed"))
                file.UseSeaBed = bool.Parse(GetAttribute(doc, "Environment", "UseSeaBed").Value);

            if (AttributeExists(doc, "Environment", "SeaBedSmoothness"))
                file.SeaBedSmoothness = double.Parse(GetAttribute(doc, "Environment", "SeaBedSmoothness").Value);

            if (AttributeExists(doc, "Environment", "SeaBedMaxIncline"))
                file.SeaBedMaxIncline = double.Parse(GetAttribute(doc, "Environment", "SeaBedMaxIncline").Value);

            if (AttributeExists(doc, "Environment", "SeaBedBrushIndex"))
                file.SeaBedBrushIndex = int.Parse(GetAttribute(doc, "Environment", "SeaBedBrushIndex").Value);

            if (AttributeExists(doc, "Environment", "SeaBrushIndex"))
                file.SeaBrushIndex = int.Parse(GetAttribute(doc, "Environment", "SeaBrushIndex").Value);

            if (AttributeExists(doc, "Environment", "GenerateAndUseRandomSeaBedBrush"))
                file.GenerateAndUseRandomSeaBedBrush = bool.Parse(GetAttribute(doc, "Environment", "GenerateAndUseRandomSeaBedBrush").Value);

            if (AttributeExists(doc, "Environment", "GenerateAndUseRandomSeaBrush"))
                file.GenerateAndUseRandomSeaBrush = bool.Parse(GetAttribute(doc, "Environment", "GenerateAndUseRandomSeaBrush").Value);
            if (AttributeExists(doc, "Environment", "UseLineSegmentsInSeaBedPath"))
                file.UseLineSegmentsInSeaBedPath = bool.Parse(GetAttribute(doc, "Environment", "UseLineSegmentsInSeaBedPath").Value);

            if (AttributeExists(doc, "Environment", "UseArcSegmentsInSeaBedPath"))
                file.UseArcSegmentsInSeaBedPath = bool.Parse(GetAttribute(doc, "Environment", "UseArcSegmentsInSeaBedPath").Value);

            if (AttributeExists(doc, "Environment", "WaterViscosity"))
                file.WaterViscosity = double.Parse(GetAttribute(doc, "Environment", "WaterViscosity").Value);

            if (AttributeExists(doc, "Environment", "UseLightingEffect"))
                file.UseLightingEffect = bool.Parse(GetAttribute(doc, "Environment", "UseLightingEffect").Value);

            if (AttributeExists(doc, "Environment", "LightingEffectStrength"))
                file.LightingEffectStrength = double.Parse(GetAttribute(doc, "Environment", "LightingEffectStrength").Value);

            if (AttributeExists(doc, "Environment", "LightingEffectSpeedRatio"))
                file.LightingEffectSpeedRatio = double.Parse(GetAttribute(doc, "Environment", "LightingEffectSpeedRatio").Value);

            return file;
        }

        /// <summary>
        /// Open an existing PlanktonSettingsFile.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The created PlanktonSettingsFile.</returns>
        public PlanktonSettingsFile CreateNew(string path)
        {
            return new PlanktonSettingsFile { FullPath = path };
        }

        /// <summary>
        /// Get if a node exists.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="tagName">The tag to search for.</param>
        /// <returns>True if it exists, else false.</returns>
        public static bool NodeExists(XmlDocument doc, string tagName)
        {
            return doc.Cast<XmlNode>().Any(node => node.Name == tagName);
        }

        /// <summary>
        /// Get if a attribue exists.
        /// </summary>
        /// <param name="node">The node to search for.</param>
        /// <param name="attributeName">The attribute to search for.</param>
        /// <returns>True if it exists, else false.</returns>
        public static bool AttributeExists(XmlNode node, string attributeName)
        {
            return node.Attributes != null && node.Attributes.Cast<XmlAttribute>().Any(attribute => attribute.Name == attributeName);
        }

        /// <summary>
        /// Get if a attribue exists.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="tagName">The tag to search for.</param>
        /// <param name="attributeName">The attribute to search for.</param>
        /// <returns>True if it exists, else false.</returns>
        public static bool AttributeExists(XmlDocument doc, string tagName, string attributeName)
        {
            foreach (XmlNode node in doc.GetElementsByTagName(tagName))
            {
                if (node?.Attributes == null)
                    continue;

                foreach (XmlAttribute attribute in node.Attributes)
                    if (attribute.Name == attributeName)
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Get an attribute.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="tagName">The tag to search for.</param>
        /// <param name="attributeName">The attribute to search for.</param>
        /// <returns>The located attribute.</returns>
        public static XmlAttribute GetAttribute(XmlDocument doc, string tagName, string attributeName)
        {
            foreach (XmlNode node in doc.GetElementsByTagName(tagName))
            {
                if (node?.Attributes == null)
                    continue;

                foreach (XmlAttribute attribute in node.Attributes)
                    if (attribute.Name == attributeName)
                        return attribute;
            }

            return null;
        }

        /// <summary>
        /// Get a inner node from a node at a specified index.
        /// </summary>
        /// <param name="node">The parent node to search.</param>
        /// <param name="attributeName">The attribute that is being searched for.</param>
        /// <returns>The located attribute.</returns>
        public static XmlAttribute GetAttribute(XmlNode node, string attributeName)
        {
            if (node?.Attributes == null)
                return null;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.Name == attributeName)
                    return attribute;
            }

            return null;
        }

        /// <summary>
        /// Get a inner node from a node at a specified index.
        /// </summary>
        /// <param name="node">The parent node to search.</param>
        /// <param name="index">The index of the node.</param>
        /// <returns>The located node.</returns>
        public static XmlNode GetNode(XmlNode node, short index)
        {
            return index < node?.Attributes?.Count ? node.ChildNodes[index] : null;
        }

        /// <summary>
        /// Get a node.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        /// <param name="tagName">The tag name to search for.</param>
        /// <returns>The located node.</returns>
        public static XmlNode GetNode(XmlDocument doc, string tagName)
        {
            return doc.Cast<XmlNode>().FirstOrDefault(node => node.Name == tagName);
        }

        /// <summary>
        /// Get a node.
        /// </summary>
        /// <param name="parentNode">The parent node to search.</param>
        /// <param name="tagName">The tag name to search for.</param>
        /// <returns>The located node.</returns>
        public static XmlNode GetNode(XmlNode parentNode, string tagName)
        {
            return parentNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.Name == tagName);
        }

        #endregion
    }
}