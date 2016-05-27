using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using AngryShop.Items.Enums;
using ContextMenu = System.Windows.Controls.ContextMenu;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace AngryShop.Windows
{
    public partial class MainWindow
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        private bool _textIsReplacedNow;

        public MainWindow()
        {
            InitializeComponent();

            //AllocConsole(); this is needed for testing purposes

            var timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 1)};
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

            AutomationElement focusedElement = null;
            try
            {
                //Console.WriteLine("tick!");
                focusedElement = AutomationElement.FocusedElement;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Strange exceptions at UIAutomationClient.CUIAutomation8Class.GetFocusedElement()
                // We just will try the same action in the next _timer_Tick()
            }
            if (focusedElement == null) return;

            int processId = focusedElement.Current.ProcessId;
            if (processId == DataManager.ThisProcessId)
            {
                //Console.WriteLine("!");
                return;
            }

            
            //Console.WriteLine(focusedElement.Current.ControlType.ProgrammaticName);

            if ("ControlType.Document ControlType.Pane ControlType.Editor".Contains(
                    focusedElement.Current.ControlType.ProgrammaticName))
            {
                var text = focusedElement.GetText();
                if (string.IsNullOrEmpty(text))
                {
                    if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnFocus) Hide();
                    lstItems.ItemsSource = null;
                }
                else
                {
                    if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnFocus) Show();

                    DataManager.LastProcessId = processId;
                    DataManager.LastAutomationClassName = focusedElement.Current.ClassName;

                    //var textShort = text.Length > 50 ? text.Substring(0, 50) : text;
                    //Console.WriteLine(textShort);

                    var listWords = TextHelper.GetListOfUniqueWords(text);
                    if (listWords != null)
                        lstItems.ItemsSource = listWords;
                }
            }
            else
            {
                if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnFocus) Hide();
                lstItems.ItemsSource = null;
            }
        }


        void replaceText(string oldSubstring, string newSubstring)
        {
            _textIsReplacedNow = true; // don't get active window text while we replacing it with our edited one

            Process process = Process.GetProcessById(DataManager.LastProcessId);
            AutomationElement windowElement = AutomationElement.FromHandle(process.MainWindowHandle);
            if (windowElement != null)
            {
                var elementCollection = FindElementFromClassName(windowElement, DataManager.LastAutomationClassName);
                if (elementCollection != null && elementCollection.Count > 0)
                {
                    var element = elementCollection[0];
                    var text = element.GetText();
                    if (!string.IsNullOrEmpty(text))
                    {
                        insertTextUsingUiAutomation(element, TextHelper.GetNewTextForSending(text, oldSubstring, newSubstring));
                    }
                }
            }

            _textIsReplacedNow = false;
        }

        private AutomationElementCollection FindElementFromClassName(AutomationElement targetApp, string className)
        {
            return targetApp.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ClassNameProperty, className));
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
                var txt = (TextBox) sender;
                var grid = txt.Parent as Grid;
                if (grid != null)
                {
                    var border = grid.Children[0] as Border;
                    if (border != null)
                    {
                        var blk = border.Child as TextBlock;
                        if (blk != null)
                        {
                            var oldText = blk.Text;
                            blk.Text = txt.Text;
                            replaceText(oldText, txt.Text);
                        }
                    }
                }
                txt.Visibility = Visibility.Collapsed;
            }
        }


        private void buttonClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }





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
                    throw new ArgumentNullException(
                        "AutomationElement parameter must not be null");

                // A series of basic checks prior to attempting an insertion.
                //
                // Check #1: Is control enabled?
                // An alternative to testing for static or read-only controls 
                // is to filter using 
                // PropertyCondition(AutomationElement.IsEnabledProperty, true) 
                // and exclude all read-only text controls from the collection.
                if (!element.Current.IsEnabled)
                {
                    throw new InvalidOperationException(
                        "The control with an AutomationID of "
                        + element.Current.AutomationId
                        + " is not enabled.\n\n");
                }

                // Check #2: Are there styles that prohibit us 
                //           from sending text to this control?
                if (!element.Current.IsKeyboardFocusable)
                {
                    throw new InvalidOperationException(
                        "The control with an AutomationID of "
                        + element.Current.AutomationId
                        + "is read-only.\n\n");
                }


                // Once you have an instance of an AutomationElement,  
                // check if it supports the ValuePattern pattern.
                object valuePattern = null;

                // Control does not support the ValuePattern pattern 
                // so use keyboard input to insert content.
                //
                // NOTE: Elements that support TextPattern do not support ValuePattern and TextPattern
                //       does not support setting the text of multi-line edit or document controls.
                //       For this reason, text input must be simulated n using one of the following methods.

                if (!element.TryGetCurrentPattern(
                    ValuePattern.Pattern, out valuePattern))
                {
                    // Set focus for input functionality and begin.
                    element.SetFocus();

                    // Pause before sending keyboard input.
                    Thread.Sleep(100);

                    // The curly brace is a special symbol for the SendKeys method,
                    // so we need to escape it by bracketing it with curly brace (treat it like special symbol)
                    var newWholeText = new StringBuilder();
                    foreach (char c in value)
                    {
                        if (c == '{' || c == '}')
                        {
                            newWholeText.Append('{');
                            newWholeText.Append(c);
                            newWholeText.Append('}');
                        }
                        else
                        {
                            newWholeText.Append(c);
                        }
                    }

                    // Delete existing content in the control and insert new content.
                    SendKeys.SendWait("^{HOME}");   // Move to start of control
                    SendKeys.SendWait("^+{END}");   // Select everything
                    SendKeys.SendWait("{DEL}");     // Delete selection

                    SendKeys.SendWait(newWholeText.ToString());
                }
                // Control supports the ValuePattern pattern so we can 
                // use the SetValue method to insert content.
                else
                {
                    // Set focus for input functionality and begin.
                    element.SetFocus();

                    ((ValuePattern)valuePattern).SetValue(value);
                }
            }
            catch (ArgumentNullException exc)
            {
                LogHelper.SaveError(exc);
                MessageBox.Show(exc.Message);

            }
            catch (InvalidOperationException exc)
            {
                LogHelper.SaveError(exc);
                MessageBox.Show(exc.Message);
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
