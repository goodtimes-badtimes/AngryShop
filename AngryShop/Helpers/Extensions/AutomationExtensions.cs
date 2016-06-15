using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using System.Windows.Automation.Text;

namespace AngryShop.Helpers.Extensions
{
    public static class AutomationExtensions
    {
        /// <summary> Method tries to get text contents from automation element using 4 different methods </summary>
        /// <param name="element">AutomationElement to get text from</param>
        /// <returns>Text contents or null, if something goes wrong</returns>
        public static string GetText(this AutomationElement element)
        {
            try
            {
                Process process = Process.GetProcessById(element.Current.ProcessId);

                // Used for debug purposes: these lines get all supported patterns of given AutomationElement
                //AutomationPattern[] patterns = element.GetSupportedPatterns();
                //foreach (AutomationPattern pattern in patterns)
                //{
                //    Console.WriteLine("ProgrammaticName: " + pattern.ProgrammaticName);
                //    Console.WriteLine("PatternName: " + Automation.PatternName(pattern));
                //}

                object patternObj;
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
                {
                    var textPattern = (TextPattern)patternObj;
                    return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
                }
                if (GetDocumentTextBytes(process.MainWindowHandle, element.Current.ClassName, out patternObj))
                {
                    return patternObj.ToString();
                }
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
                {
                    var valuePattern = (ValuePattern)patternObj;
                    return valuePattern.Current.Value;
                }
                return element.Current.Name;
            }
            catch (Exception e)
            {
                LogHelper.SaveError(e);
                return null;
            }
        }

        /// <summary> Gets text from control by classname and its parent pointer using Win API methods </summary>
        /// <param name="hwndParent">Parent window (control) pointer</param>
        /// <param name="className">Class name of control</param>
        /// <param name="result">Variable for text store</param>
        /// <returns>Whether operation was successful</returns>
        public static bool GetDocumentTextBytes(IntPtr hwndParent, string className, out object result)
        {
            var childHandle = WinApiHelper.FindWindowEx(hwndParent, IntPtr.Zero, className, null);
            int subsize = WinApiHelper.SendMessage((int) childHandle, WinApiHelper.WM_GETTEXTLENGTH, 0, 0).ToInt32();
            if (subsize > 0)
            {
                var subtitle = new StringBuilder(subsize + 1);
                WinApiHelper.SendMessage(childHandle, WinApiHelper.WM_GETTEXT, subtitle.Capacity, subtitle);
                result = subtitle.ToString();
                return true;
            }
            result = "";
            return false;
        }



        #region Unused methods

        /// <summary> Unused method. Gets only selected text from focused element. Left it for possible necessities. </summary>
        public static string GetSelectedText()
        {
            AutomationElement focusedElement = AutomationElement.FocusedElement;

            object currentPattern;

            if (focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out currentPattern))
            {
                var textPattern = (TextPattern)currentPattern;
                TextPatternRange[] textPatternRanges = textPattern.GetSelection();
                if (textPatternRanges.Length > 0)
                {
                    string textSelection = textPatternRanges[0].GetText(-1);
                    return textSelection;
                }
            }
            return string.Empty;

        }

        /// <summary> Unused method. Changes only selected text in focused element. Left it for possible necessities. </summary>
        public static void SetSelectedText(string value)
        {
            AutomationElement focusedElement = AutomationElement.FocusedElement;
            IntPtr windowHandle = new IntPtr(focusedElement.Current.NativeWindowHandle);
            SendMessage(windowHandle, EM_REPLACESEL, true, value);
        }

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, bool wParam, string lParam);

        const int EM_REPLACESEL = 0x00C2;

        #endregion

    }
}
