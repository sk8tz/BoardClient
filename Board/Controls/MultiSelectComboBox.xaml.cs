using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using PropertyChanged;

namespace Board.Controls
{
    using System;

    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBox : UserControl
    {
        private ObservableCollection<Node> _nodeList = new ObservableCollection<Node>();

        public MultiSelectComboBox()
        {
            InitializeComponent();
            _nodeList = new ObservableCollection<Node>();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<Node>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));
        public ObservableCollection<Node> ItemsSource
        {
            get { return (ObservableCollection<Node>)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(string), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));
        public string SelectedItems
        {
            get { return (string)GetValue(SelectedItemsProperty); }
            set
            {
                SetValue(SelectedItemsProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #region costumized event

        public static readonly RoutedEvent PopupClosedRoutedEvent =
            EventManager.RegisterRoutedEvent("PopupClosed", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(MultiSelectComboBox));
        //CLR事件包装
        public event RoutedEventHandler PopupClosed
        {
            add { this.AddHandler(PopupClosedRoutedEvent, value); }
            remove { this.RemoveHandler(PopupClosedRoutedEvent, value); }
        }
        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.Text = string.Empty;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.SelectNodes();
            control.SetText();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedBox = (CheckBox)sender;

            if(clickedBox.Content.ToString() == "ALL")
            {
                if(clickedBox.IsChecked.Value)
                {
                    foreach(Node node in _nodeList)
                    {
                        node.IsSelected = true;
                    }
                }
                else
                {
                    foreach(Node node in _nodeList)
                    {
                        node.IsSelected = false;
                    }
                }

            }
            else
            {
                int selectedCount = _nodeList.Count(s => s.IsSelected && s.Title != "ALL");
                if(selectedCount == _nodeList.Count - 1)
                {
                    var firstOrDefault = _nodeList.FirstOrDefault(i => i.Title == "ALL");
                    if (firstOrDefault != null)
                    {
                        firstOrDefault.IsSelected = true;
                    }
                }
                else
                {
                    var orDefault = _nodeList.FirstOrDefault(i => i.Title == "ALL");
                    if (orDefault != null)
                    {
                        orDefault.IsSelected = false;
                    }
                }
            }
            SetSelectedItems();
            SetText();

        }
        #endregion

        #region Methods
        private void SelectNodes()
        {
            if (SelectedItems == null)
            {
                return;
            }

            foreach(var node in SelectedItems.Split(','))
            {
                Node nodex = _nodeList.FirstOrDefault(i => i.Id == node);
                if(nodex != null)
                {
                    nodex.IsSelected = true;
                }
            }
        }

        private void SetSelectedItems()
        {
            SelectedItems = string.Empty;
            foreach(Node node in _nodeList)
            {
                if(node.IsSelected && node.Id != "ALL")
                {
                    if(ItemsSource.Count > 0)
                    {
                        SelectedItems += node.Id + ",";
                    }
                }
            }
            SelectedItems = SelectedItems.TrimEnd(',');
        }

        private void DisplayInControl()
        {
            if(_nodeList == null)
            {
                _nodeList = new ObservableCollection<Node>();
            }
            _nodeList.Clear();
            if(ItemsSource.Count > 0)
            {
                _nodeList.Add(new Node { Id = "ALL", Title = "ALL", IsSelected = false });
            }
            foreach(var node in ItemsSource)
            {
                _nodeList.Add(node);
            }
            MultiSelectCombo.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if(SelectedItems != null)
            {
                StringBuilder displayText = new StringBuilder();
                foreach(Node s in _nodeList)
                {
                    if(s.IsSelected == true && s.Title == "ALL")
                    {
                        displayText = new StringBuilder();
                        displayText.Append("ALL");
                        break;
                    }
                    else if(s.IsSelected == true && s.Title != "ALL")
                    {
                        displayText.Append(s.Title);
                        displayText.Append(',');
                    }
                }
                Text = displayText.ToString().TrimEnd(new char[] { ',' });
            }
            // set DefaultText if nothing else selected
            if(string.IsNullOrEmpty(Text))
            {
                Text = string.Empty;
            }
        }


        #endregion

        private void OnPopupClosed(object sender, EventArgs e)
        {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(PopupClosedRoutedEvent, this);
            this.RaiseEvent(routedEventArgs);
        }
    }

    [ImplementPropertyChanged]
    public class Node
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public bool IsSelected { get; set; }
    }
}
