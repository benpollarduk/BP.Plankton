using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace BP.Plankton.Model.Currents
{
    /// <summary>
    /// Represents an underwater current.
    /// </summary>
    public class Current
    {
        #region Constants

        private const double StrengthRealignment = 0.1d;

        #endregion

        #region StaticProperties

        private static readonly Vector Stationary = new Vector(0, 0);
        private static readonly Vector3D Stationary3D = new Vector3D(0, 0, 0);
        
        #endregion

        #region Fields

        private Vector relativeDirection = Stationary;
        private Vector preCurrentRelativeDirection = Stationary;
        private Vector3D vector = Stationary3D;
        private double totalZAdjustement;

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the strength.
        /// </summary>
        public double Strength { get; set; } = 1;

        /// <summary>
        /// Get if this is currently active.
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Get or set the direction in degrees.
        /// </summary>
        public double Direction { get; set; }

        /// <summary>
        /// Get the swell stage.
        /// </summary>
        public CurrentSwellStage SwellStage { get; private set; } = CurrentSwellStage.PreMainUp;

        /// <summary>
        /// Get or set the acceleration ratio within a range of 0.0 and 1.0 where 1.0 is infinite.
        /// </summary>
        public double Acceleration { get; set; } = 0.95d;

        /// <summary>
        /// Get or set the deceleration ratio within a range of 0.0 and 1.0 where 1.0 is infinite.
        /// </summary>
        public double Deceleration { get; set; } = 0.97d;

        /// <summary>
        /// Get or set a factor that is used when calculating acceleration for the pre-current. This is used as a divisor between the difference of 1 - acceleration.
        /// </summary>
        public double PreCurrentAccelerationFactor { get; set; } = 4d;

        /// <summary>
        /// Get or set a factor that is used when calculating deceleration for the pre-current. This is used as a divisor between the difference of 1 - deceleration.
        /// </summary>
        public double PreCurrentDecelerationFactor { get; set; } = 1d;

        /// <summary>
        /// Get or set a factor that is used when calculating strength for the pre-current. This is used to divide the Strength to obtain a lesser appearance of current.
        /// </summary>
        public double PreCurrentStrengthFactor { get; set; } = 8d;

        /// <summary>
        /// Get or set an adjustment on Z that is applied for each step of the current.
        /// </summary>
        public double ZAdjustmentPerStep { get; set; }

        /// <summary>
        /// Get or set the maximum z adjustment.
        /// </summary>
        public double MaximumZAdjustment { get; set; } = 50d;

        /// <summary>
        /// Get or set the minimum z adjustment.
        /// </summary>
        public double MinimumZAdjustment { get; set; } = -10d;

        /// <summary>
        /// Get or set the minimum X/Y z movement before it classed as zero. This allows the vector of the current to be zeroed when it becomes so small as to be irrelevant.
        /// </summary>
        public double MinimumXYMovementBeforeZeroing { get; set; } = 0.25d;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Current class.
        /// </summary>
        public Current()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Current class.
        /// </summary>
        /// <param name="strength">Specify the strength.</param>
        /// <param name="direction">Specify the direction in degrees.</param>
        public Current(double strength, double direction)
        {
            Strength = strength;
            Direction = direction;
            ZAdjustmentPerStep = 0.0d;
        }

        /// <summary>
        /// Initializes a new instance of the Current class.
        /// </summary>
        /// <param name="strength">Specify the strength.</param>
        /// <param name="direction">Specify the direction in degrees.</param>
        /// <param name="zAdjustment">Specify an adjustment on Z that is applied for each step of the current.</param>
        public Current(double strength, double direction, double zAdjustment)
        {
            Strength = strength;
            Direction = direction;
            ZAdjustmentPerStep = zAdjustment;
        }

        /// <summary>
        /// Initializes a new instance of the Current class.
        /// </summary>
        /// <param name="strength">Specify the strength.</param>
        /// <param name="direction">Specify the direction in degrees.</param>
        /// <param name="entryPoint">Specify the entry point of the current.</param>
        public Current(double strength, double direction, CurrentSwellStage entryPoint)
        {
            Strength = strength;
            Direction = direction;
            ZAdjustmentPerStep = 0.0d;
            SwellStage = entryPoint;
        }

        /// <summary>
        /// Initializes a new instance of the Current class.
        /// </summary>
        /// <param name="strength">Specify the strength.</param>
        /// <param name="direction">Specify the direction in degrees.</param>
        /// <param name="zAdjustment">Specify an adjustment on Z that is applied for each step of the current.</param>
        /// <param name="entryPoint">Specify the entry point of the current.</param>
        public Current(double strength, double direction, double zAdjustment, CurrentSwellStage entryPoint)
        {
            Strength = strength;
            Direction = direction;
            ZAdjustmentPerStep = zAdjustment;
            SwellStage = entryPoint;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Stop any active movement this Current.
        /// </summary>
        public virtual void Stop()
        {
            vector = Stationary3D;
            IsActive = false;
        }

        /// <summary>
        /// Start active movement for this Current.
        /// </summary>
        public virtual void Start()
        {
            IsActive = true;
            Start(SwellStage);
        }

        /// <summary>
        /// Start active movement for this Current.
        /// </summary>
        /// <param name="entryPoint">Specify the entry point of the current.</param>
        public virtual void Start(CurrentSwellStage entryPoint)
        {
            IsActive = true;
            SwellStage = entryPoint;

            switch (SwellStage)
            {
                case (CurrentSwellStage.MainDown):

                        // set vector
                        relativeDirection.X = Math.Sin(Direction / (180 / Math.PI));
                        relativeDirection.Y = -Math.Cos(Direction / (180 / Math.PI));

                        // ensure fully ramped up to start
                        while (relativeDirection.Length < (Strength * StrengthRealignment))
                        {
                            relativeDirection.X /= Acceleration;
                            relativeDirection.Y /= Acceleration;
                        }

                        break;
                case (CurrentSwellStage.MainUp):

                        // set vector
                        relativeDirection.X = Math.Sin(Direction / (180 / Math.PI));
                        relativeDirection.Y = -Math.Cos(Direction / (180 / Math.PI));

                        break;
                case (CurrentSwellStage.PreMainDown):

                        // set vector
                        relativeDirection.X = Math.Sin(Direction / (180 / Math.PI));
                        relativeDirection.Y = -Math.Cos(Direction / (180 / Math.PI));

                        // set pre-current vector
                        preCurrentRelativeDirection.X = -relativeDirection.X;
                        preCurrentRelativeDirection.Y = -relativeDirection.Y;

                        // ensure fully ramped up first
                        while (preCurrentRelativeDirection.Length < (Strength * StrengthRealignment) / PreCurrentStrengthFactor)
                        {
                            preCurrentRelativeDirection.X /= (Acceleration - ((1 - Acceleration) / PreCurrentAccelerationFactor));
                            preCurrentRelativeDirection.Y /= (Acceleration - ((1 - Acceleration) / PreCurrentAccelerationFactor));
                        }

                        break;
                case (CurrentSwellStage.PreMainUp):

                        // set vector
                        relativeDirection.X = Math.Sin(Direction / (180 / Math.PI));
                        relativeDirection.Y = -Math.Cos(Direction / (180 / Math.PI));

                        // set pre-current vector
                        preCurrentRelativeDirection.X = -relativeDirection.X;
                        preCurrentRelativeDirection.Y = -relativeDirection.Y;

                        break;
                default: throw new NotImplementedException();
            }

            IncrementToNextStep();
        }

        /// <summary>
        /// Get the current strength of the Current compared to the total strength. This is returned as a percentage.
        /// </summary>
        /// <returns>The current strength as a percentage of the total strength.</returns>
        public double GetCurrentStrengthOfTotalStrength()
        {
            return (100d / (Strength * StrengthRealignment)) * Get2DVectorLength(vector);
        }

        /// <summary>
        /// Get the active step for this Current.
        /// </summary>
        /// <returns>A vector describing the effect of this Current.</returns>
        public virtual Vector3D ActiveStep()
        {
            return vector;
        }

        /// <summary>
        /// Increment the effect of this current to the next step.
        /// </summary>
        public virtual void IncrementToNextStep()
        { 
            const double tollerance = 0.0d;

            switch (SwellStage)
            {
                case CurrentSwellStage.MainDown:

                        // apply adjustment per step
                        vector.Z = ZAdjustmentPerStep * Deceleration;

                        if (Math.Abs(vector.X) > tollerance)
                        {
                            // update vector
                            vector.X = relativeDirection.X *= Deceleration;

                            // if been reduced to an irrelevant amount
                            if (Math.Abs(vector.X) < MinimumXYMovementBeforeZeroing)
                                vector.X = 0.0d;
                        }

                        if (Math.Abs(vector.Y) > tollerance)
                        {
                            // update vector
                            vector.Y = relativeDirection.Y *= Deceleration;

                            // if been reduced to an irrelevant amount
                            if (Math.Abs(vector.Y) < MinimumXYMovementBeforeZeroing)
                                vector.Y = 0.0d;
                        }

                        // if now stationary
                        if (Math.Abs(Get2DVectorLength(vector)) < tollerance)
                            Stop();

                        break;
                case CurrentSwellStage.MainUp:

                        // apply adjustment per step
                        vector.Z = ZAdjustmentPerStep / Acceleration;

                        // get return vector
                        vector.X = relativeDirection.X /= Acceleration;
                        vector.Y = relativeDirection.Y /= Acceleration;

                        // if hit full strength
                        if (Get2DVectorLength(vector) >= (Strength * StrengthRealignment))
                            SwellStage = CurrentSwellStage.MainDown;

                        break;
                case CurrentSwellStage.PreMainDown:

                        // apply adjustment per step
                        vector.Z = ZAdjustmentPerStep * (Deceleration - ((1 - Deceleration) / PreCurrentDecelerationFactor));

                        if (Math.Abs(vector.X) > tollerance)
                        {
                            // update vector
                            vector.X = preCurrentRelativeDirection.X *= (Deceleration - ((1 - Deceleration) / PreCurrentDecelerationFactor));

                            // if been reduced to an irrelevant amount
                            if (Math.Abs(vector.X) < MinimumXYMovementBeforeZeroing)
                                vector.X = 0.0d;
                        }

                        if (Math.Abs(vector.Y) > tollerance)
                        {
                            // update vector
                            vector.Y = preCurrentRelativeDirection.Y *= (Deceleration - ((1 - Deceleration) / PreCurrentDecelerationFactor));

                            // if been reduced to an irrelevant amount
                            if (Math.Abs(vector.Y) < MinimumXYMovementBeforeZeroing)
                                vector.Y = 0.0d;
                        }

                        if (Math.Abs(Get2DVectorLength(vector)) < tollerance)
                            SwellStage = CurrentSwellStage.MainUp;

                        break;
                case CurrentSwellStage.PreMainUp:

                        // apply adjustment per step
                        vector.Z = ZAdjustmentPerStep / (Acceleration - ((1 - Acceleration) / PreCurrentAccelerationFactor));

                        // get return vector
                        vector.X = preCurrentRelativeDirection.X /= (Acceleration - ((1 - Acceleration) / PreCurrentAccelerationFactor));
                        vector.Y = preCurrentRelativeDirection.Y /= (Acceleration - ((1 - Acceleration) / PreCurrentAccelerationFactor));

                        // if hit pre strength for retraction
                        if (Get2DVectorLength(vector) >= (Strength * StrengthRealignment) / PreCurrentStrengthFactor)
                            SwellStage = CurrentSwellStage.PreMainDown;

                        break;
                default: throw new NotImplementedException();
            }

            // check range of adjustment
            if ((totalZAdjustement + vector.Z >= MaximumZAdjustment) && (vector.Z > 0))
                vector.Z = 0.0d;
            else if ((totalZAdjustement + vector.Z <= MinimumZAdjustment) && (vector.Z < 0))
                vector.Z = 0.0d;
            else
                totalZAdjustement += vector.Z;
        }

        #endregion

        #region StaticMethods

        /// <summary>
        /// Get a 2D vector length from the X and Y values of a 3D vector.
        /// </summary>
        /// <param name="vector3D">The 3D vector to use in the length calculation.</param>
        /// <returns>The length of the 3D vector as if it were a 2D vector.</returns>
        public static double Get2DVectorLength(Vector3D vector3D)
        {
            return Math.Abs(Math.Sqrt((vector3D.X * vector3D.X) + (vector3D.Y * vector3D.Y)));
        }

        #endregion
    }
}