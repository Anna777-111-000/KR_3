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
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace K3
{
    public partial class ProductEditWindow : Window
    {
        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=3;Integrated Security=True";
        int? productId;

        public ProductEditWindow()
        {
            InitializeComponent();
        }

        public ProductEditWindow(int id)
        {
            InitializeComponent();
            productId = id;
            Title = "Редактирование продукции";
            txtTitle.Text = "Редактирование продукции";
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT p.Name, pt.ProductType, p.Price, p.Count
                                     FROM Products p
                                     JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID
                                     WHERE p.ProductsID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", productId.Value);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtName.Text = reader["Name"].ToString();
                        txtPrice.Text = reader["Price"].ToString();
                        txtCount.Text = reader["Count"].ToString();

                        // Выбираем нужный тип в ComboBox
                        string type = reader["ProductType"].ToString();
                        for (int i = 0; i < cmbType.Items.Count; i++)
                        {
                            ComboBoxItem item = cmbType.Items[i] as ComboBoxItem;
                            if (item != null && item.Content.ToString() == type)
                            {
                                cmbType.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message,
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация наименования
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Заполните наименование продукции!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // Валидация типа
            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип продукции!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация цены
            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            // Валидация количества
            int count;
            if (!int.TryParse(txtCount.Text, out count) || count < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCount.Focus();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string typeName = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString();

                    // Получаем ProductTypeID
                    int typeId = 1;
                    SqlCommand typeCmd = new SqlCommand("SELECT ProductTypeID FROM ProductTypes WHERE ProductType = @type", conn);
                    typeCmd.Parameters.AddWithValue("@type", typeName);
                    object result = typeCmd.ExecuteScalar();
                    if (result != null) typeId = Convert.ToInt32(result);

                    if (productId.HasValue)
                    {
                        // Редактирование существующей записи
                        string query = "UPDATE Products SET Name=@name, ProductTypeID=@typeId, Price=@price, Count=@count WHERE ProductsID=@id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@typeId", typeId);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@count", count);
                        cmd.Parameters.AddWithValue("@id", productId.Value);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // Добавление новой записи
                        int newId = Convert.ToInt32(new SqlCommand("SELECT ISNULL(MAX(ProductsID),0)+1 FROM Products", conn).ExecuteScalar());
                        string artikle = "ART-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                        string query = "INSERT INTO Products (ProductsID, Artikle, ProductTypeID, Name, Description, Price, Count, ManufacturerID) VALUES(@id, @artikle, @typeId, @name, '', @price, @count, 1)";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", newId);
                        cmd.Parameters.AddWithValue("@artikle", artikle);
                        cmd.Parameters.AddWithValue("@typeId", typeId);
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@count", count);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Данные успешно сохранены!",
                              "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message,
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}