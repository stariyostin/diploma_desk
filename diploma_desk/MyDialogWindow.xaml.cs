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
    /// Логика взаимодействия для MyDialogWindow.xaml
    /// </summary>
    public partial class MyDialogWindow : Window
    {
        public bool IsCanceled { get; private set; } = false;
        public bool IsOutOfStock { get; private set; } = false;
        public MyDialogWindow()
        {
            InitializeComponent();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            Close();
        }

        private void OutOfStockButton_Click(object sender, RoutedEventArgs e)
        {
            IsOutOfStock = true;
            Close();
        }

    }
}
