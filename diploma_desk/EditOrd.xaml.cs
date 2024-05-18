using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using System.Collections.ObjectModel;

namespace diploma_desk
{
    /// <summary>
    /// Логика взаимодействия для EditOrd.xaml
    /// </summary>
    public partial class EditOrd : Window
    {
        private PapirusEntities1 dbContext;
        private int orderId;
        public EditOrd(int orderId)
        {
            InitializeComponent();
            dbContext = new PapirusEntities1(); // Инициализация контекста данных
            this.orderId = orderId;
            LoadOrderData();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrderData();
        }
        public void LoadOrderData()
        {
            try
            {
                // Загрузка информации о заказе из базы данных по его ID
                var orderInfo = dbContext.Order
                                        .Where(o => o.IDOrder == orderId)
                                        .Select(o => new
                                        {
                                            ClientName = o.User.ClientName,
                                            ClientContact = o.User.ClientContact,
                                            Deadline = o.DeadLine,
                                            Products = o.Order_Product.Select(op => new
                                            {
                                                ProductName = op.Product.Name,
                                                Amount = op.Amount
                                            }).ToList()
                                        }).FirstOrDefault();

                if (orderInfo != null)
                {
                    // Отображение информации о заказе в соответствующих элементах управления
                    ClientNameTextBox.Text = orderInfo.ClientName;
                    ClientContactTextBox.Text = orderInfo.ClientContact;
                    DeadlineDatePicker.SelectedDate = orderInfo.Deadline;

                    // Отображение списка товаров заказа
                    ProductsDataGrid.ItemsSource = orderInfo.Products;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading order information: " + ex.Message);
            }
        }
        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic selectedItem = ProductsDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                // Получаем название товара из выбранного элемента списка
                string productName = selectedItem.ProductName;

                // Здесь вы должны получить идентификатор товара по его названию из базы данных
                int productId = GetProductIdByName(productName);

                // Создаем экземпляр окна изменения товара, передавая идентификатор товара
                EditProductWindow editProductWindow = new EditProductWindow(productId, orderId);
                editProductWindow.ProductUpdated += EditProductWindow_ProductUpdated; // Подписываемся на событие
                editProductWindow.Show();
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования.");
            }
        }
        private void EditProductWindow_ProductUpdated(object sender, EventArgs e)
        {
            LoadOrderData(); // Обновляем информацию
        }
        private int GetProductIdByName(string productName)
        {
            try
            {
                // Создаем экземпляр контекста данных
                using (var dbContext = new PapirusEntities1())
                {
                    // Ищем товар по его названию в базе данных
                    var product = dbContext.Product.FirstOrDefault(p => p.Name == productName);

                    // Если товар найден, возвращаем его идентификатор, иначе возвращаем значение по умолчанию (0)
                    return product != null ? product.IDProduct : 0;
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок доступа к базе данных
                MessageBox.Show("Ошибка при доступе к базе данных: " + ex.Message);
                return 0;
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Находим заказ в базе данных
                var order = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);

                if (order != null)
                {
                    // Удаляем заказ из базы данных
                    dbContext.Order.Remove(order);
                    dbContext.SaveChanges();
                    MessageBox.Show("Заказ успешно удалён");
                    ManagerOrder managerOrder = new ManagerOrder();
                    managerOrder.Show();
                    // Закрываем окно
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Заказ не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при удалении заказа: {ex.Message}");
            }
        }

        private void ClearFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистить все поля ввода
            ClientNameTextBox.Text = "";
            ClientContactTextBox.Text = "";
            DeadlineDatePicker.SelectedDate = null;
        }
        private void ProductArrivedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Находим заказ в базе данных
                var order = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);

                if (order != null)
                {
                    // Проверяем текущий статус заказа
                    if (order.StatusID == 6)
                    {
                        // Меняем статус заказа на "Товар прибыл"
                        order.StatusID = 4;

                        // Сохраняем изменения в базе данных
                        dbContext.SaveChanges();

                        // Возможно, здесь нужно выполнить какие-то дополнительные действия
                        // в случае успешного обновления статуса заказа

                        MessageBox.Show("Статус заказа успешно обновлен.");
                    }
                    else
                    {
                        // Статус заказа не соответствует требуемому для обновления
                        MessageBox.Show("Статус заказа не позволяет обновить его до 'Товар прибыл'.");
                    }
                }
                else
                {
                    MessageBox.Show("Заказ не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при обновлении статуса заказа: {ex.Message}");
            }
        }
        private bool ValidateOrder()
        {
            // Проверяем, есть ли хотя бы один товар в заказе
            if (ProductsDataGrid.Items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в заказ.");
                return false;
            }

            // Проверяем, заполнены ли поля с информацией о клиенте
            if (string.IsNullOrWhiteSpace(ClientNameTextBox.Text) || string.IsNullOrWhiteSpace(ClientContactTextBox.Text))
            {
                MessageBox.Show("Введите имя клиента и контактные данные.");
                return false;
            }

            // Проверяем, выбран ли дедлайн заказа
            if (!DeadlineDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дедлайн заказа.");
                return false;
            }

            return true;
        }
        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем валидность заказа
                if (!ValidateOrder())
                {
                    return; // Прерываем выполнение метода, если заказ не прошел валидацию
                }
                // Проверяем, существует ли пользователь с указанным именем и контактом
                User existingUser = dbContext.User.FirstOrDefault(u => u.ClientName == ClientNameTextBox.Text && u.ClientContact == ClientContactTextBox.Text);

                if (existingUser == null)
                {
                    // Если пользователь не существует, создаем новую запись
                    existingUser = new User
                    {
                        ClientName = ClientNameTextBox.Text,
                        ClientContact = ClientContactTextBox.Text
                    };

                    // Добавляем нового пользователя в контекст данных
                    dbContext.User.Add(existingUser);
                }

                // Получаем информацию о заказе из базы данных
                var order = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);

                if (order != null)
                {
                    // Обновляем информацию о пользователе (клиенте)
                    order.UserID = existingUser.IDUser;

                    // Предполагая, что вам нужно также обновить дедлайн пользователя
                    if (DeadlineDatePicker.SelectedDate.HasValue)
                    {
                        order.DeadLine = DeadlineDatePicker.SelectedDate.Value;
                    }

                    // Сохраняем изменения в базе данных
                    dbContext.SaveChanges();

                    MessageBox.Show("Изменения сохранены успешно.");
                }
                else
                {
                    MessageBox.Show("Заказ не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении изменений: {ex.Message}");
            }
        }
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
                // Создаем экземпляр окна добавления товара, передавая orderId
                AddProductWindow addProductWindow = new AddProductWindow(orderId);
                // Подписываемся на событие успешного добавления товара
                addProductWindow.ProductAdded += AddProductWindow_ProductAdded;
                addProductWindow.ShowDialog(); // Открываем окно модально
        }
        private void AddProductWindow_ProductAdded(object sender, EventArgs e)
        {
            // Обновляем данные заказа
            LoadOrderData();
        }
        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбран ли товар для удаления
            dynamic selectedItem = ProductsDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                try
                {
                    // Получаем название товара из выбранной записи
                    string productName = selectedItem.ProductName;

                    // Получаем ID товара по его названию из базы данных
                    int productId = GetProductIdByName(productName);

                    // Проверяем, получен ли ID товара
                    if (productId != 0)
                    {
                        // Находим запись товара в заказе по ID товара и ID заказа
                        var orderProduct = dbContext.Order_Product.FirstOrDefault(op => op.OrderID == orderId && op.ProductID == productId);

                        if (orderProduct != null)
                        {
                            // Удаляем запись товара из заказа из контекста данных
                            dbContext.Order_Product.Remove(orderProduct);

                            // Сохраняем изменения в базе данных
                            dbContext.SaveChanges();

                            // Обновляем интерфейс для отображения изменений
                            LoadOrderData();
                        }
                        else
                        {
                            MessageBox.Show("Товар не найден в заказе.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось получить ID товара.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при удалении товара из заказа: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления из заказа.");
            }
            // Проверяем, остались ли еще товары в заказе после удаления
            if (!dbContext.Order_Product.Any(op => op.OrderID == orderId))
            {
                // Удаляем заказ, если в нем больше нет товаров
                var order = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                if (order != null)
                {
                    dbContext.Order.Remove(order);
                    dbContext.SaveChanges();
                    // Обновляем интерфейс для отображения изменений
                    LoadOrderData();
                }
                else
                {
                    MessageBox.Show("Заказ не найден.");
                }
            }
        }
        private void BtnCreateOrd_Click(object sender, RoutedEventArgs e)
        {
            CreateOrd createOrd = new CreateOrd();
            createOrd.Show();
            this.Close();
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
