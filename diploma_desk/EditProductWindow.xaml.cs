using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для EditProductWindow.xaml
    /// </summary>
    public partial class EditProductWindow : Window
    {
        public event EventHandler ProductUpdated;
        private PapirusEntities1 dbContext;
        private int productId;
        private int orderId;

        public EditProductWindow(int productId, int orderId)
        {
            InitializeComponent();
            dbContext = new PapirusEntities1(); // Инициализация контекста данных
            this.productId = productId;
            this.orderId = orderId; // Присваиваем orderId
            LoadProductData();
        }

        private void LoadProductData()
        {
            try
            {
                // Получаем информацию о выбранном товаре из базы данных Order_Product
                var productInfo = dbContext.Order_Product.FirstOrDefault(op => op.Product.IDProduct == productId && op.Order.IDOrder == orderId);

                if (productInfo != null)
                {
                    // Получаем название товара и его количество в заказе
                    var productName = productInfo.Product.Name;
                    var amount = productInfo.Amount;

                    // Заполняем ComboBox всеми товарами из таблицы Product
                    ProductComboBox.ItemsSource = dbContext.Product.ToList();
                    ProductComboBox.DisplayMemberPath = "Name";
                    ProductComboBox.SelectedValuePath = "IDProduct";
                    ProductComboBox.SelectedValue = productInfo.Product.IDProduct;

                    // Заполняем TextBox количеством товара из заказа
                    AmountTextBox.Text = amount.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading product information: " + ex.Message);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранный товар из комбобокса
                Product selectedProductFromComboBox = ProductComboBox.SelectedItem as Product;

                if (selectedProductFromComboBox == null)
                {
                    MessageBox.Show("Выберите товар.");
                    return;
                }

                // Получаем новое количество товара из текстового поля
                int newAmount;
                if (!int.TryParse(AmountTextBox.Text, out newAmount))
                {
                    MessageBox.Show("Введено некорректное значение количества товара.");
                    return;
                }

                // Проверяем, что новое количество не превышает количество товара на складе
                if (newAmount > Convert.ToInt32(selectedProductFromComboBox.TotalAmount))
                {
                    MessageBox.Show("Товара на складе меньше необходимого количества.");
                    return;
                }

                // Находим запись в таблице Order_Product, которая соответствует выбранному товару и заказу
                Order_Product orderProduct = dbContext.Order_Product.FirstOrDefault(op => op.Product.IDProduct == selectedProductFromComboBox.IDProduct && op.Order.IDOrder == orderId);

                // Обновляем количество товара в заказе
                if (orderProduct != null)
                {
                    orderProduct.Amount = Convert.ToString(newAmount);
                }
                else
                {
                    MessageBox.Show("Не удалось найти товар в заказе.");
                    return;
                }

                // Сохраняем изменения в базе данных
                dbContext.SaveChanges();
                // вызов события после успешного сохранения
                ProductUpdated?.Invoke(this, EventArgs.Empty);

                // Закрываем окно
                this.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении изменений: {ex.Message}");
            }
        }

    }
}
