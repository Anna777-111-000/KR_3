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


namespace K3
{
    public partial class PartnerEditWindow : Window
    {
        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=3;Integrated Security=True";
        int? partnerId; // null = режим добавления

        public PartnerEditWindow(int? id = null)
        {
            InitializeComponent();
            partnerId = id;

            if (partnerId.HasValue)
            {
                Title = "Редактирование партнера";
                txtTitle.Text = "Редактирование партнера";
                LoadPartnerData();
            }
        }

        private void LoadPartnerData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT p.PartnerGroupName, pt.PartnerType, p.Raiting, 
                                            p.Addresss, p.DirectorName, p.Phone, p.Email
                                     FROM Partners p
                                     LEFT JOIN PartnerTypes pt ON p.PartnerTypeID = pt.PartnerTypeID
                                     WHERE p.PartnerID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", partnerId.Value);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtName.Text = reader["PartnerGroupName"].ToString();

                        string type = reader["PartnerType"].ToString();
                        foreach (ComboBoxItem item in cmbType.Items)
                        {
                            if (item.Content.ToString() == type)
                            {
                                item.IsSelected = true;
                                break;
                            }
                        }

                        txtRating.Text = reader["Raiting"].ToString();
                        txtAddress.Text = reader["Addresss"].ToString();
                        txtDirector.Text = reader["DirectorName"].ToString();
                        txtPhone.Text = reader["Phone"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
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
            // Валидация
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Заполните наименование партнера!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип партнера!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal rating;
            if (!decimal.TryParse(txtRating.Text, out rating) || rating < 0 || rating > 10)
            {
                MessageBox.Show("Рейтинг должен быть числом от 0 до 10!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtRating.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text) ||
                string.IsNullOrWhiteSpace(txtDirector.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Заполните все обязательные поля (адрес, директор, телефон)!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string typeName = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString();

                    // Получаем PartnerTypeID
                    int typeId = 1;
                    SqlCommand typeCmd = new SqlCommand("SELECT PartnerTypeID FROM PartnerTypes WHERE PartnerType = @type", conn);
                    typeCmd.Parameters.AddWithValue("@type", typeName);
                    object result = typeCmd.ExecuteScalar();
                    if (result != null) typeId = Convert.ToInt32(result);

                    if (partnerId.HasValue)
                    {
                        // Обновление
                        string query = @"UPDATE Partners SET PartnerGroupName=@name, PartnerTypeID=@typeId,
                                         Raiting=@rating, Addresss=@address, DirectorName=@director,
                                         Phone=@phone, Email=@email WHERE PartnerID=@id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@typeId", typeId);
                        cmd.Parameters.AddWithValue("@rating", rating);
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@director", txtDirector.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);
                        cmd.Parameters.AddWithValue("@id", partnerId.Value);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // Добавление
                        string query = @"INSERT INTO Partners (PartnerGroupName, PartnerTypeID, Raiting, 
                                         Addresss, DirectorName, Phone, Email)
                                         VALUES (@name, @typeId, @rating, @address, @director, @phone, @email)";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@typeId", typeId);
                        cmd.Parameters.AddWithValue("@rating", rating);
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@director", txtDirector.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Данные успешно сохранены!",
                              "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message,
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}