using System;

namespace Plankton.Classes
{
    /// <summary>
    /// Provides callback for handling PlanktonControl.IsInFullScreenMode property changes
    /// </summary>
    /// <param name="sender">The calling obkect</param>
    /// <param name="e">Event arguments describing the event</param>
    public delegate void FullScreenModeChangedEventHandler(object sender, EventArgs e);
}
