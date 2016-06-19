using System.ComponentModel;

namespace AngryShop.Items.Enums
{
    /// <summary> Types of when list is visible </summary>
    public enum ListVisibilityTypes
    {
        /// <summary> Automatically upon focussing on a text input box (default) </summary>
        [Description("On Focus")]
        OnFocus = 0,
        /// <summary> Always enabled as per System Tray icon behaviour </summary>
        [Description("On Tray Icon Click")]
        OnTrayIconClick = 1,
        ///// <summary> Listens for use of key combination </summary>
        //[Description("On Hotkey")]
        //OnHotkey = 2
    }
}
