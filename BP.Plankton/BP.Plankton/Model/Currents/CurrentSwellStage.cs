namespace BP.Plankton.Model.Currents
{
    /// <summary>
    /// Enumeration of swell stages for currents.
    /// </summary>
    public enum CurrentSwellStage
    {
        /// <summary>
        /// Ramping up for main.
        /// </summary>
        MainUp = 0,
        /// <summary>
        /// Ramping down from main.
        /// </summary>
        MainDown,
        /// <summary>
        /// Ramping up for pre main.
        /// </summary>
        PreMainUp,
        /// <summary>
        /// Ramping down for pre main.
        /// </summary>
        PreMainDown
    }
}