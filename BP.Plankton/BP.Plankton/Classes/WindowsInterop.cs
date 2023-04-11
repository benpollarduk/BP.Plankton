using System.Runtime.InteropServices;

namespace Plankton.Classes
{
    /// <summary>
    /// Represents a class for invoking Windows Interop methods
    /// </summary>
    public static class NativeMethods
    {
        #region DLLImports

        /// <summary>
        /// Get a key state.
        /// </summary>
        /// <param name="vKey">The key code of the key to get the state for.</param>
        /// <returns>The state of the key.</returns>
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        #endregion

        #region StaticProperties

        /// <summary>
        /// Get the code for the left mouse button.
        /// </summary>
        private const int VK_LBUTTON = 0x01;

        #endregion

        #region StaticMethods

        /// <summary>
        /// Get if the left mouse button is down.
        /// </summary>
        /// <returns>True if the left mouse button is down, else false.</returns>
        public static bool IsLeftMouseButtonDown()
        {
            return IsButtonDown(VK_LBUTTON);
        }

        /// <summary>
        /// Get if a button is down.
        /// </summary>
        /// <param name="key">The keycode of the button to check.</param>
        /// <returns>True if the button is down, else false.</returns>
        public static bool IsButtonDown(int key)
        {
            return GetAsyncKeyState(key) < 0;
        }

        #endregion
    }
}