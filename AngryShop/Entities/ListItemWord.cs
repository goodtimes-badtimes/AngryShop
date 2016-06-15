using System.ComponentModel;
using System.Windows;

namespace AngryShop.Entities
{
    /// <summary>
    /// Item source for Words List Window
    /// </summary>
    class ListItemWord : INotifyPropertyChanged
    {
        private string _word;
        /// <summary> Word string displayed in list </summary>
        public string Word { get { return _word; } set { _word = value; OnPropertyChanged("Word"); } }

        /// <summary> Word string displayed editor input box (when user clicks on item) </summary>
        public string WordEdited { get; set; }

        /// <summary> The number of matches in text </summary>
        public int Count { get; set; }

        private Visibility _editorVisibility = Visibility.Collapsed;
        public Visibility EditorVisibility { get { return _editorVisibility; } set { _editorVisibility = value; OnPropertyChanged("EditorVisibility"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
