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
using Microsoft.Data.SqlClient;

namespace Shaghouri
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=shadat;Integrated Security=True;TrustServerCertificate=True";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("تم الاتصال بقاعدة البيانات shadat بنجاح!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في الاتصال بقاعدة البيانات: " + ex.Message);
                }
            }
        }
    }
}
