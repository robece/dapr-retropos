
/* Unmerged change from project 'Terminal.Gui (netstandard2.0)'
Before:
using System;
using NStack;
After:
using NStack;
using System;
*/
using NStack;

namespace Terminal.Gui
{
    /// <summary>
    /// Provides cut, copy, and paste support for the clipboard. 
    /// NOTE: Currently not implemented.
    /// </summary>
    public static class Clipboard
    {
        /// <summary>
        /// 
        /// </summary>
        public static ustring Contents { get; set; }
    }
}
