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
    /// Логика взаимодействия для AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public event EventHandler ProductAdded;
        private PapirusEntities1 dbContext;
        private int orderId;


        public AddProductWindow(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            dbContext = new PapirusEntities1();
            LoadProductData();
        }
        private void LoadProductData()
        {
            try
            {
                // Заполняем ComboBox всеми товарами из таблицы Product
                ProductComboBox.ItemsSource = dbContext.Product.ToList();
                ProductComboBox.DisplayMemberPath = "Name";
                ProductComboBox.SelectedValuePath = "IDProduct";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading product information: {ex.Message}");
            }
        }
        private void AddProdButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранный товар из комбо-бокса
                dynamic selectedProduct = ProductComboBox.SelectedItem;
                if (selectedProduct != null)
                {
                    // Проверяем, введено ли количество товара
                    if (!string.IsNullOrEmpty(AmountTextBox.Text))
                    {
                        // Получаем количество товара из текстового поля
                        int amount = Convert.ToInt32(AmountTextBox.Text);

                        // Получаем ID выбранного товара
                        int productId = selectedProduct.IDProduct;

                        // Проверяем, есть ли уже такой товар в заказе
                        bool productExistsInOrder = dbContext.Order_Product.Any(op => op.OrderID == orderId && op.ProductID == productId);
                        if (productExistsInOrder)
                        {
                            MessageBox.Show("Данный товар уже есть в заказе.");
                            return;
                        }

                        // Получаем информацию о товаре из базы данных
                        var product = dbContext.Product.FirstOrDefault(p => p.IDProduct == productId);

                        // Проверяем, есть ли указанное количество товара на складе
                        if (product != null && int.Parse(product.TotalAmount) < amount)
                        {
                            MessageBox.Show("Товара на складе недостаточно.");
                            return;
                        }

                        // Создаем новый экземпляр Order_Product
                        Order_Product newOrderProduct = new Order_Product
                        {
                            OrderID = orderId, // Предполагая, что orderId доступен в этом контексте
                            ProductID = productId,
                            Amount = Convert.ToString(amount)
                        };

                        // Добавляем новый товар к заказу в контексте данных
                        dbContext.Order_Product.Add(newOrderProduct);

                        // Сохраняем изменения в базе данных
                        dbContext.SaveChanges();

                        MessageBox.Show("Товар успешно добавлен в заказ.");
                        // Вызываем событие успешного добавления товара
                        ProductAdded?.Invoke(this, EventArgs.Empty);

                        // Закрываем окно добавления товара
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Введите количество товара.");
                    }
                }
                else
                {
                    MessageBox.Show("Выберите товар для добавления.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при добавлении товара в заказ: {ex.Message}");
            }
        }
    }
}
