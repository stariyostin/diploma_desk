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

namespace diploma_desk
{
    /// <summary>
    /// Логика взаимодействия для EditOrd.xaml
    /// </summary>
    public partial class EditOrd : Window
    {
        public EditOrd()
        {
            InitializeComponent();
        }
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void RemoveProductButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnCreateOrd_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnOrd_Click(object sender, RoutedEventArgs e)
        {
            OrderWin orderWin = new OrderWin();
            orderWin.Show();
            this.Close();
        }
        private void BtnMyOrd_Click(object sender, RoutedEventArgs e)
        {
            ManagerOrder managerOrder = new ManagerOrder();
            managerOrder.Show();
            this.Close();
        }
    }
}
