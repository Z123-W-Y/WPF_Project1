using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project.Views.StartViews;


namespace Project.Views.Pages
{
    /// <summary>
    /// StartView.xaml 的交互逻辑
    /// </summary>
    public partial class StartView : UserControl
    {
        public StartView()
        {
            InitializeComponent();
        }
        private void btnMaterial_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnThickness_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyPage = new btnHistorySearchView();
            SystemContent.Content = historyPage;
        }
        private void btnParameter_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnCamera_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnOnline_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
