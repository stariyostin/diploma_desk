using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Логика взаимодействия для Order.xaml
    /// </summary>
    public class ButtonAvailabilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2 || !(values[0] is string status) || !(values[1] is string buttonType))
                return false;

            if (buttonType == "Confirm")
                return status == "Создан"; // Кнопка "Confirm" доступна, если статус равен "Создан" или "Ожидает поставки"
            else if (buttonType == "Reject")
                return status == "Создан";
            else if (buttonType == "Take")
                return status == "Подтвержден";

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class OrderWin : Window
    {
        private PapirusEntities1 dbContext; // Контекст данных
        public OrderWin()
        {
            InitializeComponent();
            dbContext = new PapirusEntities1(); // Инициализация контекста данных
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var orders = from order in dbContext.Order
                             join user in dbContext.User on order.UserID equals user.IDUser
                             join orderProduct in dbContext.Order_Product on order.IDOrder equals orderProduct.OrderID
                             join product in dbContext.Product on orderProduct.ProductID equals product.IDProduct
                             join manager in dbContext.Manager on order.ManagerID equals manager.IDManager into managerGroup
                             from mgr in managerGroup.DefaultIfEmpty()
                             join status in dbContext.Status on order.StatusID equals status.IDStatus
                             select new
                             {
                                 IDOrder = order.IDOrder,
                                 ClientName = user.ClientName,
                                 ClientContact = user.ClientContact,
                                 ProductName = product.Name,
                                 Amount = orderProduct.Amount,
                                 DateOfCreate = order.DateOfCreate,
                                 Deadline = order.DeadLine,
                                 Status = status.Name,
                                 Manager = mgr != null ? mgr.Name : "None"
                             };

                ordersDataGrid.ItemsSource = orders.ToList(); // Заполнение DataGrid данными
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (ordersDataGrid.SelectedItem != null)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                int orderId = selectedOrder.IDOrder;
                var orderToUpdate = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                if (orderToUpdate != null)
                {
                    orderToUpdate.StatusID = 3;
                    dbContext.SaveChanges();
                    LoadData(); // Обновляем данные после изменения статуса
                }
            }
        }
        //private void RejectButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (ordersDataGrid.SelectedItem != null)
        //    {
        //        dynamic selectedOrder = ordersDataGrid.SelectedItem;
        //        int orderId = selectedOrder.IDOrder;
        //        var orderToUpdate = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
        //        if (orderToUpdate != null)
        //        {
        //            orderToUpdate.StatusID = 2;
        //            orderToUpdate.ManagerID = CurrentManager.Id;
        //            dbContext.SaveChanges();
        //            LoadData(); // Обновляем данные после изменения статуса
        //        }
        //    }
        //}
        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            MyDialogWindow dialog = new MyDialogWindow();
            dialog.Owner = this; // Устанавливаем владельца диалогового окна

            // Отображаем диалоговое окно и ждем его закрытия
            bool? dialogResult = dialog.ShowDialog();

            // Проверяем результат диалога
            if (dialogResult == false)
            {
                if (dialog.IsCanceled)
                {
                    // Выполняем действие "Отменить заказ"
                    ChangeOrderStatus(2);
                }
                else if (dialog.IsOutOfStock)
                {
                    // Выполняем действие "Не хватает товара"
                    ChangeOrderStatus(6);
                }
            }
        }

        private void ChangeOrderStatus(int newStatusId)
        {
            if (ordersDataGrid.SelectedItem != null)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                int orderId = selectedOrder.IDOrder;
                var orderToUpdate = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                if (orderToUpdate != null)
                {
                    orderToUpdate.StatusID = newStatusId;
                    orderToUpdate.ManagerID = CurrentManager.Id;
                    dbContext.SaveChanges();
                    LoadData(); // Обновляем данные после изменения статуса
                }
            }
        }
        private void TakeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ordersDataGrid.SelectedItem != null)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                int orderId = selectedOrder.IDOrder;
                var orderToUpdate = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                if (orderToUpdate != null)
                {
                    orderToUpdate.StatusID = 4;
                    orderToUpdate.ManagerID = CurrentManager.Id;
                    dbContext.SaveChanges();
                    LoadData(); // Обновляем данные после изменения статуса
                }
            }
        }

        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnMyOrd_Click(object sender, RoutedEventArgs e)
        {
            ManagerOrder managerOrder = new ManagerOrder();
            managerOrder.Show();
            this.Close();
        }

        private void BtnCreateOrd_Click(object sender, RoutedEventArgs e)
        {
            CreateOrd createOrd = new CreateOrd();
            createOrd.Show();
            this.Close();
        }
    }
}
