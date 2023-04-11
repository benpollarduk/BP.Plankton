namespace Plankton.Classes
{
    /// <summary>
    /// Enumertaion of zoom preview locator modes
    /// </summary>
    public enum ZoomPreviewLocatorMode
    {
        /// <summary>
        /// Never show the locator
        /// </summary>
        Never = 0,
        /// <summary>
        /// Always show the locator
        /// </summary>
        Always,
        /// <summary>
        /// Only show locator with plankton
        /// </summary>
        OnlyPlankton,
        /// <summary>
        /// Only show with child bubbles
        /// </summary>
        OnlyChildBubbles,
        /// <summary>
        /// Only the main bubble
        /// </summary>
        OnlyMainBubble,
        /// <summary>
        /// Always show except with plankton
        /// </summary>
        AnythingButPlankton,
        /// <summary>
        /// Always show except with main bubble
        /// </summary>
        AnythingButMainBubble
    }
}
