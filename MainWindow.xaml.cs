using System;
using System.Collections.Generic;
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

namespace Shaghouri
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new Create();
            createWindow.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new Settings();
            settingsWindow.Show();
        }

        private void BtnLibraries_Click(object sender, RoutedEventArgs e)
        {
            var librariesWindow = new Libraries();
            librariesWindow.Show();
        }
    }
}
