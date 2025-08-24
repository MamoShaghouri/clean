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
    /// Interaction logic for Rename_Piece.xaml
    /// </summary>
    public partial class Rename_Piece : Window
    {
        public string NewName { get; set; }
        public bool IsConfirmed { get; set; } = false;
        public Rename_Piece(string currentName) : this()
        {
            Edit_Name.Text = currentName;
        }
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            NewName = Edit_Name.Text;
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
                btn_ok_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                btn_Cancel_Click(sender, e);
            }
        }
        public Rename_Piece()
        {
            InitializeComponent();
            btn_ok.Click += btn_ok_Click;
            btn_Cancel.Click += btn_Cancel_Click;
            this.KeyDown += Window_KeyDown;
        }
    }
}
