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
using System.Windows.Shapes;

namespace Shaghouri
{
    /// <summary>
    /// Interaction logic for Draw_Rectangle.xaml
    /// </summary>
    public partial class Draw_Rectangle : Window
    {
        public string RectangleName { get; set; }
        public double RectangleWidth { get; set; }
        public double RectangleHeight { get; set; }
        public bool IsConfirmed { get; set; } = false;

        public Draw_Rectangle()
        {
            InitializeComponent();
        }

        public Draw_Rectangle(string name, double width, double height, string unit = "cm") : this()
        {
            Edit_Name.Text = name;
            Edit_Width.Text = width.ToString();
            Edit_Height.Text = height.ToString();
            Unit_Width.Text = unit;
            Unit_Height.Text = unit;
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            RectangleName = Edit_Name.Text;
            double.TryParse(Edit_Width.Text, out double w);
            double.TryParse(Edit_Height.Text, out double h);
            RectangleWidth = w;
            RectangleHeight = h;
            IsConfirmed = true;
            this.Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_Ok_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                btn_Cancel_Click(sender, e);
            }
        }
    }
}
