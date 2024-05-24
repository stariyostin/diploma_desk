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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(double left, double top)
        {
            InitializeComponent();
            this.Left = left;
            this.Top = top;
            using (PapirusEntities1 db = new PapirusEntities1())
            {
                Manager manager = db.Manager.Where(x => x.IDManager == CurrentManager.Id).FirstOrDefault();
                if (manager != null)
                {
                    WelLbl.Content = "Добро пожаловать," + manager.Name + "!";
                }
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateOrd createOrd = new CreateOrd(this.Left, this.Top);
            createOrd.Show();
            this.Close();
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderWin order = new OrderWin(this.Left, this.Top);
            order.Show();
            this.Close();
        }

        private void BtnMyOrd_Click(object sender, RoutedEventArgs e)
        {
            ManagerOrder managerOrder = new ManagerOrder(this.Left, this.Top);
            managerOrder.Show();
            this.Close();
        }
    }
}
