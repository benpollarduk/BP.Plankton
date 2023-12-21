namespace BP.Plankton.Model
{
    /// <summary>
    /// Enumeration of zoom preview locater modes.
    /// </summary>
    public enum ZoomPreviewLocaterMode
    {
        /// <summary>
        /// Never show the locater.
        /// </summary>
        Never = 0,
        /// <summary>
        /// Always show the locater.
        /// </summary>
        Always,
        /// <summary>
        /// Only show locater with plankton.
        /// </summary>
        OnlyPlankton,
        /// <summary>
        /// Only show with child bubbles.
        /// </summary>
        OnlyChildBubbles,
        /// <summary>
        /// Only the main bubble.
        /// </summary>
        OnlyMainBubble,
        /// <summary>
        /// Always show except with plankton.
        /// </summary>
        AnythingButPlankton,
        /// <summary>
        /// Always show except with main bubble.
        /// </summary>
        AnythingButMainBubble
    }
}
