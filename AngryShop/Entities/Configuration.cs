using System.ComponentModel;
using AngryShop.Items.Enums;

namespace AngryShop.Entities
{
    /// <summary>
    /// Application configuration stored in Config file
    /// </summary>
    public class Configuration : INotifyPropertyChanged
    {
        #region [ Data ]

        private SortOrderTypes _sortOrderType = SortOrderTypes.ByFrequency;

        /// <summary> Words list sort order </summary>
        public SortOrderTypes SortOrderType
        {
            get { return _sortOrderType; }
            set
            {
                _sortOrderType = value;
                OnPropertyChanged("SortOrderType");
            }
        }


        private bool _sortOrderIsAscending = false;
        /// <summary> Words list sort order is ascending </summary>
        public bool SortOrderIsAscending
        {
            get { return _sortOrderIsAscending; }
            set
            {
                _sortOrderIsAscending = value;
                _sortOrderIsDescending = !value;
                OnPropertyChanged("SortOrderIsAscending");
                OnPropertyChanged("SortOrderIsDescending");
            }
        }

        private bool _sortOrderIsDescending = true;
        /// <summary> Words list sort order is descending </summary>
        public bool SortOrderIsDescending
        {
            get { return _sortOrderIsDescending; }
            set
            {
                _sortOrderIsDescending = value;
                _sortOrderIsAscending = !value;
                OnPropertyChanged("SortOrderIsAscending");
                OnPropertyChanged("SortOrderIsDescending");
            }
        }


        private int _frequencyThreshold = 2;

        /// <summary> Minimum frequency for words to appear on list (default >=2) </summary>
        public int FrequencyThreshold
        {
            get { return _frequencyThreshold; }
            set
            {
                _frequencyThreshold = value;
                OnPropertyChanged("FrequencyThreshold");
            }
        }


        private ListVisibilityTypes _listVisibilityType = ListVisibilityTypes.OnTrayIconClick;

        /// <summary> Words list sort order </summary>
        public ListVisibilityTypes ListVisibilityType
        {
            get { return _listVisibilityType; }
            set
            {
                _listVisibilityType = value;
                OnPropertyChanged("ListVisibilityType");
            }
        }


        private bool _toHideCommonWords = true;

        /// <summary> Allows user to display/hide "common" words </summary>
        public bool ToHideCommonWords
        {
            get { return _toHideCommonWords; }
            set
            {
                _toHideCommonWords = value;
                OnPropertyChanged("ToHideCommonWords");
            }
        }


        private bool _toLaunchOnSystemStart = true;

        /// <summary> Determines if the application launches at startup </summary>
        public bool ToLaunchOnSystemStart
        {
            get { return _toLaunchOnSystemStart; }
            set
            {
                _toLaunchOnSystemStart = value;
                OnPropertyChanged("ToLaunchOnSystemStart");
            }
        }

        private bool _toRestoreClipboard = true;

        /// <summary> Determines if the application restores user clipboard object after replacing text in active textbox </summary>
        public bool ToRestoreClipboard
        {
            get { return _toRestoreClipboard; }
            set
            {
                _toRestoreClipboard = value;
                OnPropertyChanged("ToRestoreClipboard");
            }
        }

        private bool _needsSaving = false;
        /// <summary> Configuration instance needs to be saved </summary>
        public bool NeedsSaving
        {
            get { return _needsSaving; }
            set
            {
                _needsSaving = value;
                OnPropertyChanged("NeedsSaving");
            }
        }



        /// <summary> Main window X position </summary>
        public double? WinPositionX { get; set; }

        /// <summary> Main window Y position </summary>
        public double? WinPositionY { get; set; }


        /// <summary> Main window width </summary>
        public double? WinSizeWidth { get; set; }

        /// <summary> Main window height </summary>
        public double? WinSizeHeight { get; set; }

        #endregion


        
        /// <summary>
        /// Sets all values to default
        /// </summary>
        public void SetToDefaultCommonValues()
        {
            SortOrderType = SortOrderTypes.ByFrequency;
            SortOrderIsAscending = false;
            FrequencyThreshold = 2;
            ListVisibilityType = ListVisibilityTypes.OnTrayIconClick;
            ToHideCommonWords = true;
            ToLaunchOnSystemStart = true;
            ToRestoreClipboard = true;
        }

        #region [ INotifyPropertyChanged members ]

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
                if (propertyName != "NeedsSaving")
                    NeedsSaving = true;
            }
        }

        #endregion

    }
}
