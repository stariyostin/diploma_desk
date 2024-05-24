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
    public class BtnManOrdConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2 || !(values[0] is string status) || !(values[1] is string buttonType))
                return false;

            if (buttonType == "Ready")
                return status == "В процессе";
            else if (buttonType == "Change")
                return status == "Отклонён" || status == "Ожидает поставки" || status == "В процессе";
            else if (buttonType == "Reject")
                return status == "В процессе" || status == "Ожидает поставки";

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Логика взаимодействия для ManagerOrder.xaml
    /// </summary>
    public partial class ManagerOrder : Window
    {
        private PapirusEntities1 dbContext; // Контекст данных
        public ManagerOrder(double left, double top)
        {
            InitializeComponent();
            this.Left = left;
            this.Top = top;
            dbContext = new PapirusEntities1(); // Инициализация контекста данных
            LoadClientsForManager(CurrentManager.Id); // Загрузка клиентов для текущего менеджера
            LoadOrdersForManager(CurrentManager.Id); // Загрузка заказов для текущего менеджера
        }

        private void LoadClientsForManager(int managerId)
        {
            try
            {
                var uniqueClients = dbContext.Order
                    .Where(order => order.ManagerID == managerId)
                    .Select(order => order.User.ClientName)
                    .Distinct()
                    .ToList();

                ClientFilterComboBox.Items.Clear();
                ClientFilterComboBox.Items.Add("Все клиенты"); // Добавляем элемент "Все клиенты" в начало списка
                foreach (var client in uniqueClients)
                {
                    ClientFilterComboBox.Items.Add(client);
                }
                ClientFilterComboBox.SelectedIndex = 0; // Устанавливаем выбранным первый элемент по умолчанию
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading clients: " + ex.Message);
            }
        }

        private void LoadOrdersForManager(int managerId)
        {
            try
            {
                string selectedClient = ClientFilterComboBox.SelectedItem?.ToString();

                var ordersQuery = from order in dbContext.Order
                                  where order.ManagerID == managerId
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

                if (!string.IsNullOrEmpty(selectedClient) && selectedClient != "Все клиенты")
                {
                    ordersQuery = ordersQuery.Where(order => order.ClientName == selectedClient);
                }

                ordersDataGrid.ItemsSource = ordersQuery.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void ClientFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOrdersForManager(CurrentManager.Id);
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ClientFilterComboBox.SelectedIndex = 0; // Сбрасываем выбранный элемент в комбо-боксе
            LoadOrdersForManager(CurrentManager.Id); // Перезагружаем все заказы
        }


        private void ButtonReady_Click(object sender, RoutedEventArgs e)
        {
            if (ordersDataGrid.SelectedItem != null)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                int orderId = selectedOrder.IDOrder;
                var orderToUpdate = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                if (orderToUpdate != null)
                {
                    // Получаем список товаров в выбранном заказе
                    var orderProducts = dbContext.Order_Product.Where(op => op.OrderID == orderId);

                    foreach (var orderProduct in orderProducts)
                    {
                        // Получаем информацию о товаре из таблицы Product
                        var product = dbContext.Product.FirstOrDefault(p => p.IDProduct == orderProduct.ProductID);
                        if (product != null)
                        {
                            // Вычитаем количество товара в заказе из TotalAmount на складе
                            int TotalAmount = int.Parse(product.TotalAmount);
                            int Amount = int.Parse(orderProduct.Amount);
                            TotalAmount -= Amount;
                            product.TotalAmount = Convert.ToString(TotalAmount);
                        }
                    }
                    orderToUpdate.StatusID = 5;
                    orderToUpdate.ManagerID = CurrentManager.Id;
                    dbContext.SaveChanges();
                    LoadOrdersForManager(CurrentManager.Id); // Обновляем данные после изменения статуса
                }
            }
        }
        private void ButtonReject_Click(object sender, RoutedEventArgs e)
        {
            if (ordersDataGrid.SelectedItem != null)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                int orderId = selectedOrder.IDOrder;
                var orderToUpdate = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                if (orderToUpdate != null)
                {
                    orderToUpdate.StatusID = 3;
                    orderToUpdate.ManagerID = null;
                    dbContext.SaveChanges();
                    LoadOrdersForManager(CurrentManager.Id); // Обновляем данные после изменения статуса
                }
            }
        }

        private void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            if (ordersDataGrid.SelectedItem != null)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                int orderId = selectedOrder.IDOrder;

                // Создаем экземпляр окна изменения заказа, передавая ID заказа
                EditOrd editOrderWindow = new EditOrd(orderId, this.Left, this.Top);

                // Открываем окно изменения заказа
                editOrderWindow.Show();
                this.Close();
            }
        }

        private void BtnCreateOrd_Click(object sender, RoutedEventArgs e)
        {
            CreateOrd createOrd = new CreateOrd(this.Left, this.Top);
            createOrd.Show();
            this.Close();
        }

        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(this.Left, this.Top);
            mainWindow.Show();
            this.Close();
        }

        private void BtnOrd_Click(object sender, RoutedEventArgs e)
        {
            OrderWin orderWin = new OrderWin(this.Left, this.Top);
            orderWin.Show();
            this.Close();
        }
    }
}
