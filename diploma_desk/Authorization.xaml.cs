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

namespace diploma_desk
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Login.Text == "" || Password.Password == "")
            {
                MessageBox.Show("Заполните все поля");
            }
            else
            {
                using (PapirusEntities1 db = new PapirusEntities1())
                {
                    Manager manager = db.Manager.Where(x => x.Login == Login.Text && x.Password == Password.Password).FirstOrDefault();
                    if (manager != null)
                    {
                        CurrentManager.Id = manager.IDManager;
                        MainWindow mainWindow = new MainWindow(this.Left, this.Top);
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверные данные для входа");
                    }
                }
            }
        }
    }
}
