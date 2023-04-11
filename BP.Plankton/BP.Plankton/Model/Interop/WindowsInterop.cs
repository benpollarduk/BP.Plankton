using System.Runtime.InteropServices;

namespace BP.Plankton.Model.Interop
{
    /// <summary>
    /// Represents a class for invoking Windows Interop methods
    /// </summary>
    public static class NativeMethods
    {
        #region DLLImports

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        #endregion

        #region Constants

        private const int VkLbutton = 0x01;

        #endregion

        #region StaticMethods

        /// <summary>
        /// Get if the left mouse button is down.
        /// </summary>
        /// <returns>True if the left mouse button is down, else false.</returns>
        public static bool IsLeftMouseButtonDown()
        {
            return IsButtonDown(VkLbutton);
        }

        /// <summary>
        /// Get if a button is down.
        /// </summary>
        /// <param name="key">The key code of the button to check.</param>
        /// <returns>True if the button is down, else false.</returns>
        public static bool IsButtonDown(int key)
        {
            return GetAsyncKeyState(key) < 0;
        }

        #endregion
    }
}