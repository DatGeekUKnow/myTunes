using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace myTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Link to database
            DataSet musicDataSet = new DataSet();
            musicDataSet.ReadXmlSchema("music.xsd");
            musicDataSet.ReadXml("music.xml");

            // initialize data grid to music located in music.xml
            musicDataGrid.ItemsSource = musicDataSet.Tables["song"].DefaultView;
            playlistListBox.ItemsSource = musicDataSet.Tables["playlist"].AsDataView();
        }

        // https://stackoverflow.com/questions/4662428/how-to-hide-arrow-on-right-side-of-a-toolbar/4662570
        // code used to hide pesky drop down arrow on right side of toolbar
        private void myToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }
    }
}
