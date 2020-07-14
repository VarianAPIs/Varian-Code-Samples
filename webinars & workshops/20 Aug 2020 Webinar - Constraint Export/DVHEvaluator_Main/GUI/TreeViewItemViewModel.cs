using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class TreeViewItemViewModel : INotifyPropertyChanged
    {
        #region Data

        static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        readonly ObservableCollection<TreeViewItemViewModel> _children;
        readonly TreeViewItemViewModel _parent;

        bool _isExpanded = false;
        bool _isSelected = false;
        bool _isEnabled = true;
        bool? _isChecked = false;
        Brush _textColor = Brushes.Black;

        #endregion // Data

        #region Constructors

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            _parent = parent;
            _children = new ObservableCollection<TreeViewItemViewModel>();
            _isExpanded = true;

            if (lazyLoadChildren)
                _children.Add(DummyChild);
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors

        #region Presentation Members

        #region Children

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get { return _children; }
        }

        #endregion // Children

        #region HasLoadedChildren

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        #endregion // HasLoadedChildren

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// Selection is disabled
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = false;
                this.OnPropertyChanged("IsSelected");
            }
        }

        #endregion // IsSelected

        #region TextColor

        /// <summary>
        /// Gets/sets text color
        /// </summary>
        public Brush TextColor
        {
            get { return _textColor; }
            set
            {
                if (value != _textColor)
                {
                    _textColor = value;
                    this.OnPropertyChanged("ChangeTextColor");
                }
            }
        }

        #endregion // IsSelected

        #region IsChecked

        /// <summary>
        /// Gets/sets whether the TreeViewItem checkbox
        /// associated with this object is checked.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        // Value setter
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            // Set this
            if (value == _isChecked || !this.IsEnabled)
                return;
            _isChecked = value;

            // Verify that this is expanded
            if (value == true)
                this.IsExpanded = true;

            // Set children
            if (updateChildren && _isChecked.HasValue)
                foreach (TreeViewItemViewModel child in this.Children)
                    child.SetIsChecked(_isChecked, true, false);
            if (Children.Count != 0)
                this.VerifyCheckState();

            // Set parent
            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            // Event notify
            this.OnPropertyChanged("IsChecked");
        }

        // Helper to set parent state
        void VerifyCheckState()
        {
            bool? state = (this.Children.All(x => x.IsChecked == true)) ? true :
                          (this.Children.All(x => x.IsChecked == false)) ? false : (bool?)null;
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsSelected

        #region IsEnabled

        // Gets/sets whether checkbox is enabled. Not enabled if dose isn't calculated
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                // Set this
                if (value == _isEnabled)
                    return;
                _isEnabled = value;

                // Also uncheck and set text to grey if we're un-enabling it.
                if (value == false)
                {
                    this.IsChecked = false;
                    this.TextColor = Brushes.Gray;
                }

                this.OnPropertyChanged("IsEnabled");
            }
        }

        #endregion  // IsEnabled

        #region LoadChildren

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        #endregion // LoadChildren

        #region Parent

        public TreeViewItemViewModel Parent
        {
            get { return _parent; }
        }

        #endregion // Parent

        #endregion // Presentation Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}