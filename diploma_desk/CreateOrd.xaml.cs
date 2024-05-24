using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для CreateOrd.xaml
    /// </summary>
    public partial class CreateOrd : Window
    {
        private PapirusEntities1 dbContext;
        private int orderId;

        public CreateOrd(double left, double top)
        {
            InitializeComponent();
            this.Left = left;
            this.Top = top;
            dbContext = new PapirusEntities1(); // Инициализация контекста данных
            CreateOrderWithDummyData(); // Создаем заказ с заглушками при открытии окна
            LoadOrderData();
            // Подписываемся на событие закрытия окна
            Closing += CreateOrd_Closing;
        }
        private void CreateOrderWithDummyData()
        {
            try
            {
                    // Создаем новый заказ с заглушками
                    Order newOrder = new Order
                    {
                        DateOfCreate = DateTime.Now, // Устанавливаем текущую дату
                        StatusID = 1, // Устанавливаем начальный статус заказа
                        UserID = 1,
                        DeadLine = new DateTime(2001, 1, 1)
            };

                    // Добавляем заказ в контекст данных
                    dbContext.Order.Add(newOrder);

                    // Сохраняем изменения в базе данных
                    dbContext.SaveChanges();

                    // Получаем ID созданного заказа
                    orderId = newOrder.IDOrder;  
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при создании заказа: {ex.Message}");
            }
        }
        private void CreateOrd_Closing(object sender, CancelEventArgs e)
        {
            // Проверяем, существует ли заказ
            if (orderId != 0)
            {
                // Находим заказ по его ID
                var order = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                if (order != null)
                {
                    // Проверяем, является ли заказ заглушечным
                    if (IsDummyOrder(order))
                    {
                        // Удаляем заказ из контекста данных
                        dbContext.Order.Remove(order);
                        // Сохраняем изменения в базе данных
                        dbContext.SaveChanges();
                    }
                }
            }
        }
        private void DeleteOrd()
        {
            // Проверяем, существует ли заказ
            if (orderId != 0)
            {
                    // Находим заказ по его ID
                    var order = dbContext.Order.FirstOrDefault(o => o.IDOrder == orderId);
                    if (order != null)
                    {
                         // Проверяем, является ли заказ заглушечным
                         if (IsDummyOrder(order))
                         {
                             // Удаляем заказ из контекста данных
                             dbContext.Order.Remove(order);
                             // Сохраняем изменения в базе данных
                             dbContext.SaveChanges();
                         }
                    }
            }
        }
        private bool IsDummyOrder(Order order)
        {
            // Здесь определяем условие, по которому заказ считается заглушечным.
            // Например, если все поля заказа содержат значения по умолчанию.
            // Пусть, для примера, заказ считается заглушечным, если ID пользователя равен 1,
            // дата создания заказа равна текущей дате, а статус заказа равен 1.

            return order.UserID == 1 &&
                   order.DeadLine == new DateTime(2001, 1, 1) &&
                   order.StatusID == 1;
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
                    //// Отображение информации о заказе в соответствующих элементах управления
                    //ClientNameTextBox.Text = orderInfo.ClientName;
                    //ClientContactTextBox.Text = orderInfo.ClientContact;
                    //DeadlineDatePicker.SelectedDate = orderInfo.Deadline;

                    // Отображение списка товаров заказа
                    ProductsDataGrid.ItemsSource = orderInfo.Products;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading order information: " + ex.Message);
            }
        }
        private bool ValidateOrder()
        {
            // Проверяем, есть ли хотя бы один товар в заказе
            if (!dbContext.Order_Product.Any(op => op.OrderID == orderId))
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
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем экземпляр окна добавления товара, передавая orderId
                AddProductWindow addProductWindow = new AddProductWindow(orderId);
                addProductWindow.ProductAdded += AddProductWindow_ProductAdded;
                addProductWindow.ShowDialog(); // Открываем окно модально
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при открытии окна добавления товара: {ex.Message}");
            }
        }
        private void AddProductWindow_ProductAdded(object sender, EventArgs e)
        {
            // Обновляем данные заказа
            LoadOrderData();
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
        }
        // Метод для проверки наличия текста, а не только спецсимволов
        private bool ContainsValidText(string input)
        {
            // Проверяем, что строка содержит хотя бы одну букву или цифру (латинские или русские буквы)
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"[a-zA-Zа-яА-Я0-9]");
        }
        private void СreateOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем валидность заказа
                if (!ValidateOrder())
                {
                    return; // Прерываем выполнение метода, если заказ не прошел валидацию
                }

                // Очищаем ввод от лишних пробелов
                string clientName = ClientNameTextBox.Text.Trim();
                string clientContact = ClientContactTextBox.Text.Trim();

                // Проверка на наличие спецсимволов без текста
                if (!ContainsValidText(clientName) || !ContainsValidText(clientContact))
                {
                    MessageBox.Show("Имя клиента и контактные данные не должны содержать только специальные символы.");
                    return;
                }

                // Проверяем, заполнены ли поля
                if (string.IsNullOrWhiteSpace(clientName) || string.IsNullOrWhiteSpace(clientContact))
                {
                    MessageBox.Show("Имя клиента и контактные данные не должны быть пустыми или состоять только из пробелов.");
                    return;
                }
                // Проверяем, существует ли уже заказ с такими же данными
                var existingOrder = dbContext.Order.Any(o =>
                    dbContext.User.Any(u =>
                    u.ClientName == ClientNameTextBox.Text &&
                    u.ClientContact == ClientContactTextBox.Text &&
                    o.UserID == u.IDUser) &&
                    o.DeadLine == DeadlineDatePicker.SelectedDate &&
                    (o.StatusID != 2 || o.StatusID != 5));


                if (existingOrder)
                {
                    MessageBox.Show("Такой заказ уже существует.");
                    return;
                }

                // Проверяем, существует ли пользователь с указанным именем и контактом
                User existingUser = dbContext.User.FirstOrDefault(u => u.ClientName == clientName && u.ClientContact == clientContact);

                if (existingUser == null)
                {
                    // Если пользователь не существует, создаем новую запись
                    existingUser = new User
                    {
                        ClientName = clientName,
                        ClientContact = clientContact
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

                    MessageBox.Show("Заказ успешно создан.");
                    OrderWin orderWin = new OrderWin(this.Left, this.Top);
                    orderWin.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Заказ не найден.");
                    DeleteOrd();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при создании заказа: {ex.Message}");
            }
        }
        private void ClearFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистить все поля ввода
            ClientNameTextBox.Text = "";
            ClientContactTextBox.Text = "";
            DeadlineDatePicker.SelectedDate = null;
        }
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            DeleteOrd();
            MainWindow mainWindow = new MainWindow(this.Left, this.Top);
            mainWindow.Show();
            this.Close();
        }

        private void BtnOrd_Click(object sender, RoutedEventArgs e)
        {
            DeleteOrd();
            OrderWin orderWin = new OrderWin(this.Left, this.Top);
            orderWin.Show();
            this.Close();
        }

        private void BtnMyOrd_Click(object sender, RoutedEventArgs e)
        {
            DeleteOrd();
            ManagerOrder managerOrder = new ManagerOrder(this.Left, this.Top);
            managerOrder.Show();
            this.Close();
        }
    }
}
