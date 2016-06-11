using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using AngryShop.Helpers;
using AngryShop.Helpers.Extensions;
using AngryShop.Items;
using AngryShop.Items.Enums;
using ContextMenu = System.Windows.Controls.ContextMenu;
using IDataObject = System.Windows.Forms.IDataObject;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using TextDataFormat = System.Windows.TextDataFormat;

namespace AngryShop.Windows
{
    public partial class MainWindow
    {
        //[DllImport("Kernel32")]
        //public static extern void AllocConsole();

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
                    if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnFocus) Hide();
                    lstItems.ItemsSource = null;
                }
                else
                {
                    var text = focusedElement.GetText() ?? string.Empty;
                    if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnFocus) Show();

                    DataManager.LastAutomationElement = focusedElement;

                    //var textShort = text.Length > 50 ? text.Substring(0, 50) : text;
                    //Console.WriteLine(textShort);

                    var listWords = TextHelper.GetListOfUniqueWords(text);
                    if (listWords != null)
                        lstItems.ItemsSource = listWords;
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


        void replaceText(List<ListItemWord> items)
        {
            _textIsReplacedNow = true; // don't get active window text while we replacing it with our edited one

            var element = DataManager.LastAutomationElement;
            var text = element.GetText();
            if (!string.IsNullOrEmpty(text))
            {
                insertTextUsingUiAutomation(element, TextHelper.GetNewTextForSending(text, items));
            }

            _textIsReplacedNow = false;
        }

        private AutomationElementCollection FindElementFromClassName(AutomationElement targetApp, string className)
        {
            return targetApp.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.NameProperty, className));
        }



        private void previewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Equals(e.Source, brdMain))
            {
                DragMove();
            }
        }


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

        private void partTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox)
            {
                var items = lstItems.ItemsSource as List<ListItemWord>;
                if (items != null)
                {
                    replaceText(items.Where(p => p.Word != p.WordEdited).ToList());
                }
                //var txt = (TextBox) sender;
                //var grid = txt.Parent as Grid;
                //if (grid != null)
                //{
                //    var border = grid.Children[0] as Border;
                //    if (border != null)
                //    {
                //        var blk = border.Child as TextBlock;
                //        if (blk != null)
                //        {
                //            var oldText = blk.Text;
                //            blk.Text = txt.Text;
                //            replaceText(oldText, txt.Text);
                //        }
                //    }
                //}
                //txt.Visibility = Visibility.Collapsed;
            }
        }


        private void buttonClose_OnClick(object sender, RoutedEventArgs e)
        {
            DataManager.Configuration.ListVisibilityType = ListVisibilityTypes.OnTrayIconClick;
            Close();
        }



        private static IDataObject _clipboardObj;

        /// <summary>
        /// Inserts a string into each text control of interest
        /// </summary>
        /// <param name="element">A text control</param>
        /// <param name="value">The string to be inserted</param>
        private static void insertTextUsingUiAutomation(AutomationElement element, string value)
        {
            try
            {
                // Validate arguments / initial setup
                if (value == null)
                    throw new ArgumentNullException(
                        "String parameter must not be null.");

                if (element == null)
                    throw new ArgumentNullException("AutomationElement parameter must not be null");

                // Once you have an instance of an AutomationElement,  
                // check if it supports the ValuePattern pattern.
                object valuePattern = null;

                // Control does not support the ValuePattern pattern 
                // so use keyboard input to insert content.
                //
                // NOTE: Elements that support TextPattern do not support ValuePattern and TextPattern
                //       does not support setting the text of multi-line edit or document controls.
                //       For this reason, text input must be simulated n using one of the following methods.

                bool toSendKeys = false;

                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                {
                    // Control supports the ValuePattern pattern so
                    // we can use the SetValue method to insert content.

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
                                System.Windows.Forms.Clipboard.SetDataObject(_clipboardObj);
                            }
                            catch
                            {
                            }

                        }

                        //var timer = new System.Windows.Forms.Timer {Interval = 1000};
                        //timer.Tick += (sender, args) =>
                        //{
                        //    // restore previous user clipboard state
                        //    if (_clipboardObj != null)
                        //    {
                        //        try
                        //        {
                        //            System.Windows.Forms.Clipboard.SetDataObject(_clipboardObj);
                        //        }
                        //        catch
                        //        {
                        //        }

                        //    }
                        //    timer.Stop();
                        //};
                        //timer.Start();
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

        private void MenuItemAddToCommonWordsList_OnClick(object sender, RoutedEventArgs e)
        {
            var mnu = sender as System.Windows.Controls.MenuItem;
            if (mnu != null)
            {
                var txtBlck = ((ContextMenu)mnu.Parent).PlacementTarget as TextBlock;
                if (txtBlck != null)
                {
                    string wordToAdd = txtBlck.Text.Trim();
                    if (!string.IsNullOrEmpty(wordToAdd))
                    {
                        DataManager.CommonWords.Add(wordToAdd);
                        DataManager.SaveCommonWords();

                        var list = lstItems.ItemsSource as List<string>;
                        if (list != null)
                            list.Remove(txtBlck.Text);

                        lstItems.ItemsSource = null;
                        lstItems.ItemsSource = list;
                    }
                }
            }
        }
    }
}
