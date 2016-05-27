using System;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Automation.Text;

namespace AngryShop.Helpers.Extensions
{
    public static class AutomationExtensions
    {
        public static string GetText(this AutomationElement element)
        {
            object patternObj;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }
            else
            {
                return element.Current.Name;
            }
        }

        public static string GetSelectedText()
        {
            AutomationElement focusedElement = AutomationElement.FocusedElement;

            object currentPattern = null;

            if (focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out currentPattern))
            {
                TextPattern textPattern = (TextPattern)currentPattern;
                TextPatternRange[] textPatternRanges = textPattern.GetSelection();
                if (textPatternRanges.Length > 0)
                {
                    string textSelection = textPatternRanges[0].GetText(-1);
                    return textSelection;
                }
            }
            return string.Empty;

        }

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, bool wParam, string lParam);

        const int EM_REPLACESEL = 0x00C2;

        public static void SetSelectedText(string value)
        {
            AutomationElement focusedElement = AutomationElement.FocusedElement;
            IntPtr windowHandle = new IntPtr(focusedElement.Current.NativeWindowHandle);
            SendMessage(windowHandle, EM_REPLACESEL, true, value);
        }
    }
}
