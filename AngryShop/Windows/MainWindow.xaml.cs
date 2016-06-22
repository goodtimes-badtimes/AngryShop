using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using AngryShop.Entities;
using AngryShop.Helpers;
using AngryShop.Helpers.Extensions;
using AngryShop.Items;
using ContextMenu = System.Windows.Controls.ContextMenu;
using IDataObject = System.Windows.Forms.IDataObject;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace AngryShop.Windows
{
    public partial class MainWindow
    {
        public SomethingHappenedDelegate OnWindowShowing;

        // This is needed for testing and debug
        //[DllImport("Kernel32")]
        //public static extern void AllocConsole();


        /// <summary> We save user clipboard contents here before using clipboard to replace text in active control </summary>
        private static IDataObject _clipboardObj;

        /// <summary> Bool flag for knowing that text replacing operation is executed right now and getting text from active control must not be executed </summary>
        private bool _textIsReplacedNow;

        public MainWindow()
        {
            InitializeComponent();

            //AllocConsole(); // this is needed for testing purposes

            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            timer.Tick += _timer_Tick;
            timer.Start();
            MouseMove += previewMouseMove;

            if (DataManager.Configuration.WinPositionX != null && DataManager.Configuration.WinPositionY != null)
            {
                Left = DataManager.Configuration.WinPositionX.Value;
                Top = DataManager.Configuration.WinPositionY.Value;
            }
            else
            {
                Rect workAreaRectangle = SystemParameters.WorkArea;
                Left = workAreaRectangle.Right - Width - BorderThickness.Right - 10;
                Top = workAreaRectangle.Bottom - Height - BorderThickness.Bottom - 10;
            }

            if (DataManager.Configuration.WinSizeWidth != null && DataManager.Configuration.WinSizeHeight != null)
            {
                Width = DataManager.Configuration.WinSizeWidth.Value;
                Height = DataManager.Configuration.WinSizeHeight.Value;
            }
        }

        /// <summary>  WPF-styled wndProc function to receive PInvoke calls </summary>
        private IntPtr wndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WinApiHelper.WM_SHOWFIRSTINSTANCE)
            {
                ShowWindow();
                handled = true;
            }
            return IntPtr.Zero;
        }


        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Disable maximize functionality through the WinAPI call
            var hwnd = new WindowInteropHelper((Window)sender).Handle;
            WinApiHelper.DisableMaximizeFunctionality(hwnd);

            // Create WPF-styled wndProc function to receive PInvoke calls
            var source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            if (source != null) source.AddHook(wndProc);
        }


        /// <summary> Timer tick event handler. Gets text from active control, saves this control and process ID, shows words list in main window </summary>
        void _timer_Tick(object sender, EventArgs e)
        {
            if (_textIsReplacedNow) return; // do nothing if edited text is still sending into text input box

            try
            {
                //Console.WriteLine("tick!");
                AutomationElement focusedElement = AutomationElement.FocusedElement;
                if (focusedElement == null) return;


                int processId = focusedElement.Current.ProcessId;
                if (processId == DataManager.ThisProcessId)
                {
                    //Console.WriteLine("!");
                    // Do nothing if we focused on our own applicaton window:
                    return;
                }

                Process process = Process.GetProcessById(processId);
                //Console.WriteLine(@"process.ProcessName: {0}", process.ProcessName);
                //Console.WriteLine(@"process.MainWindowHandle: {0}", process.MainWindowHandle);
                //Console.WriteLine(@"process.MainWindowTitle: {0}", process.MainWindowTitle);
                //Console.WriteLine(@"ProgrammaticName: {0}", focusedElement.Current.ControlType.ProgrammaticName);
                //Console.WriteLine(@"Name: {0}", focusedElement.Current.Name);
                //Console.WriteLine(@"ClassName: {0}", focusedElement.Current.ClassName);
                //Console.WriteLine(@"AutomationId: {0}", focusedElement.Current.AutomationId);

                if ((process.ProcessName == Constants.InternetExplorerProcessName &&
                     focusedElement.Current.ControlType.ProgrammaticName == "ControlType.Pane") ||
                     (process.ProcessName == Constants.MozillaFirefoxProcessName &&
                     focusedElement.Current.ControlType.ProgrammaticName == "ControlType.Document") ||
                     (process.ProcessName == Constants.GoogleChromeProcessName &&
                     focusedElement.Current.ControlType.ProgrammaticName == "ControlType.Document") ||
                    !"ControlType.Document ControlType.Pane ControlType.Editor ControlType.ComboBox".Contains(
                        focusedElement.Current.ControlType.ProgrammaticName))
                {
                    if (DataManager.Configuration.ToDisplayListOnTextFocus) Close();
                    LstItems.ItemsSource = null;
                }
                else
                {
                    var text = focusedElement.GetText() ?? string.Empty;
                    if (DataManager.Configuration.ToDisplayListOnTextFocus) ShowWindow();

                    DataManager.LastAutomationElement = focusedElement;
                    DataManager.LastAutomationElementText = text;

                    //var textShort = text.Length > 50 ? text.Substring(0, 50) : text;
                    //Console.WriteLine(textShort);

                    var listWords = TextHelper.GetListOfUniqueWords(text);
                    if (listWords != null)
                        LstItems.ItemsSource = listWords;
                }
            }
            catch (Exception)
            {
                // Strange "Element not available" exceptions at:
                // - UIAutomationClient.CUIAutomation8Class.GetFocusedElement()
                // - System.Windows.Automation.AutomationElementInformation.get_ProcessId()
                // - System.Windows.Automation.AutomationElementInformation.get_Name()
                // We just will try the same action in the next _timer_Tick()
            }
        }
        

        /// <summary> Mousedown event handler for list item: show textbox editor for this word </summary>
        private void listItemContent_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var block = sender as TextBlock;
            if (block != null)
            {
                var blk = block;
                var border = blk.Parent as Border;
                if (border != null)
                {
                    var grid = border.Parent as Grid;
                    if (grid != null)
                    {
                        var txt = grid.Children[1] as TextBox;
                        if (txt != null)
                        {
                            txt.IsVisibleChanged += (o, args) =>
                            {
                                if ((bool)args.NewValue)
                                {
                                    Dispatcher.BeginInvoke(
                                    DispatcherPriority.ContextIdle,
                                    new Action(delegate
                                    {
                                        txt.Focus();
                                        txt.CaretIndex = txt.Text.Length;
                                    }));
                                }
                            };
                            txt.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        /// <summary> Word Editor Textbox keydown event handler: replaces edited words in last active cotrol </summary>
        private void partTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox)
            {
                var items = LstItems.ItemsSource as List<ListItemWord>;
                if (items != null)
                {
                    items = items.Where(p => p.Word != p.WordEdited).ToList();

                    _textIsReplacedNow = true; // don't get active window text while we replacing it with our edited one

                    if (!string.IsNullOrEmpty(DataManager.LastAutomationElementText))
                    {
                        insertTextUsingUiAutomation(DataManager.LastAutomationElement, TextHelper.GetNewTextForSending(DataManager.LastAutomationElementText, items));
                    }

                    _textIsReplacedNow = false;
                }
            }
        }

        /// <summary> Context menu item click for adding a word to the common words list </summary>
        private void MenuItemAddToCommonWordsList_OnClick(object sender, RoutedEventArgs e)
        {
            var mnu = sender as System.Windows.Controls.MenuItem;
            if (mnu != null)
            {
                var txtBlck = ((ContextMenu)mnu.Parent).PlacementTarget as TextBlock;
                if (txtBlck != null)
                {
                    var inline = txtBlck.Inlines.FirstInline;
                    var wordToIgnore = new TextRange(inline.ContentStart, inline.ContentEnd).Text;

                    if (!string.IsNullOrEmpty(wordToIgnore))
                    {
                        DataManager.CommonWords.Add(wordToIgnore);
                        DataManager.SaveCommonWords();

                        var list = LstItems.ItemsSource as List<ListItemWord>;
                        if (list != null)
                            list.RemoveAll(liw => liw.Word == wordToIgnore);

                        LstItems.ItemsSource = null;
                        LstItems.ItemsSource = list;
                    }
                }
            }
        }

        /// <summary> Dragging window </summary>
        private void previewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Equals(e.Source, brdMain))
            {
                DragMove();
            }
        }

        /// <summary> Closing window </summary>
        private void buttonClose_OnClick(object sender, RoutedEventArgs e)
        {
            DataManager.Configuration.ToDisplayListOnTextFocus = false;
            Close();
        }

        /// <summary> Activates this window and announces about it by calling OnWindowShowing delegate </summary>
        public void ShowWindow()
        {
            if (IsVisible)
            {
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
            }
            else
            {
                Show();
            }
            if (OnWindowShowing != null) OnWindowShowing();
        }


        /// <summary> Inserts a string into text control of interest </summary>
        /// <param name="element">A text control</param>
        /// <param name="value">The string to be inserted</param>
        private static void insertTextUsingUiAutomation(AutomationElement element, string value)
        {
            try
            {
                // Validate arguments / initial setup
                if (value == null)
                    throw new ArgumentNullException("String parameter must not be null.");

                if (element == null)
                    throw new ArgumentNullException("AutomationElement parameter must not be null");

                // Once you have an instance of an AutomationElement, check if it supports the ValuePattern pattern
                object valuePattern = null;

                bool toSendKeys = false;

                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                {
                    // Control supports the ValuePattern pattern so we can use the SetValue method to insert content.

                    // Set focus for input functionality and begin.
                    try
                    {
                        element.SetFocus();
                        ((ValuePattern)valuePattern).SetValue(value);
                    }
                    catch (Exception)
                    {
                        toSendKeys = true;
                    }
                }
                else
                {
                    toSendKeys = true;
                }

                // Control does not support the ValuePattern pattern so use keyboard input to insert content.
                if (toSendKeys)
                {
                    // Set focus for input functionality and begin.
                    element.SetFocus();

                    if (DataManager.Configuration.ToRestoreClipboard)
                        _clipboardObj = System.Windows.Forms.Clipboard.GetDataObject();
                    System.Windows.Forms.Clipboard.Clear();
                    
                    System.Windows.Forms.Clipboard.SetDataObject(value, true, 10, 100);
                    //System.Windows.Forms.Clipboard.SetData(System.Windows.Forms.DataFormats.Text, value);
                    //System.Windows.Clipboard.SetText(value, TextDataFormat.UnicodeText);

                    // Pause before sending keyboard input.
                    Thread.Sleep(100);

                    // Delete existing content in the control and insert new content.
                    SendKeys.SendWait("{HOME}"); // Move to start of line
                    SendKeys.SendWait("^{HOME}"); // Move to start of control
                    SendKeys.SendWait("+{END}"); // Select till end of line
                    SendKeys.SendWait("^+{END}"); // Select everything
                    SendKeys.SendWait("{DEL}"); // Delete selection

                    //SendKeys.SendWait("^{V}");     // Paste new text
                    SendKeys.SendWait("+{INSERT}"); // Paste new text

                    if (DataManager.Configuration.ToRestoreClipboard)
                    {
                        if (_clipboardObj != null)
                        {
                            Thread.Sleep(100);
                            try
                            {
                                System.Windows.Forms.Clipboard.SetDataObject(_clipboardObj); // restore user clipboard contents
                            }
                            catch
                            {
                            }

                        }
                    }
                }
            }
            catch (Exception exc)
            {
                LogHelper.SaveError(exc);
                #if DEBUG
                MessageBox.Show(exc.Message);
                #endif
            }
        }

        //private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        //{
        //    // Remove the hook for WndProc when our window is closing
        //    IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
        //    HwndSource src = HwndSource.FromHwnd(windowHandle);
        //    src.RemoveHook(new HwndSourceHook(this.wndProc));
        //}

        
    }
}
